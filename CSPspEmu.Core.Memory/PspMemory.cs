using System;
using System.IO;
using System.Text;
using CSharpUtils;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Memory
{
	public unsafe abstract class PspModel
	{
		public const bool IsSlim = false;
		//public const bool IsSlim = true;
	}

	/// <summary>
	/// Start       End         Size  Description
	/// 0x00010000  0x00013fff  16kb  Scratchpad
	/// 0x04000000  0x041fffff  2mb   Video Memory / Frame Buffer
	/// 0x04200000                    Video Memory (Unswizzled)
	/// 0x04400000  0x046fffff  2mb   Video Memory (Mirror)
	/// 0x04600000  0x048fffff  2mb   VRAM with "swizzle" + 32-byte column interleave. Reading from VRAM+6Mib will give you a proper linearized version of the depth buffer with no effort. The GE sees the same view; a GE copy operation returns the same data (represented as RGB 565): images/readdepth.png
	/// 0x08000000  0x09ffffff  32mb  Main Memory
	/// 0x1c000000  0x1fbfffff        Hardware I/O
	/// 0x1fc00000  0x1fcfffff  1mb   Hardware Exception Vectors (RAM)
	/// 0x1fd00000  0x1fffffff        Hardware I/O
	/// </summary>
	public unsafe abstract class PspMemory : IPspMemoryInfo, IDisposable
	{
		//internal static Logger Logger = Logger.GetLogger("Memory");

		abstract public bool HasFixedGlobalAddress { get; }
		abstract public IntPtr FixedGlobalAddress { get; }

		public const uint MemoryMask = 0x1FFFFFFF;

		static public readonly void* InvalidPointer = Marshal.AllocHGlobal(0x10000).ToPointer();
		public readonly void* InvalidPointerInstance = PspMemory.InvalidPointer;

		public class InvalidAddressException : Exception
		{
			public InvalidAddressException(string message) : base (message) { }
			public InvalidAddressException(string message, Exception innerException) : base(message, innerException) { }
			public InvalidAddressException(ulong Address) : base(String.Format("Invalid Address : 0x{0:X8}", Address)) { }
			public InvalidAddressException(ulong Address, Exception innerException) : base(String.Format("Invalid Address : 0x{0:X8}", Address), innerException) { }
		}

		public sealed class Segment
		{
			public readonly uint Low;
			public readonly uint High;
			public readonly uint Size;

			public Segment(uint Offset, uint Size)
			{
				this.Low = Offset;
				this.Size = Size;
				this.High = Offset + Size;
			}

			public bool Contains(uint Address)
			{
				return (Address >= Low && Address < High);
			}
		}

		public void Reset()
		{
			ZeroFillSegment(ScratchPadSegment);
			ZeroFillSegment(MainSegment);
			ZeroFillSegment(FrameBufferSegment);
		}

		public void ZeroFillSegment(Segment Segment)
		{
			PointerUtils.Memset((byte *)PspAddressToPointerSafe(Segment.Low), 0, (int)Segment.Size);
		}

		public static readonly Segment ScratchPadSegment = new Segment(ScratchPadOffset, ScratchPadSize);
		public static readonly Segment MainSegment = new Segment(MainOffset, MainSize);
		public static readonly Segment FrameBufferSegment = new Segment(FrameBufferOffset, FrameBufferSize);
		public static readonly Segment VectorsSegment = new Segment(VectorsOffset, VectorsSize);

		//public static Segment ScratchPadSegment = new Segment() { Offset = 0x00010000, Size = 4 * 1024 };

		public const int ScratchPadSize = 4 * 1024; // 4KB
		public const int FrameBufferSize = 2 * 0x00100000; // 2MB
		public const int MainSize = (PspModel.IsSlim ? 64 : 32) * 0x100000; // 32MB (PHAT) / 64MB (SLIM)
		//public const int HardwareVectorsSize = 1 * 1024 * 1024;
		public const int VectorsSize = 4 * 1024 * 1024;

		public const uint ScratchPadOffset = 0x00010000;
		public const uint FrameBufferOffset = 0x04000000;
		public const uint MainOffset = 0x08000000;
		//public const uint VectorsOffset = 0xbfc00000;
		public const uint VectorsOffset = 0x1fc00000;

		protected byte* NullPtr;
		protected byte* ScratchPadPtr; // 4KB
		protected byte* FrameBufferPtr; // 2MB
		protected byte* MainPtr;
		protected byte* VectorsPtr;
		protected uint* LogMainPtr;

		public abstract uint PointerToPspAddressUnsafe(void* Pointer);
		public abstract void* PspAddressToPointerUnsafe(uint Address);

		public void* PspAddressToPointerNotNull(uint _Address) 
		{
			var Pointer = PspAddressToPointerUnsafe(_Address);
			if (Pointer == null) throw (new InvalidAddressException(_Address));
			return Pointer;
		}

		public PspPointer PointerToPspPointer(void* Pointer)
		{
			return new PspPointer(PointerToPspAddressSafe(Pointer));
		}

		public void* PspPointerToPointerSafe(PspPointer Pointer, int Size = 0)
		{
			return PspAddressToPointerSafe(Pointer.Address, Size);
		}

		public virtual uint PointerToPspAddressSafe(void* Pointer)
		{
			return PointerToPspAddressSafe((byte*)Pointer);
		}

		public virtual uint PointerToPspAddressSafe(byte* Pointer)
		{
			if (Pointer == null) return 0;
			//if (Pointer == NullPtr) return 0;
			if ((Pointer >= ScratchPadPtr) && (Pointer < ScratchPadPtr + ScratchPadSize)) return (uint)(ScratchPadOffset + (Pointer - ScratchPadPtr));
			if ((Pointer >= FrameBufferPtr) && (Pointer < FrameBufferPtr + FrameBufferSize)) return (uint)(FrameBufferOffset + (Pointer - FrameBufferPtr));
			if ((Pointer >= MainPtr) && (Pointer < MainPtr + MainSize)) return (uint)(MainOffset + (Pointer - MainPtr));
			if ((Pointer >= VectorsPtr) && (Pointer < VectorsPtr + VectorsSize)) return (uint)(VectorsOffset + (Pointer - VectorsPtr));
			throw (new InvalidAddressException(String.Format("Pointer 0x{0:X} doesn't belong to PSP Memory. Main: 0x{1:X}-0x{2:X}", (ulong)Pointer, (ulong)MainPtr, (ulong)MainSize)));
		}

		public virtual void SetPCWriteAddress(uint Address, uint PC)
		{
		}

		public virtual uint GetPCWriteAddress(uint Address)
		{
			return 0xFFFFFFFF;
		}

		public static bool IsRangeValid(uint Address, int Size)
		{
			if (!IsAddressValid(Address)) return false;
			if (!IsAddressValid((uint)(Address + Size - 1))) return false;
			return true;
		}

		public static void ValidateRange(uint Address, int Size)
		{
			if (!IsAddressValid((uint)(Address + 0))) throw (new InvalidAddressException(Address));
			if (Size > 1)
			{
				if (!IsAddressValid((uint)(Address + Size - 1))) throw (new InvalidAddressException(Address));
			}
		}

		public virtual void* PspAddressToPointerSafe(uint Address, int Size = 0, bool CanBeNull = true, bool InvalidAsNull = false)
		{
			if (Address == 0 && CanBeNull) return null;
			ValidateRange(Address, Size);
			return PspAddressToPointerUnsafe(Address);
		}

		public static void CheckAndEnforceAddressValid(uint Address)
		{
			ValidateRange(Address, 0);
		}

		public static bool IsAddressValid(uint _Address)
		{
			var Address = _Address & PspMemory.MemoryMask;
			if (MainSegment.Contains(Address)) return true;
			if (FrameBufferSegment.Contains(Address)) return true;
			if (ScratchPadSegment.Contains(Address)) return true;
			if (VectorsSegment.Contains(Address)) return true;
			return false;
		}

		/*
		public void Write1Unsafe(uint Address, byte Value)
		{
			*((byte*)PspAddressToPointerUnsafe(Address)) = Value;
		}

		public void Write2Unsafe(uint Address, ushort Value)
		{
			*((ushort*)PspAddressToPointerUnsafe(Address)) = Value;
		}

		public void Write4Unsafe(uint Address, uint Value)
		{
			*((uint*)PspAddressToPointerUnsafe(Address)) = Value;
		}

		public void Write8Unsafe(uint Address, ulong Value)
		{
			*((ulong*)PspAddressToPointerUnsafe(Address)) = Value;
		}
		*/

		public void WriteSafe<TType>(uint Address, TType Value) where TType : struct
		{
			WriteStruct<TType>(Address, Value);
		}

		public void WriteBytes(uint Address, byte[] DataIn)
		{
			PointerUtils.Memcpy((byte*)PspAddressToPointerSafe(Address, DataIn.Length), DataIn, DataIn.Length);
		}

		public void WriteBytes(uint Address, byte* DataInPointer, int DataInLength)
		{
			PointerUtils.Memcpy((byte*)PspAddressToPointerSafe(Address, DataInLength), DataInPointer, DataInLength);
		}

		public void WriteStruct<TType>(uint Address, TType Value) where TType : struct
		{
			WriteBytes(Address, StructUtils.StructToBytes(Value));
		}

		public void WriteRepeated1(byte Value, uint Address, int Count)
		{
			PointerUtils.Memset((byte*)PspAddressToPointerSafe(Address, Count), Value, Count);
		}

		/*
		public byte Read1Unsafe(uint Address)
		{
			return *((byte*)PspAddressToPointerUnsafe(Address));
		}

		public ushort Read2Unsafe(uint Address)
		{
			return *((ushort*)PspAddressToPointerUnsafe(Address));
		}

		public uint Read4Unsafe(uint Address)
		{
			return *((uint*)PspAddressToPointerUnsafe(Address));
		}

		public ulong Read8Unsafe(uint Address)
		{
			return *((ulong*)PspAddressToPointerUnsafe(Address));
		}
		*/

		public TType ReadSafe<TType>(uint Address) where TType : struct
		{
			return StructUtils.BytesToStruct<TType>(ReadBytes(Address, PointerUtils.Sizeof<TType>()));
		}

		public byte[] ReadBytes(uint Address, int Count)
		{
			var Output = new byte[Count];
			fixed (byte* OutputPtr = Output)
			{
				ReadBytes(Address, OutputPtr, Count);
			}
			return Output;
		}

		public virtual byte Read1(uint Address) { return *(byte *)PspAddressToPointerNotNull(Address); }
		public virtual ushort Read2(uint Address) { return *(ushort*)PspAddressToPointerNotNull(Address); }
		public virtual uint Read4(uint Address) { return *(uint*)PspAddressToPointerNotNull(Address); }
		public virtual ulong Read8(uint Address) { return *(ulong*)PspAddressToPointerNotNull(Address); }

		public virtual void Write1(uint Address, byte Value) { *(byte*)PspAddressToPointerNotNull(Address) = Value; }
		public virtual void Write2(uint Address, ushort Value) { *(ushort*)PspAddressToPointerNotNull(Address) = Value; }
		public virtual void Write4(uint Address, uint Value) { *(uint*)PspAddressToPointerNotNull(Address) = Value; }
		public virtual void Write8(uint Address, ulong Value) { *(ulong*)PspAddressToPointerNotNull(Address) = Value; }

		public void ReadBytes(uint Address, byte* DataOutPointer, int DataOutLength)
		{
			PointerUtils.Memcpy(DataOutPointer, (byte*)PspAddressToPointerSafe(Address, DataOutLength), DataOutLength);
		}

		public TType ReadStruct<TType>(uint Address) where TType : struct
		{
			return StructUtils.BytesToStruct<TType>(ReadBytes(Address, PointerUtils.Sizeof<TType>()));
		}

		public abstract void Dispose();

		public void Copy(uint SourceAddress, uint DestinationAddress, int Size)
		{
			var Source = PspAddressToPointerSafe(SourceAddress, Size);
			var Destination = PspAddressToPointerSafe(DestinationAddress, Size);
			PointerUtils.Memcpy((byte*)Destination, (byte*)Source, Size);
		}

		public string ReadStringz(uint Address, Encoding Encoding)
		{
			return new PspMemoryStream(this).SliceWithLength(Address).ReadStringz(-1, Encoding);
		}

		public uint WriteStringz(uint Address, string String)
		{
			var Bytes = Encoding.UTF8.GetBytes(String + "\0");
			WriteBytes(Address, Bytes);
			return (uint)Bytes.Length;
		}

		public uint WriteStringz(uint Address, string[] Strings)
		{
			uint StartAddress = Address;
			foreach (var String in Strings)
			{
				Address += WriteStringz(Address, String);
			}
			return Address - StartAddress;
		}

		public void Dump(string OutputFile)
		{
			using (var Stream = File.OpenWrite(OutputFile))
			{
				Stream.WriteStream(new PspMemoryStream(this).SliceWithBounds(MainSegment.Low, MainSegment.High - 1));
				Stream.Flush();
				Stream.Close();
			}
		}

		bool IPspMemoryInfo.IsAddressValid(uint Address)
		{
			return PspMemory.IsAddressValid(Address);
		}
	}
}
