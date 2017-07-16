using System;
using System.IO;

namespace CSharpUtils.Streams
{
    public class ConcatStream : Stream
    {
        Stream Stream1;
        Stream Stream2;
        long _Position;

        public ConcatStream(Stream Stream1, Stream Stream2)
        {
            this.Stream1 = Stream1;
            this.Stream2 = Stream2;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return Stream1.Length + Stream2.Length; }
        }

        public override long Position
        {
            get { return _Position; }
            set { _Position = Math.Min(value, Length); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readed = 0;

            // Just the second stream.
            if (Position >= this.Stream1.Length)
            {
                Stream2.PreservePositionAndLock(() =>
                {
                    Stream2.Position = Position - this.Stream1.Length;
                    readed += Stream2.Read(buffer, offset + readed, count);
                });

                Position += readed;
            }
            // On the first stream, and maybe a part of the second.
            else
            {
                if (Position + count > this.Stream1.Length)
                {
                    int count1 = (int) (this.Stream1.Length - Position);
                    readed += Read(buffer, offset + readed, count1);
                    readed += Read(buffer, offset + readed, count - count1);
                }
                else
                {
                    Stream1.PreservePositionAndLock(() =>
                    {
                        Stream1.Position = Position;
                        readed += Stream1.Read(buffer, offset + readed, count);
                    });

                    Position += readed;
                }
            }

            return readed;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    Position = offset + Length;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}