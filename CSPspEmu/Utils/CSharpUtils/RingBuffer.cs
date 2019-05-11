using System;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public unsafe class ByteRingBufferWrapper
    {
        private byte* _dataPointer;
        private int _dataLength;
        private long _readPosition;
        private long _writePosition;

        private ByteRingBufferWrapper()
        {
            _readPosition = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        public static ByteRingBufferWrapper FromPointer(byte* pointer, int dataLength)
        {
            return new ByteRingBufferWrapper
            {
                _dataPointer = pointer,
                _dataLength = dataLength,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public int Capacity => _dataLength;

        /// <summary>
        /// 
        /// </summary>
        public long ReadAvailable => _writePosition - _readPosition;

        /// <summary>
        /// 
        /// </summary>
        public long WriteAvailable => Capacity - ReadAvailable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="OverflowException"></exception>
        public void Write(byte item)
        {
            if (WriteAvailable <= 0) throw (new OverflowException("RingBuffer is full"));
            _dataPointer[_writePosition++ % Capacity] = item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OverflowException"></exception>
        public byte Read()
        {
            if (ReadAvailable <= 0) throw (new OverflowException("RingBuffer is empty"));
            return _dataPointer[_readPosition++ % Capacity];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Write(byte[] transferData, int offset = 0, int length = -1)
        {
            if (length == -1) length = _dataLength - offset;
            length = Math.Min(length, (int) WriteAvailable);
            var transferred = 0;
            while (length-- > 0)
            {
                Write(transferData[offset++]);
                transferred++;
            }
            return transferred;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(byte[] transferData, int offset = 0, int length = -1)
        {
            if (length == -1) length = _dataLength - offset;
            length = Math.Min(length, (int) ReadAvailable);
            var transferred = 0;
            while (length-- > 0)
            {
                transferData[offset++] = Read();
                transferred++;
            }
            return transferred;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T>
    {
        private T[] Data;
        private long _readPosition;
        private long _writePosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        public RingBuffer(int capacity)
        {
            Data = new T[capacity];
        }

        /// <summary>
        /// 
        /// </summary>
        public int Capacity => Data.Length;

        /// <summary>
        /// 
        /// </summary>
        public long ReadAvailable => _writePosition - _readPosition;

        /// <summary>
        /// 
        /// </summary>
        public long WriteAvailable => Capacity - ReadAvailable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="OverflowException"></exception>
        public void Write(T item)
        {
            if (WriteAvailable <= 0) throw (new OverflowException("RingBuffer is full"));
            Data[_writePosition++ % Capacity] = item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OverflowException"></exception>
        public T Read()
        {
            if (ReadAvailable <= 0) throw (new OverflowException("RingBuffer is empty"));
            return Data[_readPosition++ % Capacity];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Write(T[] transferData, int offset = 0, int length = -1)
        {
            if (length == -1) length = Data.Length - offset;
            length = Math.Min(length, (int) WriteAvailable);
            var transferred = 0;
            while (length-- > 0)
            {
                Write(transferData[offset++]);
                transferred++;
            }
            return transferred;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(T[] transferData, int offset = 0, int length = -1)
        {
            if (length == -1) length = Data.Length - offset;
            length = Math.Min(length, (int) ReadAvailable);
            var transferred = 0;
            while (length-- > 0)
            {
                transferData[offset++] = Read();
                transferred++;
            }
            return transferred;
        }
    }
}