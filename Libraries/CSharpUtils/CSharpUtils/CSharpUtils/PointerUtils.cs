using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpUtils
{
	unsafe public class PointerUtils
	{
		static public String PtrToStringUtf8(byte* Pointer)
		{
			return PtrToString(Pointer, Encoding.UTF8);
		}

		static public String PtrToString(byte* Pointer, Encoding Encoding)
		{
			if (Pointer == null) return null;
			List<byte> Bytes = new List<byte>();
			for (; *Pointer != 0; Pointer++) Bytes.Add(*Pointer);
			return Encoding.GetString(Bytes.ToArray());
		}

		static public ushort PtrToShort_BE(void* ptr)
		{
			var bytes = (byte*)ptr;
			//return (ushort)((bytes[1] << 8) | (bytes[0] << 8));
			return (ushort)((bytes[0] << 8) | (bytes[1] << 0));
		}

		static public String PtrToString(byte* Pointer, int Length, Encoding Encoding)
		{
			if (Pointer == null) return null;
			List<byte> Bytes = new List<byte>();
			for (int n = 0; n < Length; n++)
			{
				if (Pointer[n] == 0) break;
				Bytes.Add(Pointer[n]);
			}
			return Encoding.GetString(Bytes.ToArray());
		}

		static public void StoreStringOnPtr(string String, Encoding Encoding, byte* Pointer, int PointerMaxLength = 0x10000)
		{
			if (Pointer == null) return;
			//if (String == null) return;
			if (String != null)
			{
				var Bytes = Encoding.GetBytes(String);
				foreach (var Byte in Bytes)
				{
					*Pointer++ = Byte;
				}
			}
			*Pointer++ = 0;
		}

		static public void Memset(byte[] Array, byte Value, int Count)
		{
			Memset(Array, Value, 0, Count);
		}

		static public void Memset(byte[] Array, byte Value, int Offset, int Count)
		{
			if (Count > 0)
			{
				if (Offset + Count > Array.Length) throw (new InvalidOperationException(String.Format("Array out of bounts")));
				fixed (byte* ArrayPointer = &Array[Offset])
				{
					Memset(ArrayPointer, Value, Count);
				}
			}
		}

		public static bool Is64;

		static PointerUtils()
		{
			Is64 = Environment.Is64BitProcess;
		}

		static public void Memset(byte* Pointer, byte Value, int Count)
		{
			if (Pointer == null) throw(new ArgumentNullException("Memset pointer is null"));

#if true
			if (Count >= 16)
			{
				var Value2 = (ushort)(((ushort)Value << 8) | ((ushort)Value << 0));
				var Value4 = (uint)(((uint)Value2 << 16) | ((uint)Value2 << 0));

				if (Is64)
				{

					var Value8 = (ulong)(((ulong)Value4 << 32) | ((ulong)Value4 << 0));
					var Pointer8 = (ulong*)Pointer;
					while (Count >= 8)
					{
						*Pointer8++ = Value8;
						Count -= 8;
					}
					Pointer = (byte*)Pointer8;
				}
				else
				{
					var Pointer4 = (uint*)Pointer;
					while (Count >= 4)
					{
						*Pointer4++ = Value4;
						Count -= 4;
					}
					Pointer = (byte*)Pointer4;
				}
			}
#endif

			while (Count > 0)
			{
				*Pointer++ = Value;
				Count--;
			}
		}

#if false
		static public void MemsetSlow(byte[] Array, byte Value, int Count)
		{
			fixed (byte* ArrayPointer = Array)
			{
				MemsetSlow(ArrayPointer, Value, Count);
			}
		}

		static public void MemsetSlow(byte* Pointer, byte Value, int Count)
		{
			if (Pointer == null) throw (new ArgumentNullException("Memset pointer is null"));

			while (Count-- > 0) *Pointer++ = Value;
		}
#endif

		public static void Memcpy(byte* Destination, ArraySegment<byte> Source)
		{
			if (Source.Count > 0)
			{
				fixed (byte* SourcePtr = &Source.Array[Source.Offset])
				{
					Memcpy(Destination, SourcePtr, Source.Count);
				}
			}
		}

		public static void Memcpy(byte* Destination, byte[] Source, int Count)
		{
			fixed (byte* SourcePtr = Source)
			{
				Memcpy(Destination, SourcePtr, Count);
			}
		}

		public static void Memcpy(ArraySegment<byte> Destination, byte* Source)
		{
			//var Pin = GCHandle.Alloc(Destination.Array, GCHandleType.Pinned);
			//Pin.Free();
			if (Destination.Count > 0)
			{
				fixed (byte* DestinationPtr = &Destination.Array[Destination.Offset])
				{
					//Marshal.UnsafeAddrOfPinnedArrayElement(
					Memcpy(DestinationPtr, Source, Destination.Count);
				}
			}
		}

		public static void Memcpy(byte[] Destination, byte* Source, int Count)
		{
			fixed (byte* DestinationPtr = Destination)
			{
				Memcpy(DestinationPtr, Source, Count);
			}
		}

		public static int FindLargestMatch(byte* Haystack, byte* Needle, int MaxLength)
		{
			int Match = 0;

			if (Is64)
			{
				while ((MaxLength >= 8) && (*(ulong*)Haystack == *(ulong*)Needle))
				{
					Match += 8;
					Haystack += 8;
					Needle += 8;
					MaxLength -= 8;
				}
			}

			while ((MaxLength >= 4) && (*(uint*)Haystack == *(uint*)Needle))
			{
				Match += 4;
				Haystack += 4;
				Needle += 4;
				MaxLength -= 4;
			}

			while ((MaxLength >= 1) && (*(byte*)Haystack == *(byte*)Needle))
			{
				Match += 1;
				Haystack += 1;
				Needle += 1;
				MaxLength -= 1;
			}

			return Match;
		}

		public static int FindLargestMatchByte(byte* Haystack, byte Value1, int MaxLength)
		{
			int Match = 0;

			if (MaxLength >= 4)
			{
				var Value2 = (ushort)(((ushort)Value1 << 0) | ((ushort)Value1 << 8));
				var Value4 = (uint)(((uint)Value2 << 0) | ((uint)Value2 << 16));


				if ((MaxLength >= 8) && Is64)
				{
					var Value8 = (ulong)(((ulong)Value4 << 0) | ((ulong)Value4 << 32));

					while ((MaxLength >= 8) && (*(ulong*)Haystack == Value8))
					{
						Match += 8;
						Haystack += 8;
						MaxLength -= 8;
					}
				}

				while ((MaxLength >= 4) && (*(uint*)Haystack == Value4))
				{
					Match += 4;
					Haystack += 4;
					MaxLength -= 4;
				}
			}

			while ((MaxLength >= 1) && (*(byte*)Haystack == Value1))
			{
				Match += 1;
				Haystack += 1;
				MaxLength -= 1;
			}

			return Match;
		}

		public static void Memcpy(byte* Destination, byte* Source, int Size)
		{
			long Distance = (long)Math.Abs(Destination - Source);
#if true
			if (Is64 && (Distance >= 8))
			{
				while (Size >= sizeof(ulong))
				{
					*(ulong*)Destination = *(ulong*)Source;
					Destination += sizeof(ulong);
					Source += sizeof(ulong);
					Size -= sizeof(ulong);
				}
			}
			else if (Distance >= 4)
			{
				while (Size >= sizeof(uint))
				{
					*(uint*)Destination = *(uint*)Source;
					Destination += sizeof(uint);
					Source += sizeof(uint);
					Size -= sizeof(uint);
				}
			}
			else if (Distance >= 2)
			{
				while (Size >= sizeof(ushort))
				{
					*(ushort*)Destination = *(ushort*)Source;
					Destination += sizeof(ushort);
					Source += sizeof(ushort);
					Size -= sizeof(ushort);
				}
			}
#endif

			while (Size > 0)
			{
				*((byte*)Destination) = *((byte*)Source);
				Destination++;
				Source++;
				Size--;
			}
		}

		public static unsafe byte[] PointerToByteArray(byte* Pointer, int Size)
		{
			var Data = new byte[Size];
			fixed (byte* DataPtr = Data)
			{
				Memcpy(DataPtr, Pointer, Size);
			}
			return Data;
		}

		public static unsafe void GetArrayPointer<TType>(TType[] Array, Action<IntPtr> Action)
		{
			GCHandle DataGc = GCHandle.Alloc(Array, GCHandleType.Pinned);

			var DataPointer = DataGc.AddrOfPinnedObject();
			try
			{
				Action(DataPointer);
			}
			finally
			{
				DataGc.Free();
			}
		}

		public static unsafe byte[] ArrayToByteArray<TType>(TType[] InputArray)
		{
			var OutputArray = new byte[InputArray.Length * Marshal.SizeOf(typeof(TType))];
			GetArrayPointer(InputArray, (InputPointer) =>
			{
				GetArrayPointer(OutputArray, (OutputPointer) =>
				{
					Memcpy((byte*)OutputPointer.ToPointer(), (byte*)InputPointer.ToPointer(), OutputArray.Length);
				});
			});
			return OutputArray;
		}

		public static unsafe TType[] ByteArrayToArray<TType>(byte[] InputArray)
		{
			var OutputArray = new TType[InputArray.Length / Marshal.SizeOf(typeof(TType))];
			GetArrayPointer(InputArray, (InputPointer) =>
			{
				GetArrayPointer(OutputArray, (OutputPointer) =>
				{
					Memcpy((byte*)OutputPointer.ToPointer(), (byte*)InputPointer.ToPointer(), InputArray.Length);
				});
			});
			return OutputArray;
		}

		public static unsafe TType[] PointerToArray<TType>(void* Pointer, int ArrayLength)
		{
			var Array = new TType[ArrayLength];

			GetArrayPointer(Array, (DataPointer) =>
			{
				Memcpy((byte*)DataPointer.ToPointer(), (byte*)Pointer, ArrayLength * Marshal.SizeOf(typeof(TType)));
			});

			return Array;
		}

		public static unsafe void ByteArrayToPointer(byte[] Array, byte* Output)
		{
			PointerUtils.Memcpy(Output, Array, Array.Length);
		}

		public static string FixedByteGet(int Size, byte* Ptr)
		{
			return PtrToStringUtf8(Ptr);
		}

		public static void FixedByteSet(int Size, byte* Ptr, string Value)
		{
			StoreStringOnPtr(Value, Encoding.UTF8, Ptr, Size);
		}

		public static int Sizeof<T>()
		{
			return Marshal.SizeOf(typeof(T));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		public static unsafe int Memcmp(byte* Left, byte* Right, int Count)
		{
			for (int n = 0; n < Count; n++)
			{
				var Dif = (int)Left[n] - (int)Right[n];
				if (Dif != 0) return Dif;
			}
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Ptr"></param>
		/// <param name="Len"></param>
		/// <returns></returns>
		public static unsafe int FastHash(byte* Ptr, int Len)
		{
			switch (Len)
			{
				case 0: return 0;
				case 1: return (Ptr[0] << 0);
				case 2: return (Ptr[0] << 0) | (Ptr[1] << 8);
				case 3: return (Ptr[0] << 0) | (Ptr[1] << 8) | (Ptr[2] << 16);
				default:
					int Hash = Len;
					for (int n = 0; n < Len; n++)
					{
						Hash ^= n << 28;
						Hash += *Ptr++;
					}
					return Hash;
			}
		}

		public static unsafe void SafeFixed<TType>(TType Object, Action<IntPtr> Callback)
		{
			var Handle = GCHandle.Alloc(Object, GCHandleType.Pinned);
			try
			{
				Callback(Handle.AddrOfPinnedObject());
			}
			finally
			{
				Handle.Free();
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
