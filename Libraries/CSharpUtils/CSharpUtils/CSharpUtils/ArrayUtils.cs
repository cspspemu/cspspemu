using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class ArrayUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IEnumerable<int> Range(int @from, int to)
        {
            for (var n = @from; n < to; n++)
            {
                yield return n;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IEnumerable<int> Range(int to)
        {
            return Range(0, to);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static unsafe T[] CreateArray<T>(void* source, int length)
        {
            var type = typeof(T);
            var sizeInBytes = Marshal.SizeOf(typeof(T));

            var output = new T[length];

            if (type.IsPrimitive)
            {
                // Make sure the array won't be moved around by the GC 
                var handle = GCHandle.Alloc(output, GCHandleType.Pinned);

                var destination = (byte*) handle.AddrOfPinnedObject().ToPointer();
                var byteLength = length * sizeInBytes;

                // There are faster ways to do this, particularly by using wider types or by 
                // handling special lengths.
                for (int i = 0; i < byteLength; i++)
                    destination[i] = ((byte*) source)[i];

                handle.Free();
            }
            else if (type.IsValueType)
            {
                if (!type.IsLayoutSequential && !type.IsExplicitLayout)
                {
                    throw new InvalidOperationException(string.Format("{0} does not define a StructLayout attribute",
                        type));
                }

                //var sourcePtr = new IntPtr(source);

                for (var i = 0; i < length; i++)
                {
                    var p = new IntPtr((byte*) source + i * sizeInBytes);

                    output[i] = (T) Marshal.PtrToStructure(p, typeof(T));
                }
            }
            else
            {
                throw new InvalidOperationException($"{type} is not supported");
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxSize"></param>
        public static void HexDump(byte[] data, int maxSize = -1)
        {
            if (maxSize == -1) maxSize = data.Length;
            var offset = 0;

            Console.WriteLine("");
            while (offset < maxSize)
            {
                var rowCount = Math.Min(maxSize - offset, 16);

                for (var n = 0; n < rowCount; n++)
                {
                    Console.Write("%02X ".Sprintf(data[offset + n]));
                }
                for (var n = 0; n < rowCount; n++)
                {
                    var c = ((char) data[offset + n]);
                    Console.Write("{0}", char.IsControl(c) ? '?' : c);
                }

                Console.WriteLine("");
                offset += rowCount;
            }
        }
    }
}