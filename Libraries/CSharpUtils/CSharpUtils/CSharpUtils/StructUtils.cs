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
        /// <param name="ExpectedSize"></param>
        public static void ExpectSize<T>(int ExpectedSize)
        {
            int RealSize = Marshal.SizeOf(typeof(T));
            if (RealSize != ExpectedSize)
            {
                throw (new Exception("Expecting struct '" + typeof(T).FullName + "' size. Expected(" + ExpectedSize +
                                     ") but Obtained(" + RealSize + ")."));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RawData"></param>
        /// <returns></returns>
        public unsafe static T BytesToStruct<T>(byte[] RawData) where T : struct
        {
            T result = default(T);

            int ExpectedLength = Marshal.SizeOf(typeof(T));
            if (RawData.Length < ExpectedLength)
                throw (new Exception(String.Format("BytesToStruct. Not enough bytes. Expected: {0} Provided: {1}",
                    ExpectedLength, RawData.Length)));

#if true
            fixed (byte* RawDataPointer = &RawData[0])
            {
                result = (T) Marshal.PtrToStructure(new IntPtr(RawDataPointer), typeof(T));
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
        /// <param name="RawData"></param>
        /// <returns></returns>
        public unsafe static T[] BytesToStructArray<T>(byte[] RawData) where T : struct
        {
            int ElementSize = Marshal.SizeOf(typeof(T));
            T[] Array = new T[RawData.Length / ElementSize];

            var Type = typeof(T);
            fixed (byte* RawDataPointer = &RawData[0])
            {
                for (int n = 0; n < Array.Length; n++)
                {
                    Array[n] = (T) Marshal.PtrToStructure(new IntPtr(RawDataPointer + n * ElementSize), Type);
                }
            }

            return Array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <returns></returns>
        public unsafe static byte[] StructToBytes<T>(T Data) where T : struct
        {
            byte[] RawData = new byte[Marshal.SizeOf(Data)];

#if true
            fixed (byte* RawDataPointer = RawData)
            {
                Marshal.StructureToPtr(Data, new IntPtr(RawDataPointer), false);
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

            return RawData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DataArray"></param>
        /// <returns></returns>
        public unsafe static byte[] StructArrayToBytes<T>(T[] DataArray, int Count = -1) where T : struct
        {
            if (Count == -1) Count = DataArray.Length;
            int ElementSize = Marshal.SizeOf(DataArray[0]);
            byte[] RawData = new byte[ElementSize * Count];

#if true
            fixed (byte* RawDataPointer = RawData)
            {
                for (int n = 0; n < Count; n++)
                {
                    Marshal.StructureToPtr(DataArray[n], new IntPtr(RawDataPointer + ElementSize * n), false);
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

            return RawData;
        }

        public static ulong GetULongFrom2UInt(ref uint Low, ref uint High)
        {
            return ((ulong) Low) | (((ulong) High) << 32);
        }

        public static void ConvertULongTo2UInt(ulong Value, ref uint Low, ref uint High)
        {
            Low = ((uint) (Value >> 0) & uint.MaxValue);
            High = ((uint) (Value >> 32) & uint.MaxValue);
        }
    }
}