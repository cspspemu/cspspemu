using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSharpUtils
{
	public static class ArrayUtils
	{
		public static IEnumerable<int> Range(int From, int To)
		{
			for (int n = From; n < To; n++)
			{
				yield return n;
			}
		}

		public static IEnumerable<int> Range(int To)
		{
			return Range(0, To);
		}

		public static unsafe T[] CreateArray<T>(void* source, int length)
		{
			var type = typeof(T);
			var sizeInBytes = Marshal.SizeOf(typeof(T));

			T[] output = new T[length];

			if (type.IsPrimitive)
			{
				// Make sure the array won't be moved around by the GC 
				var handle = GCHandle.Alloc(output, GCHandleType.Pinned);

				var destination = (byte*)handle.AddrOfPinnedObject().ToPointer();
				var byteLength = length * sizeInBytes;

				// There are faster ways to do this, particularly by using wider types or by 
				// handling special lengths.
				for (int i = 0; i < byteLength; i++)
					destination[i] = ((byte*)source)[i];

				handle.Free();
			}
			else if (type.IsValueType)
			{
				if (!type.IsLayoutSequential && !type.IsExplicitLayout)
				{
					throw new InvalidOperationException(string.Format("{0} does not define a StructLayout attribute", type));
				}

				IntPtr sourcePtr = new IntPtr(source);

				for (int i = 0; i < length; i++)
				{
					IntPtr p = new IntPtr((byte*)source + i * sizeInBytes);

					output[i] = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(p, typeof(T));
				}
			}
			else
			{
				throw new InvalidOperationException(string.Format("{0} is not supported", type));
			}

			return output;
		}

		public static void HexDump(byte[] Data, int MaxSize = -1)
		{
			if (MaxSize == -1) MaxSize = Data.Length;
			int Offset = 0;

			Console.WriteLine("");
			while (Offset < MaxSize)
			{
				int RowCount = Math.Min(MaxSize - Offset, 16);

				for (int n = 0; n < RowCount; n++)
				{
					Console.Write("%02X ".Sprintf(Data[Offset + n]));
				}
				for (int n = 0; n < RowCount; n++)
				{
					char c = ((char)Data[Offset + n]);
					Console.Write("{0}", char.IsControl(c) ? '?' : c);
				}
	
				Console.WriteLine("");
				Offset += RowCount;
			}
		}
	}
}
