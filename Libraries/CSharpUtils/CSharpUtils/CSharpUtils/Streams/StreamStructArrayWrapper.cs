using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CSharpUtils.Arrays;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class StreamStructArrayWrapper<TType> : IArray<TType> where TType : struct
    {
        Stream Stream;
        static readonly Type StructType = typeof(TType);

        /// <summary>
        /// 
        /// </summary>
        public readonly int StructSize = Marshal.SizeOf(StructType);

        /// <summary>
        /// 
        /// </summary>
        public StreamStructArrayWrapper()
        {
            Stream = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public StreamStructArrayWrapper(Stream stream)
        {
            Stream = stream;
        }

        private void SeekToIndex(int index)
        {
            Stream.Position = index * StructSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public TType this[int index]
        {
            get
            {
                SeekToIndex(index);
                return Stream.ReadStruct<TType>();
            }
            set
            {
                SeekToIndex(index);
                Stream.WriteStruct(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Length => Stream == null ? 0 : (int) (Stream.Length / StructSize);

        IEnumerator<TType> IEnumerable<TType>.GetEnumerator()
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
        /// <returns></returns>
        public TType[] GetArray() => this.ToArray();
    }
}