using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpUtils.Extensions;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MapStream : Stream
    {
        /// <summary>
        /// 
        /// </summary>
        public class StreamEntry
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly long Position;

            /// <summary>
            /// 
            /// </summary>
            public readonly Stream Stream;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="position"></param>
            /// <param name="stream"></param>
            public StreamEntry(long position, Stream stream)
            {
                this.Position = position;
                this.Stream = stream;
            }

            /// <summary>
            /// 
            /// </summary>
            public long Length => Stream.Length;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"StreamEntry({Position}, {Length}, {Stream})";
            }
        }

        private List<StreamEntry> _StreamEntries;
        private Stream _currentStream;
        private long _currentStreamPosition;
        private long _currentStreamLow;
        private long _currentStreamHigh;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<StreamEntry> StreamEntries => _StreamEntries.AsReadOnly();

        /// <summary>
        /// 
        /// </summary>
        public MapStream()
        {
            _StreamEntries = new List<StreamEntry>();
            _currentStream = null;
            _currentStreamPosition = 0;
            _currentStreamLow = 0;
            _currentStreamHigh = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public MapStream Map(long offset, Stream stream)
        {
            _StreamEntries.Add(new StreamEntry(offset, stream));
            return this;
        }

        /// <summary>
        /// Function that writtes all the mappings into another stream.
        /// Useful for patching a file or memory.
        /// </summary>
        /// <param name="targetStream">Stream to write the mapped contents to</param>
        public void WriteSegmentsToStream(Stream targetStream)
        {
            foreach (var streamEntry in _StreamEntries)
            {
                var sourceSliceStream = streamEntry.Stream.SliceWithLength(0, streamEntry.Length);
                var targetSliceStream = targetStream.SliceWithLength(streamEntry.Position, streamEntry.Length);
                sourceSliceStream.CopyToFast(targetSliceStream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// 
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 
        /// </summary>
        public override void Flush()
        {
            foreach (var streamEntry in _StreamEntries) streamEntry.Stream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Length
        {
            get { return _StreamEntries.Max(streamEntry => streamEntry.Position + streamEntry.Length); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private void _PrepareCurrentStream()
        {
            // Cached!
            if (Position >= _currentStreamLow && Position < _currentStreamHigh)
            {
                _currentStreamPosition = Position - _currentStreamLow;
            }
            // Not Cached!
            else
            {
                foreach (var streamEntry in _StreamEntries)
                {
                    if ((Position >= streamEntry.Position) && (Position < streamEntry.Position + streamEntry.Length))
                    {
                        _currentStream = streamEntry.Stream;
                        _currentStreamLow = streamEntry.Position;
                        _currentStreamHigh = streamEntry.Position + streamEntry.Length;
                        _currentStreamPosition = Position - streamEntry.Position;
                        return;
                    }
                }

                _currentStream = null;
                _currentStreamPosition = 0;
                _currentStreamLow = 0;
                _currentStreamHigh = 0;

                //_Position
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetAvailableBytesOnCurrentStream() => _currentStream.Length - _currentStreamPosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private long _Transfer(byte[] buffer, long offset, long count, Func<byte[], long, long, long> method)
        {
            if (count == 0) return 0;

            _PrepareCurrentStream();
            if (_currentStream == null)
                throw new InvalidOperationException($"Invalid/Unmapped MapStream position {Position}");

            var availableCount = GetAvailableBytesOnCurrentStream();

            if (count > availableCount)
            {
                // Read from current Stream.
                var readed1 = _Transfer(buffer, offset, availableCount, method);

                if (readed1 == 0)
                {
                    return 0;
                }
                // Try to read from the next Stream.
                var readed2 = _Transfer(buffer, offset + availableCount, count - availableCount, method);

                return readed1 + readed2;
            }

            long actualReaded = 0;

            //_PrepareCurrentStream();
            _currentStream.PreservePositionAndLock(() =>
            {
                _currentStream.Position = _currentStreamPosition;
                actualReaded = method(buffer, offset, count);
            });

            Position += actualReaded;

            return actualReaded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return (int) _Transfer(buffer, offset, count,
                (buf, off, cnt) => _currentStream.Read(buf, (int) off, (int) cnt));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _Transfer(buffer, offset, count, (buf, off, cnt) =>
            {
                _currentStream.Write(buf, (int) off, (int) cnt);
                return cnt;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        
         /// <summary>
        /// 
        /// </summary>
        /// <param name="targetStream"></param>
        public void Serialize(Stream targetStream)
        {
            // Magic
            targetStream.WriteString("MAPS");

            // Version
            targetStream.WriteVariableUlongBit8Extends(1);

            // ListEntryCount
            targetStream.WriteVariableUlongBit8Extends((ulong) _StreamEntries.Count);

            //ulong FileOffset = 0;

            // EntryHeaders
            foreach (var streamEntry in _StreamEntries)
            {
                // EntryFileOffset
                //TargetStream.WriteVariableUlongBit8Extends(FileOffset);

                // EntryMapOffset
                targetStream.WriteVariableUlongBit8Extends((ulong) streamEntry.Position);

                // EntryLength
                targetStream.WriteVariableUlongBit8Extends((ulong) streamEntry.Length);
            }

            // EntryContents
            foreach (var streamEntry in _StreamEntries)
            {
                streamEntry.Stream.SliceWithLength(0, streamEntry.Length).CopyToFast(targetStream);
            }

            targetStream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetStream"></param>
        /// <returns></returns>
        public static MapStream Unserialize(Stream targetStream)
        {
            if (targetStream.ReadString(4) != "MAPS")
                throw (new InvalidDataException("Not a MapStream serialized stream"));

            var version = targetStream.ReadVariableUlongBit8Extends();
            var mapStream = new MapStream();

            switch (version)
            {
                case 1:
                    var entryCount = (int) targetStream.ReadVariableUlongBit8Extends();
                    var entries = new Tuple<ulong, ulong>[entryCount];
                    for (var n = 0; n < entryCount; n++)
                    {
                        //var EntryFileOffset = TargetStream.ReadVariableUlongBit8Extends();
                        var entryMapOffset = targetStream.ReadVariableUlongBit8Extends();
                        var entryLength = targetStream.ReadVariableUlongBit8Extends();
                        entries[n] = new Tuple<ulong, ulong>(entryMapOffset, entryLength);
                    }

                    foreach (var entry in entries)
                    {
                        var entryMapOffset = entry.Item1;
                        var entryLength = entry.Item2;
                        var entryStream = targetStream.ReadStream((long) entryLength);
                        mapStream.Map((long) entryMapOffset, entryStream);
                    }

                    break;
                default:
                    throw (new InvalidDataException("Unsupported MapStream serialized stream version " + version));
            }

            return mapStream;
        }
    }
}