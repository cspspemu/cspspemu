using System;
using System.IO;
using CSharpUtils.Extensions;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class ConcatStream : Stream
    {
        Stream Stream1;
        Stream Stream2;
        long _position;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream1"></param>
        /// <param name="stream2"></param>
        public ConcatStream(Stream stream1, Stream stream2)
        {
            Stream1 = stream1;
            Stream2 = stream2;
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
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Length => Stream1.Length + Stream2.Length;

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get => _position;
            set => _position = Math.Min(value, Length);
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
            var readed = 0;

            // Just the second stream.
            if (Position >= Stream1.Length)
            {
                Stream2.PreservePositionAndLock(() =>
                {
                    Stream2.Position = Position - Stream1.Length;
                    readed += Stream2.Read(buffer, offset + readed, count);
                });

                Position += readed;
            }
            // On the first stream, and maybe a part of the second.
            else
            {
                if (Position + count > Stream1.Length)
                {
                    var count1 = (int) (Stream1.Length - Position);
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
                case SeekOrigin.End:
                    Position = offset + Length;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
            }
            return Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}