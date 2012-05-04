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
			for (int n = 0; n < Length; n++) Bytes.Add(Pointer[n]);
			return Encoding.GetString(Bytes.ToArray());
		}

		static public void StoreStringOnPtr(string String, Encoding Encoding, byte* Pointer, int PointerMaxLength = 0x10000)
		{
			var Bytes = Encoding.GetBytes(String);
			foreach (var Byte in Bytes)
			{
				*Pointer++ = Byte;
			}
			*Pointer++ = 0;
		}

		static public void Memset(byte[] Array, byte Value, int Count)
		{
			fixed (byte* ArrayPointer = Array)
			{
				Memset(ArrayPointer, Value, Count);
			}
		}

		static public void Memset(byte* Pointer, byte Value, int Count)
		{
			if (Pointer == null) throw(new ArgumentNullException("Memset pointer is null"));

			if (Count >= 32)
			{
				var Value2 = (ushort)(((ushort)Value << 8) | ((ushort)Value << 0));
				var Value4 = (uint)(((uint)Value2 << 16) | ((uint)Value2 << 0));

#if true
				var Value8 = (ulong)(((ulong)Value4 << 32) | ((ulong)Value4 << 0));
				var Pointer8 = (ulong*)Pointer;
				while (Count >= 8)
				{
					*Pointer8++ = Value8;
					Count -= 8;
				}
				Pointer = (byte*)Pointer8;
#else
				var Pointer4 = (uint*)Pointer;
				while (Count >= 4)
				{
					*Pointer4++ = Value4;
					Count -= 4;
				}
				Pointer = (byte*)Pointer4;
#endif
			}

			while (Count > 0)
			{
				*Pointer++ = Value;
				Count--;
			}
		}

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

		[DllImport("kernel32.dll")]
		static extern void RtlMoveMemory(byte* Destination, byte* Source, int Size);

		public static void Memcpy(byte* Destination, byte* Source, int Size)
		{
			//Marshal.Copy(new IntPtr(Source), new IntPtr(Destination), 0, Size);
#if true
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				case PlatformID.Win32S:
					RtlMoveMemory(Destination, Source, Size);
					break;
				case PlatformID.Unix:
					while (Size-- > 0) *Destination++ = *Source++;
					break;
			}
			
			//while (Size-- > 0) *Destination++ = *Source++;
			/*
			byte* DestinationEnd = Destination + Size;
			while (Destination < DestinationEnd)
			{
				*Destination++ = *Source++;
			}
			*/
#else
			//Marshal.Copy(new IntPtr(Source), new IntPtr(Destination), 0, Size);
			for (int n = 0; n < Size; n++) Destination[n] = Source[n];
#endif
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
			Marshal.Copy(Array, 0, new IntPtr(Output), Array.Length);
		}

		public static string FixedByteGet(int Size, byte* Ptr)
		{
			return PtrToStringUtf8(Ptr);
		}

		public static void FixedByteSet(int Size, byte* Ptr, string Value)
		{
			StoreStringOnPtr(Value, Encoding.UTF8, Ptr, Size);
		}

		public static unsafe int Memcmp(byte* Left, byte* Right, int Count)
		{
			for (int n = 0; n < Count; n++)
			{
				var Dif = (int)Left[n] - (int)Right[n];
				if (Dif != 0) return Dif;
			}
			return 0;
		}
	}
}
