using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	sealed public partial class MapStream : Stream
	{
		/// <summary>
		/// 
		/// </summary>
		public class StreamEntry
		{
			/// <summary>
			/// 
			/// </summary>
			readonly public long Position;

			/// <summary>
			/// 
			/// </summary>
			readonly public Stream Stream;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="Position"></param>
			/// <param name="Stream"></param>
			public StreamEntry(long Position, Stream Stream)
			{
				this.Position = Position;
				this.Stream = Stream;
			}

			/// <summary>
			/// 
			/// </summary>
			public long Length { get { return Stream.Length; } }

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return String.Format("StreamEntry({0}, {1}, {2})", Position, Length, Stream);
			}
		}

		private List<StreamEntry> _StreamEntries;
		private long _Position;
		private Stream _CurrentStream = null;
		private long _CurrentStreamPosition = 0;
		private long _CurrentStreamLow = 0;
		private long _CurrentStreamHigh = 0;

		/// <summary>
		/// 
		/// </summary>
		public IEnumerable<StreamEntry> StreamEntries
		{
			get
			{
				return _StreamEntries.AsReadOnly();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public MapStream()
		{
			_StreamEntries = new List<StreamEntry>();
			_CurrentStream = null;
			_CurrentStreamPosition = 0;
			_CurrentStreamLow = 0;
			_CurrentStreamHigh = 0;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Offset"></param>
		/// <param name="Stream"></param>
		/// <returns></returns>
		public MapStream Map(long Offset, Stream Stream)
		{
			_StreamEntries.Add(new StreamEntry(Offset, Stream));
			return this;
		}

		/// <summary>
		/// Function that writtes all the mappings into another stream.
		/// Useful for patching a file or memory.
		/// </summary>
		/// <param name="TargetStream">Stream to write the mapped contents to</param>
		public void WriteSegmentsToStream(Stream TargetStream)
		{
			foreach (var StreamEntry in _StreamEntries)
			{
				var SourceSliceStream = StreamEntry.Stream.SliceWithLength(0, StreamEntry.Length);
				var TargetSliceStream = TargetStream.SliceWithLength(StreamEntry.Position, StreamEntry.Length);
				SourceSliceStream.CopyToFast(TargetSliceStream);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool CanRead
		{
			get { return true; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool CanSeek
		{
			get { return true; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Flush()
		{
			foreach (var StreamEntry in _StreamEntries) StreamEntry.Stream.Flush();
		}

		/// <summary>
		/// 
		/// </summary>
		public override long Length
		{
			get
			{
				return _StreamEntries.Max(StreamEntry => StreamEntry.Position + StreamEntry.Length);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override long Position
		{
			get
			{
				return _Position;
			}
			set
			{
				_Position = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void _PrepareCurrentStream()
		{
			// Cached!
			if ((Position >= _CurrentStreamLow) && (Position < _CurrentStreamHigh))
			{
				_CurrentStreamPosition = Position - _CurrentStreamLow;
			}
			// Not Cached!
			else
			{
				foreach (var StreamEntry in _StreamEntries)
				{
					if ((Position >= StreamEntry.Position) && (Position < StreamEntry.Position + StreamEntry.Length))
					{
						_CurrentStream = StreamEntry.Stream;
						_CurrentStreamLow = StreamEntry.Position;
						_CurrentStreamHigh = StreamEntry.Position + StreamEntry.Length;
						_CurrentStreamPosition = Position - StreamEntry.Position;
						return;
					}
				}

				_CurrentStream = null;
				_CurrentStreamPosition = 0;
				_CurrentStreamLow = 0;
				_CurrentStreamHigh = 0;

				//_Position
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected long GetAvailableBytesOnCurrentStream()
		{
			return _CurrentStream.Length - _CurrentStreamPosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Buffer"></param>
		/// <param name="Offset"></param>
		/// <param name="Count"></param>
		/// <param name="Method"></param>
		/// <returns></returns>
		private long _Transfer(byte[] Buffer, long Offset, long Count, Func<byte[], long, long, long> Method)
		{
			if (Count == 0) return 0;

			_PrepareCurrentStream();
			if (_CurrentStream == null) throw (new InvalidOperationException(String.Format("Invalid/Unmapped MapStream position {0}", Position)));

			var AvailableCount = GetAvailableBytesOnCurrentStream();

			if (Count > AvailableCount)
			{
				// Read from current Stream.
				var Readed1 = _Transfer(Buffer, Offset, AvailableCount, Method);

				if (Readed1 == 0)
				{
					return 0;
				}
				else
				{
					// Try to read from the next Stream.
					var Readed2 = _Transfer(Buffer, Offset + AvailableCount, Count - AvailableCount, Method);

					return Readed1 + Readed2;
				}
			}
			else
			{
				long ActualReaded = 0;

				//_PrepareCurrentStream();
				_CurrentStream.PreservePositionAndLock(() =>
				{
					_CurrentStream.Position = _CurrentStreamPosition;
					ActualReaded = Method(Buffer, Offset, Count);
				});

				Position += ActualReaded;

				return ActualReaded;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Buffer"></param>
		/// <param name="Offset"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		public override int Read(byte[] Buffer, int Offset, int Count)
		{
			return (int)_Transfer(Buffer, Offset, Count, (_Buffer, _Offset, _Count) =>
			{
				return _CurrentStream.Read(_Buffer, (int)_Offset, (int)_Count);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Buffer"></param>
		/// <param name="Offset"></param>
		/// <param name="Count"></param>
		public override void Write(byte[] Buffer, int Offset, int Count)
		{
			_Transfer(Buffer, Offset, Count, (_Buffer, _Offset, _Count) =>
			{
				_CurrentStream.Write(_Buffer, (int)_Offset, (int)_Count);
				return _Count;
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
				case SeekOrigin.Begin: Position = offset; break;
				case SeekOrigin.Current: Position = Position + offset; break;
				case SeekOrigin.End: Position = Length + offset; break;
			}
			return offset;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
	}

	sealed public partial class MapStream : Stream
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="TargetStream"></param>
		public void Serialize(Stream TargetStream)
		{
			// Magic
			TargetStream.WriteString("MAPS");

			// Version
			TargetStream.WriteVariableUlongBit8Extends(1);

			// ListEntryCount
			TargetStream.WriteVariableUlongBit8Extends((ulong)_StreamEntries.Count);

			//ulong FileOffset = 0;

			// EntryHeaders
			foreach (var StreamEntry in _StreamEntries)
			{
				// EntryFileOffset
				//TargetStream.WriteVariableUlongBit8Extends(FileOffset);

				// EntryMapOffset
				TargetStream.WriteVariableUlongBit8Extends((ulong)StreamEntry.Position);

				// EntryLength
				TargetStream.WriteVariableUlongBit8Extends((ulong)StreamEntry.Length);
			}

			// EntryContents
			foreach (var StreamEntry in _StreamEntries)
			{
				StreamEntry.Stream.SliceWithLength(0, StreamEntry.Length).CopyToFast(TargetStream);
			}

			TargetStream.Flush();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TargetStream"></param>
		/// <returns></returns>
		static public MapStream Unserialize(Stream TargetStream)
		{
			if (TargetStream.ReadString(4) != "MAPS") throw (new InvalidDataException("Not a MapStream serialized stream"));

			var Version = TargetStream.ReadVariableUlongBit8Extends();
			var MapStream = new MapStream();

			switch (Version)
			{
				case 1:
					var EntryCount = (int)TargetStream.ReadVariableUlongBit8Extends();
					var Entries = new Tuple<ulong, ulong>[EntryCount];
					for (int n = 0; n < EntryCount; n++)
					{
						//var EntryFileOffset = TargetStream.ReadVariableUlongBit8Extends();
						var EntryMapOffset = TargetStream.ReadVariableUlongBit8Extends();
						var EntryLength = TargetStream.ReadVariableUlongBit8Extends();
						Entries[n] = new Tuple<ulong, ulong>(EntryMapOffset, EntryLength);
					}

					foreach (var Entry in Entries)
					{
						var EntryMapOffset = Entry.Item1;
						var EntryLength = Entry.Item2;
						var EntryStream = TargetStream.ReadStream((long)EntryLength);
						MapStream.Map((long)EntryMapOffset, EntryStream);
					}

					break;
				default:
					throw (new InvalidDataException("Unsupported MapStream serialized stream version " + Version));
			}

			return MapStream;
		}
	}
}
