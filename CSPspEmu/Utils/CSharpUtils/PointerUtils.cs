using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpUtils
{
    public unsafe class PointerUtils
    {
        public static string PtrToStringUtf8(byte* pointer) => PtrToString(pointer, Encoding.UTF8);

        public static string PtrToString(byte* pointer, Encoding encoding)
        {
            if (pointer == null) return null;
            var bytes = new List<byte>();
            for (; *pointer != 0; pointer++) bytes.Add(*pointer);
            return encoding.GetString(bytes.ToArray());
        }

        public static ushort PtrToShort_BE(void* ptr)
        {
            var bytes = (byte*) ptr;
            //return (ushort)((bytes[1] << 8) | (bytes[0] << 8));
            return (ushort) ((bytes[0] << 8) | (bytes[1] << 0));
        }

        public static string PtrToString(byte* pointer, int length, Encoding encoding)
        {
            if (pointer == null) return null;
            var bytes = new List<byte>();
            for (var n = 0; n < length; n++)
            {
                if (pointer[n] == 0) break;
                bytes.Add(pointer[n]);
            }
            return encoding.GetString(bytes.ToArray());
        }

        public static void StoreStringOnPtr(string String, Encoding encoding, byte* pointer,
            int pointerMaxLength = 0x10000)
        {
            if (pointer == null) return;
            //if (String == null) return;
            if (String != null)
            {
                var bytes = encoding.GetBytes(String);
                foreach (var Byte in bytes)
                {
                    *pointer++ = Byte;
                }
            }

            *pointer = 0;
        }

        public static void Memset(byte[] array, byte value, int count) => Memset(array, value, 0, count);

        public static void Memset(byte[] array, byte value, int offset, int count)
        {
            if (count <= 0) return;
            if (offset + count > array.Length)
                throw (new InvalidOperationException("Array out of bounts"));
            fixed (byte* arrayPointer = &array[offset])
            {
                Memset(arrayPointer, value, count);
            }
        }

        public static readonly bool Is64 = Environment.Is64BitProcess;

        public static void Memset(byte* pointer, byte value, int count) => new Span<byte>(pointer, count).Fill(value);

        public static void Memcpy(byte* destination, ArraySegment<byte> source)
        {
            if (source.Count > 0)
            {
                fixed (byte* sourcePtr = &source.Array[source.Offset])
                {
                    Memcpy(destination, sourcePtr, source.Count);
                }
            }
        }

        public static void Memcpy(byte* destination, byte[] source, int count)
        {
            fixed (byte* sourcePtr = source)
            {
                Memcpy(destination, sourcePtr, count);
            }
        }

        public static void Memcpy(ArraySegment<byte> destination, byte* source)
        {
            //var Pin = GCHandle.Alloc(Destination.Array, GCHandleType.Pinned);
            //Pin.Free();
            if (destination.Count > 0)
            {
                fixed (byte* destinationPtr = &destination.Array[destination.Offset])
                {
                    //Marshal.UnsafeAddrOfPinnedArrayElement(
                    Memcpy(destinationPtr, source, destination.Count);
                }
            }
        }

        public static void Memcpy(byte[] destination, byte* source, int count)
        {
            fixed (byte* destinationPtr = destination)
            {
                Memcpy(destinationPtr, source, count);
            }
        }

        public static int FindLargestMatch(byte* haystack, byte* needle, int maxLength)
        {
            var match = 0;

            if (Is64)
            {
                while ((maxLength >= 8) && (*(ulong*) haystack == *(ulong*) needle))
                {
                    match += 8;
                    haystack += 8;
                    needle += 8;
                    maxLength -= 8;
                }
            }

            while ((maxLength >= 4) && (*(uint*) haystack == *(uint*) needle))
            {
                match += 4;
                haystack += 4;
                needle += 4;
                maxLength -= 4;
            }

            while ((maxLength >= 1) && (*haystack == *needle))
            {
                match += 1;
                haystack += 1;
                needle += 1;
                maxLength -= 1;
            }

            return match;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="value1"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static int FindLargestMatchByte(byte* haystack, byte value1, int maxLength)
        {
            var match = 0;

            if (maxLength >= 4)
            {
                var value2 = (ushort) ((value1 << 0) | (value1 << 8));
                var value4 = ((uint) value2 << 0) | ((uint) value2 << 16);


                if ((maxLength >= 8) && Is64)
                {
                    var value8 = ((ulong) value4 << 0) | ((ulong) value4 << 32);

                    while ((maxLength >= 8) && (*(ulong*) haystack == value8))
                    {
                        match += 8;
                        haystack += 8;
                        maxLength -= 8;
                    }
                }

                while ((maxLength >= 4) && (*(uint*) haystack == value4))
                {
                    match += 4;
                    haystack += 4;
                    maxLength -= 4;
                }
            }

            while ((maxLength >= 1) && (*haystack == value1))
            {
                match += 1;
                haystack += 1;
                maxLength -= 1;
            }

            return match;
        }

        public static void Memcpy(byte* destination, byte* source, int size) => new Span<byte>(source, size).CopyTo(new Span<byte>(destination, size));

        public static byte[] PointerToByteArray(byte* pointer, int size)
        {
            var data = new byte[size];
            fixed (byte* dataPtr = data)
            {
                Memcpy(dataPtr, pointer, size);
            }
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="action"></param>
        /// <typeparam name="TType"></typeparam>
        public static void GetArrayPointer<TType>(TType[] array, Action<IntPtr> action)
        {
            var dataGc = GCHandle.Alloc(array, GCHandleType.Pinned);
            var dataPointer = dataGc.AddrOfPinnedObject();
            try
            {
                action(dataPointer);
            }
            finally
            {
                dataGc.Free();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputArray"></param>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static byte[] ArrayToByteArray<TType>(TType[] inputArray)
        {
            var outputArray = new byte[inputArray.Length * Marshal.SizeOf(typeof(TType))];
            GetArrayPointer(inputArray,
                (inputPointer) =>
                {
                    GetArrayPointer(outputArray,
                        (outputPointer) =>
                        {
                            Memcpy((byte*) outputPointer.ToPointer(), (byte*) inputPointer.ToPointer(),
                                outputArray.Length);
                        });
                });
            return outputArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputArray"></param>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static TType[] ByteArrayToArray<TType>(byte[] inputArray)
        {
            var outputArray = new TType[inputArray.Length / Marshal.SizeOf(typeof(TType))];
            GetArrayPointer(inputArray,
                (inputPointer) =>
                {
                    GetArrayPointer(outputArray,
                        (outputPointer) =>
                        {
                            Memcpy((byte*) outputPointer.ToPointer(), (byte*) inputPointer.ToPointer(),
                                inputArray.Length);
                        });
                });
            return outputArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="arrayLength"></param>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static TType[] PointerToArray<TType>(void* pointer, int arrayLength)
        {
            var array = new TType[arrayLength];

            GetArrayPointer(array,
                (dataPointer) =>
                {
                    Memcpy((byte*) dataPointer.ToPointer(), (byte*) pointer,
                        arrayLength * Marshal.SizeOf(typeof(TType)));
                });

            return array;
        }

        public static void ByteArrayToPointer(byte[] array, byte* output) => Memcpy(output, array, array.Length);
        public static string FixedByteGet(int size, byte* ptr) => PtrToStringUtf8(ptr);
        public static void FixedByteSet(int size, byte* ptr, string value) => StoreStringOnPtr(value, Encoding.UTF8, ptr, size);
        public static int Sizeof<T>() => Marshal.SizeOf(typeof(T));

        public static int Memcmp(byte* left, byte* right, int count)
        {
            for (var n = 0; n < count; n++)
            {
                var dif = left[n] - right[n];
                if (dif != 0) return dif;
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int FastHash(byte* ptr, int len) => FastHash(new Span<byte>(ptr, len));

        public static int FastHash(Span<byte> data)
        {
            switch (data.Length)
            {
                case 0: return 0;
                case 1: return (data[0] << 0);
                case 2: return (data[0] << 0) | (data[1] << 8);
                case 3: return (data[0] << 0) | (data[1] << 8) | (data[2] << 16);
                default:
                    var hash = data.Length;
                    for (var n = 0; n < data.Length; n++)
                    {
                        hash ^= n << 28;
                        hash += data[n];
                    }
                    return hash;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="callback"></param>
        /// <typeparam name="TType"></typeparam>
        public static void SafeFixed<TType>(TType Object, Action<IntPtr> callback)
        {
            var handle = GCHandle.Alloc(Object, GCHandleType.Pinned);
            try
            {
                callback(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        //public static unsafe void CopyStructWithSize<TType>(ref TType Destination, ref TType Source, int Size)
        //{
        //	SafeFixed(ref Destination, (DestinationPtr) =>
        //	{
        //		SafeFixed(ref Source, (SourcePtr) =>
        //		{
        //		});
        //	});
        //	//Destination = Source;
        //}
    }
}