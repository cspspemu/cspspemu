using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProduceConsumerBufferStream : Stream
    {
        private MemoryStream _memoryStream;
        private long _consumePosition;
        private long _totalProducePosition = 0;
        private long _totalConsumePosition;

        /// <summary>
        /// 
        /// </summary>
        public ProduceConsumerBufferStream()
        {
            _memoryStream = new MemoryStream();
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
        public override long Length => _memoryStream.Length;

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get => _consumePosition;
            set => throw new NotImplementedException();
        }

        private void TryReduceMemorySize()
        {
            if (_readTransactionStack.Count == 0)
            {
                if (_consumePosition >= 512 * 1024 || _consumePosition >= Length / 2)
                {
                    var newMemoryStream = new MemoryStream();
                    _memoryStream.SliceWithLength(_consumePosition).CopyToFast(newMemoryStream);
                    _consumePosition = 0;
                    _memoryStream = newMemoryStream;
                }
            }
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
            _memoryStream.Position = _consumePosition;
            var readed = _memoryStream.Read(buffer, offset, count);
            _consumePosition = _memoryStream.Position;
            _totalConsumePosition += readed;
            TryReduceMemorySize();
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
            _memoryStream.Position = _memoryStream.Length;
            _memoryStream.Write(buffer, offset, count);
            _totalProducePosition += count;
        }

        internal class ReadTransactionState
        {
            internal long ConsumePosition;
            internal long TotalConsumePosition;
        }

        private readonly Stack<ReadTransactionState> _readTransactionStack = new Stack<ReadTransactionState>();

        /// <summary>
        /// 
        /// </summary>
        public void ReadTransactionBegin()
        {
            _readTransactionStack.Push(new ReadTransactionState()
            {
                ConsumePosition = _consumePosition,
                TotalConsumePosition = _totalConsumePosition,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReadTransactionCommit()
        {
            _readTransactionStack.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReadTransactionRevert()
        {
            var readTransactionState = _readTransactionStack.Pop();
            _consumePosition = readTransactionState.ConsumePosition;
            _totalConsumePosition = readTransactionState.TotalConsumePosition;
        }
    }

    /// <summary>
    ///  @TODO Have to improve performance without allocating memory all the time. Maybe a RingBuffer or so.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProduceConsumeBuffer<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public T[] Items = new T[0];

        /// <summary>
        /// 
        /// </summary>
        public long TotalConsumed { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newBytes"></param>
        public void Produce(T[] newBytes)
        {
            Produce(newBytes, 0, newBytes.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newBytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Produce(T[] newBytes, int offset, int length)
        {
            Items = Items.Concat(newBytes, offset, length);
        }

        /// <summary>
        /// 
        /// </summary>
        public int ConsumeRemaining => Items.Length;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public T[] ConsumePeek(int length)
        {
            length = Math.Min(length, ConsumeRemaining);
            return Items.Slice(0, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public ArraySegment<T> ConsumePeekArraySegment(int length)
        {
            return new ArraySegment<T>(Items, 0, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Consume(T[] buffer, int offset, int length)
        {
            length = Math.Min(length, ConsumeRemaining);
            Array.Copy(Items, 0, buffer, offset, length);
            Items = Items.Slice(length);
            TotalConsumed += length;
            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public T[] Consume(int length)
        {
            length = Math.Min(length, ConsumeRemaining);
            var Return = ConsumePeek(length);
            Items = Items.Slice(length);
            TotalConsumed += length;
            return Return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return Array.IndexOf(Items, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public int IndexOf(T[] sequence)
        {
            for (var n = 0; n <= Items.Length - sequence.Length; n++)
            {
                var found = true;
                for (var m = 0; m < sequence.Length; m++)
                {
                    if (!Items[n + m].Equals(sequence[m]))
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return n;
                }
            }
            return -1;
        }
    }
}