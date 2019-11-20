using System;
using System.IO;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class ZeroStream : Stream
    {
        protected long Size;
        private long _position;
        protected byte ValueToRepeat;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="valueToRepeat"></param>
        public ZeroStream(long size, byte valueToRepeat = 0x00)
        {
            Size = size;
            _position = 0;
            ValueToRepeat = valueToRepeat;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead => false;

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
        public override long Length => Size;

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get => _position;
            set => _position = value;
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
            var readed = Math.Min(count, (int) (Length - Position));
            for (var n = 0; n < count; n++) buffer[offset + n] = ValueToRepeat;
            Position += readed;
            return readed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
                default: throw new NotImplementedException();
            }
            return Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            Size = value;
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