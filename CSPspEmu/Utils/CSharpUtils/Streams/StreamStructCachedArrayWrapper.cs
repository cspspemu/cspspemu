using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CSharpUtils.Arrays;
using CSharpUtils.Threading;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class StreamStructCachedArrayWrapper<TType> : IArray<TType> where TType : struct
    {
        readonly List<TType> _cachedValues = new List<TType>();
        readonly int _numberOfItemsToBuffer;
        readonly Stream _stream;
        static readonly Type StructType = typeof(TType);
        private static readonly int StructSize = Marshal.SizeOf(StructType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfItemsToBuffer"></param>
        /// <param name="stream"></param>
        public StreamStructCachedArrayWrapper(int numberOfItemsToBuffer, Stream stream)
        {
            _numberOfItemsToBuffer = numberOfItemsToBuffer;
            _stream = stream;
            CustomThreadPool = new CustomThreadPool(1);
        }

        private int BufferedItemsCount => _cachedValues.Count;

        private void SecureUpToItem(int maxItem)
        {
            maxItem = Math.Min(maxItem, Length);
            var itemsToRead = maxItem - BufferedItemsCount;

            //Console.WriteLine("SecureUpToItem: {0}, {1}, {2}", MaxItem, BufferedItemsCount, ItemsToRead);

            if (itemsToRead > 0)
            {
                try
                {
                    var dataLength = itemsToRead * StructSize;
                    var data = new byte[dataLength];

                    /*
                    Stream.BeginRead(Data, 0, DataLength, (AsyncState) =>
                    {
                        int Readed = Stream.EndRead(AsyncState);
                        CachedValues.AddRange(PointerUtils.ByteArrayToArray<TType>(Data));
                    }, null);
                    */
                    CustomThreadPool.AddTask(0, () =>
                    {
                        var readed = _stream.Read(data, 0, dataLength);
                        _cachedValues.AddRange(PointerUtils.ByteArrayToArray<TType>(data));
                    });
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(exception);
                }
            }
        }

        private void SecureUpToItem(int offset, int numberOfItemsToBuffer)
        {
            if (BufferedItemsCount - offset < numberOfItemsToBuffer / 2)
            {
                while (true)
                {
                    if (BufferedItemsCount - offset < numberOfItemsToBuffer / 2)
                    {
                        SecureUpToItem(BufferedItemsCount + numberOfItemsToBuffer);
                    }

                    if (offset >= BufferedItemsCount)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        break;
                    }
                    //Console.Write("Wait({0}, {1})", Offset, BufferedItemsCount);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TType> GetEnumerator()
        {
            for (var n = 0; n < Length; n++) yield return this[n];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (var n = 0; n < Length; n++) yield return this[n];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public TType this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw(new IndexOutOfRangeException($"Invalid Index {index}. Must be in range {00}-{Length}"));
                SecureUpToItem(index, _numberOfItemsToBuffer);
                return _cachedValues[index];
            }
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Length => (int) (_stream.Length / StructSize);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TType[] GetArray()
        {
            SecureUpToItem(Length);
            return _cachedValues.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomThreadPool CustomThreadPool { get; set; }
    }
}