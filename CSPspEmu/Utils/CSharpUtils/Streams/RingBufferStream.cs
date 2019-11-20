using System;
using System.IO;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class RingBufferStream : Stream
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly RingBuffer<byte> RingBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ringBuffer"></param>
        public RingBufferStream(RingBuffer<byte> ringBuffer)
        {
            this.RingBuffer = ringBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// 
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// 
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Length => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
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
            return RingBuffer.Read(buffer, offset, count);
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
            throw new NotImplementedException();
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
        /// <exception cref="OverflowException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var ret = RingBuffer.Write(buffer, offset, count);
            if (ret != count) throw new OverflowException();
        }
    }
}