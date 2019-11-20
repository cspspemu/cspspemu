using System;
using System.IO;
using CSharpUtils.Extensions;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamChunker
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly Stream InputStream;

        /// <summary>
        /// 
        /// </summary>
        public readonly byte[] TempBuffer;

        private readonly ProduceConsumeBuffer<byte> _buffer;

        /// <summary>
        /// 
        /// </summary>
        public bool Eof => _buffer.ConsumeRemaining == 0 && InputStream.Eof();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfBytes"></param>
        /// <exception cref="Exception"></exception>
        public void MakeBytesAvailable(int numberOfBytes)
        {
            var bytesToRead = numberOfBytes - _buffer.ConsumeRemaining;
            if (bytesToRead > TempBuffer.Length)
                throw new Exception(
                    "Can't Peek More Bytes Than BufferSize. " + bytesToRead + " > " + TempBuffer.Length);
            if (bytesToRead > 0)
            {
                _buffer.Produce(TempBuffer, 0, InputStream.Read(TempBuffer, 0, bytesToRead));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfBytes"></param>
        /// <returns></returns>
        public byte[] PeekBytes(int numberOfBytes)
        {
            MakeBytesAvailable(numberOfBytes);
            return _buffer.ConsumePeek(numberOfBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfBytes"></param>
        public void SkipBytes(int numberOfBytes)
        {
            _buffer.Consume(numberOfBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="bufferSize"></param>
        public StreamChunker(Stream inputStream, int bufferSize = 4096)
        {
            InputStream = inputStream;
            TempBuffer = new byte[bufferSize];
            _buffer = new ProduceConsumeBuffer<byte>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="maxReaded"></param>
        /// <returns></returns>
        public long SkipUpToSequence(byte[] sequence, long maxReaded = long.MaxValue)
        {
            return CopyUpToSequence(null, sequence, maxReaded);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="maxReaded"></param>
        /// <returns></returns>
        public byte[] GetUpToSequence(byte[] sequence, long maxReaded = long.MaxValue)
        {
            var memoryStream = new MemoryStream();
            CopyUpToSequence(memoryStream, sequence, maxReaded);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="sequence"></param>
        /// <param name="maxReaded"></param>
        /// <returns></returns>
        public long CopyUpToSequence(Stream outputStream, byte[] sequence, long maxReaded = long.MaxValue)
        {
            long totalCopied = 0;

            void Consume(int length)
            {
                if (length <= 0) return;
                var consumedBytes = _buffer.Consume(length);
                outputStream?.Write(consumedBytes, 0, consumedBytes.Length);
                totalCopied += consumedBytes.Length;
            }

            while (true)
            {
                var tempBufferReaded = InputStream.Read(TempBuffer, 0,
                    (int) Math.Min(TempBuffer.Length, maxReaded - totalCopied));
                if (tempBufferReaded > 0)
                {
                    _buffer.Produce(TempBuffer, 0, tempBufferReaded);
                }

                if (_buffer.ConsumeRemaining <= sequence.Length) break;

                var foundIndex = _buffer.IndexOf(sequence);

                // Not Found
                if (foundIndex == -1)
                {
                    Consume(_buffer.ConsumeRemaining - sequence.Length);
                }
                // Found!
                else
                {
                    Consume(foundIndex);
                    // Remove Sequence.
                    _buffer.Consume(sequence.Length);

                    return totalCopied;
                }
            }

            Consume(_buffer.ConsumeRemaining);

            return totalCopied;
        }
    }
}