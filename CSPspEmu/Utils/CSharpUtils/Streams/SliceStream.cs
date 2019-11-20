using System;
using System.IO;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// Class to create a SliceStream that will slice a base Stream with its own cursor.
    /// </summary>
    /// <example>
    /// var File = File.OpenRead("file.txt");
    /// var SliceFile = nSliceStream.CreateWithBounds(File, 10, 20);
    /// </example>
    public class SliceStream : ProxyStream
    {
        /// <summary>
        /// Cursor for this SliceStream.
        /// </summary>
        protected long ThisPosition;

        /// <summary>
        /// Start offset of this SliceStream relative to ParentStream.
        /// </summary>
        protected long ThisStart;

        /// <summary>
        /// Length of this SliceStream.
        /// </summary>
        protected long ThisLength;

        //public long SliceBoundLow { get { return ThisStart; } }
        //public long SliceLength { get { return ThisLength; } }
        //public long SliceBoundHigh { get { return ThisStart + ThisLength; } }

        /// <summary>
        /// 
        /// </summary>
        public long SliceLength => ThisLength;

        /// <summary>
        /// 
        /// </summary>

        public long SliceLow => ThisStart;

        /// <summary>
        /// 
        /// </summary>

        public long SliceHigh => ThisStart + ThisLength;

        /// <summary>
        /// Creates a SliceStream specifying a start offset and a length.
        /// </summary>
        /// <param name="baseStream">Base Stream</param>
        /// <param name="thisStart">Starting Offset</param>
        /// <param name="thisLength">Length of the Slice</param>
        /// <param name="canWrite">Determines if the Stream will be writtable.</param>
        /// <returns>A SliceStream</returns>
        public static SliceStream CreateWithLength(Stream baseStream, long thisStart = 0, long thisLength = -1,
            bool? canWrite = null)
        {
            return new SliceStream(baseStream, thisStart, thisLength, canWrite);
        }

        /// <summary>
        /// Creates a SliceStream specifying a start offset and a end offset.
        /// </summary>
        /// <param name="baseStream">Parent Stream</param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="canWrite">Determines if the Stream will be writtable.</param>
        /// <returns>A SliceStream</returns>
        public static SliceStream CreateWithBounds(Stream baseStream, long lowerBound, long upperBound,
            bool? canWrite = null)
        {
            return new SliceStream(baseStream, lowerBound, upperBound - lowerBound, canWrite);
        }

        /// <summary>
        /// Creates a SliceStream specifying a start offset and a length.
        /// Please use CreateWithLength or CreateWithBounds to initialite the object.
        /// </summary>
        /// <param name="baseStream">Base Stream</param>
        /// <param name="thisStart">Starting Offset</param>
        /// <param name="thisLength">Length of the Slice</param>
        /// <param name="canWrite">Determines if the Stream will be writtable.</param>
        /// <param name="allowSliceOutsideHigh"></param>
        /// <returns>A SliceStream</returns>
        protected SliceStream(
            Stream baseStream,
            long thisStart = 0,
            long thisLength = -1,
            bool? canWrite = null,
            bool allowSliceOutsideHigh = true)
            : base(baseStream)
        {
            if (!baseStream.CanSeek) throw new NotImplementedException("ParentStream must be seekable");

            ThisPosition = 0;
            ThisStart = thisStart;
            ThisLength = thisLength == -1 ? baseStream.Length - thisStart : thisLength;

            if (SliceHigh < SliceLow || SliceLow < 0 || SliceHigh < 0 || !allowSliceOutsideHigh &&
                (SliceLow > baseStream.Length || SliceHigh > baseStream.Length))
            {
                throw new InvalidOperationException(
                    $"Trying to SliceStream Parent(Length={baseStream.Length}) Slice({thisStart}-{thisLength})");
            }
        }

        /// <summary>
        /// Gets the length of the SliceStream.
        /// </summary>

        public override long Length => ThisLength;

        /// <summary>
        /// Gets or sets the current cursor for this SliceStream.
        /// </summary>

        public override long Position
        {
            get => ThisPosition;
            set
            {
                if (value < 0) value = 0;
                if (value > Length) value = Length;
                ThisPosition = value;
            }
        }

        /// <summary>
        /// Seeks the SliceStream without altering the original Stream.
        /// </summary>
        /// <param name="offset">Offset to seek</param>
        /// <param name="origin">Origin for the seeking</param>
        /// <returns>Absolute offset after the operation</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            //Console.WriteLine("Seek(offset: {0}, origin: {1})", offset, origin);
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
            //Console.WriteLine("   {0}", Position);
            return Position;
        }

        /// <summary>
        /// Read a chunk from this SliceStream and move its cursor after that chunk.
        /// Only will be able to read inside the bounds of this Slice.
        /// It won't change the ParentStream cursor.
        /// </summary>
        /// <param name="buffer">ByteArray to write to</param>
        /// <param name="offset">Offset of the ByteArray to write to</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>Number of bytes readed</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (ParentStream)
            {
                var parentStreamPositionToRestore = ParentStream.Position;
                ParentStream.Position = ThisStart + Position;
                if (Position + count > Length)
                {
                    count = (int) (Length - Position);
                }
                try
                {
                    //Console.WriteLine("Read(position: {0}, count: {1})", Position, count);
                    return base.Read(buffer, offset, count);
                }
                finally
                {
                    Seek(count, SeekOrigin.Current);
                    ParentStream.Position = parentStreamPositionToRestore;
                }
            }
        }

        /// <summary>
        /// Writes a chunk from this SliceStream and move its cursor after that chunk.
        /// Only will be able to write inside the bounds of this Slice.
        /// It won't change the ParentStream cursor.
        /// </summary>
        /// <param name="buffer">ByteArray to read from</param>
        /// <param name="offset">Offset of the ByteArray to read from</param>
        /// <param name="count">Number of bytes to write</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (ParentStream)
            {
                var parentStreamPositionToRestore = ParentStream.Position;
                ParentStream.Position = ThisStart + Position;
                if (Position + count > Length)
                {
                    //count = (int)(Length - Position);
                    throw new IOException(
                        $"Can't write outside the SliceStream. Trying to Write {count} bytes but only {Length - Position} available.");
                }
                try
                {
                    base.Write(buffer, offset, count);
                }
                finally
                {
                    Seek(count, SeekOrigin.Current);
                    ParentStream.Position = parentStreamPositionToRestore;
                }
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}