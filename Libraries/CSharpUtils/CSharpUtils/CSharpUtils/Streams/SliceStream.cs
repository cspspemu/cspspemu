using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

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

        public long SliceLength
        {
            get { return ThisLength; }
        }

        /// <summary>
        /// 
        /// </summary>

        public long SliceLow
        {
            get { return ThisStart; }
        }

        /// <summary>
        /// 
        /// </summary>

        public long SliceHigh
        {
            get { return ThisStart + ThisLength; }
        }

        /// <summary>
        /// Creates a SliceStream specifying a start offset and a length.
        /// </summary>
        /// <param name="BaseStream">Base Stream</param>
        /// <param name="ThisStart">Starting Offset</param>
        /// <param name="ThisLength">Length of the Slice</param>
        /// <param name="CanWrite">Determines if the Stream will be writtable.</param>
        /// <returns>A SliceStream</returns>
        static public SliceStream CreateWithLength(Stream BaseStream, long ThisStart = 0, long ThisLength = -1,
            bool? CanWrite = null)
        {
            return new SliceStream(BaseStream, ThisStart, ThisLength, CanWrite);
        }

        /// <summary>
        /// Creates a SliceStream specifying a start offset and a end offset.
        /// </summary>
        /// <param name="BaseStream">Parent Stream</param>
        /// <param name="ThisStart">Starting Offset</param>
        /// <param name="ThisLength">Length of the Slice</param>
        /// <param name="CanWrite">Determines if the Stream will be writtable.</param>
        /// <returns>A SliceStream</returns>
        static public SliceStream CreateWithBounds(Stream BaseStream, long LowerBound, long UpperBound,
            bool? CanWrite = null)
        {
            return new SliceStream(BaseStream, LowerBound, UpperBound - LowerBound, CanWrite);
        }

        /// <summary>
        /// Creates a SliceStream specifying a start offset and a length.
        /// 
        /// Please use CreateWithLength or CreateWithBounds to initialite the object.
        /// </summary>
        /// <param name="BaseStream">Base Stream</param>
        /// <param name="ThisStart">Starting Offset</param>
        /// <param name="ThisLength">Length of the Slice</param>
        /// <param name="CanWrite">Determines if the Stream will be writtable.</param>
        /// <returns>A SliceStream</returns>
        protected SliceStream(Stream BaseStream, long ThisStart = 0, long ThisLength = -1, bool? CanWrite = null,
            bool AllowSliceOutsideHigh = true)
            : base(BaseStream, CloseParent: false)
        {
            if (!BaseStream.CanSeek) throw(new NotImplementedException("ParentStream must be seekable"));

            this.ThisPosition = 0;
            this.ThisStart = ThisStart;
            this.ThisLength = (ThisLength == -1) ? (BaseStream.Length - ThisStart) : ThisLength;

            bool ErrorCreating = false;

            if ((SliceHigh < SliceLow) || (SliceLow < 0) || (SliceHigh < 0))
            {
                ErrorCreating = true;
            }

            if (!AllowSliceOutsideHigh && ((SliceLow > BaseStream.Length) || (SliceHigh > BaseStream.Length)))
            {
                ErrorCreating = true;
            }

            if (ErrorCreating)
            {
                throw (new InvalidOperationException(String.Format(
                    "Trying to SliceStream Parent(Length={0}) Slice({1}-{2})", BaseStream.Length, ThisStart,
                    ThisLength)));
            }
        }

        /// <summary>
        /// Gets the length of the SliceStream.
        /// </summary>

        public override long Length
        {
            get { return ThisLength; }
        }

        /// <summary>
        /// Gets or sets the current cursor for this SliceStream.
        /// </summary>

        public override long Position
        {
            get { return ThisPosition; }
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
                var ParentStreamPositionToRestore = ParentStream.Position;
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
                    ParentStream.Position = ParentStreamPositionToRestore;
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
                var ParentStreamPositionToRestore = ParentStream.Position;
                ParentStream.Position = ThisStart + Position;
                if (Position + count > Length)
                {
                    //count = (int)(Length - Position);
                    throw (new IOException(String.Format(
                        "Can't write outside the SliceStream. Trying to Write {0} bytes but only {1} available.", count,
                        (Length - Position))));
                }
                try
                {
                    base.Write(buffer, offset, count);
                }
                finally
                {
                    Seek(count, SeekOrigin.Current);
                    ParentStream.Position = ParentStreamPositionToRestore;
                }
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw (new NotImplementedException());
        }
    }
}