using System;
using System.IO;

namespace CSharpUtils.Ext.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public sealed unsafe class PointerStream : Stream
    {
        private readonly byte* _startPointer;
        private readonly byte* _endPointer;
        private byte* _currentPointer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="maxLength"></param>
        public PointerStream(byte* pointer, long maxLength = 32 * 1024 * 1024)
        {
            _startPointer = pointer;
            _currentPointer = pointer;
            _endPointer = _startPointer + maxLength;
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
        public override long Length => _endPointer - _startPointer;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public override long Position
        {
            get => _currentPointer - _startPointer;
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
            int readed = 0;
            while (count-- > 0)
            {
                if (_currentPointer >= _endPointer) break;
                buffer[offset++] = *_currentPointer++;
                readed++;
            }
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
        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count-- > 0)
            {
                if (_currentPointer >= _endPointer) break;
                *_currentPointer++ = buffer[offset++];
            }
        }
    }
}