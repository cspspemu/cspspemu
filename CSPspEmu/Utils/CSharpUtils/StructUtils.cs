using System;
using System.Runtime.InteropServices;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class StructUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expectedSize"></param>
        public static void ExpectSize<T>(int expectedSize)
        {
            var realSize = Marshal.SizeOf(typeof(T));
            if (realSize != expectedSize)
            {
                throw new Exception("Expecting struct '" + typeof(T).FullName + "' size. Expected(" + expectedSize +
                                    ") but Obtained(" + realSize + ").");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static unsafe T BytesToStruct<T>(byte[] rawData) where T : struct
        {
            T result;

            var expectedLength = Marshal.SizeOf(typeof(T));
            if (rawData.Length < expectedLength)
                throw new Exception(
	                $"BytesToStruct. Not enough bytes. Expected: {expectedLength} Provided: {rawData.Length}");

#if true
            fixed (byte* rawDataPointer = &rawData[0])
            {
                result = (T) Marshal.PtrToStructure(new IntPtr(rawDataPointer), typeof(T));
            }
#else
			GCHandle handle = GCHandle.Alloc(RawData, GCHandleType.Pinned);

			try
			{
				IntPtr rawDataPtr = handle.AddrOfPinnedObject();
				result = (T)Marshal.PtrToStructure(rawDataPtr, typeof(T));
			}
			finally
			{
				handle.Free();
			}
#endif

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static unsafe T[] BytesToStructArray<T>(byte[] rawData) where T : struct
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            var array = new T[rawData.Length / elementSize];

            var type = typeof(T);
            fixed (byte* rawDataPointer = &rawData[0])
            {
                for (var n = 0; n < array.Length; n++)
                {
                    array[n] = (T) Marshal.PtrToStructure(new IntPtr(rawDataPointer + n * elementSize), type);
                }
            }

            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static unsafe byte[] StructToBytes<T>(T data) where T : struct
        {
            var rawData = new byte[Marshal.SizeOf(data)];

#if true
            fixed (byte* rawDataPointer = rawData)
            {
                Marshal.StructureToPtr(data, new IntPtr(rawDataPointer), false);
            }
#else
			GCHandle Handle = GCHandle.Alloc(RawData, GCHandleType.Pinned);
			try
			{
				IntPtr RawDataPointer = Handle.AddrOfPinnedObject();
				Marshal.StructureToPtr(Data, RawDataPointer, false);
			}
			finally
			{
				Handle.Free();
			}
#endif

            return rawData;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataArray"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static unsafe byte[] StructArrayToBytes<T>(T[] dataArray, int count = -1) where T : struct
        {
            if (count == -1) count = dataArray.Length;
            var elementSize = Marshal.SizeOf(dataArray[0]);
            var rawData = new byte[elementSize * count];

#if true
            fixed (byte* rawDataPointer = rawData)
            {
                for (var n = 0; n < count; n++)
                {
                    Marshal.StructureToPtr(dataArray[n], new IntPtr(rawDataPointer + elementSize * n), false);
                }
            }
#else
			GCHandle handle = GCHandle.Alloc(RawData, GCHandleType.Pinned);
			try
			{
				for (int n = 0; n < Count; n++)
				{
					IntPtr RawDataPointer = handle.AddrOfPinnedObject() + ElementSize * n;
					Marshal.StructureToPtr(DataArray[n], RawDataPointer, false);
				}
			}
			finally
			{
				handle.Free();
			}

			//RespectEndianness(typeof(T), rawData);
#endif

            return rawData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static ulong GetULongFrom2UInt(uint low, uint high)
        {
            return (ulong) low | ((ulong) high << 32);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        public static void ConvertULongTo2UInt(ulong value, out uint low, out uint high)
        {
            low = (uint) (value >> 0) & uint.MaxValue;
            high = (uint) (value >> 32) & uint.MaxValue;
        }
    }
}