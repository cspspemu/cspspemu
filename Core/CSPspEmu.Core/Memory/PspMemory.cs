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
			public InvalidAddressException(ulong address) : base(String.Format("Invalid Address : 0x{0:X8}", address)) { }
			public InvalidAddressException(ulong address, Exception innerException) : base(String.Format("Invalid Address : 0x{0:X8}", address), innerException) { }
		}

		public sealed class Segment
		{
			public readonly uint Low;
			public readonly uint High;
			public readonly uint Size;

			public Segment(uint offset, uint size)
			{
				Low = offset;
				Size = size;
				High = offset + size;
			}

			public bool Contains(uint address)
			{
				return (address >= Low && address < High);
			}
		}

		public void Reset()
		{
			ZeroFillSegment(ScratchPadSegment);
			ZeroFillSegment(MainSegment);
			ZeroFillSegment(FrameBufferSegment);
		}

		public void ZeroFillSegment(Segment segment)
		{
			PointerUtils.Memset((byte *)PspAddressToPointerSafe(segment.Low), 0, (int)segment.Size);
		}

		public static readonly Segment ScratchPadSegment = new Segment(ScratchPadOffset, ScratchPadSize);
		public static readonly Segment MainSegment = new Segment(MainOffset, MainSize);
		public static readonly Segment FrameBufferSegment = new Segment(FrameBufferOffset, FrameBufferSize);
		public static readonly Segment VectorsSegment = new Segment(VectorsOffset, VectorsSize);

		//public static Segment ScratchPadSegment = new Segment() { Offset = 0x00010000, Size = 4 * 1024 };

		public const int ScratchPadSize = 4 * 1024; // 4KB
		public const int FrameBufferSize = 0x00200000; // 2MB
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

		public abstract uint PointerToPspAddressUnsafe(void* pointer);
		public abstract void* PspAddressToPointerUnsafe(uint address);

		public void* PspAddressToPointerNotNull(uint address) 
		{
			var pointer = PspAddressToPointerUnsafe(address);
			if (pointer == null) throw (new InvalidAddressException(address));
			return pointer;
		}

		public PspPointer PointerToPspPointer(void* pointer)
		{
			return new PspPointer(PointerToPspAddressSafe(pointer));
		}

		public void* PspPointerToPointerSafe(PspPointer pointer, int size = 0)
		{
			return PspAddressToPointerSafe(pointer.Address, size);
		}

		public virtual uint PointerToPspAddressSafe(void* pointer)
		{
			return PointerToPspAddressSafe((byte*)pointer);
		}

		public virtual uint PointerToPspAddressSafe(byte* pointer)
		{
			if (pointer == null) return 0;
			//if (Pointer == NullPtr) return 0;
			if ((pointer >= ScratchPadPtr) && (pointer < ScratchPadPtr + ScratchPadSize)) return (uint)(ScratchPadOffset + (pointer - ScratchPadPtr));
			if ((pointer >= FrameBufferPtr) && (pointer < FrameBufferPtr + FrameBufferSize)) return (uint)(FrameBufferOffset + (pointer - FrameBufferPtr));
			if ((pointer >= MainPtr) && (pointer < MainPtr + MainSize)) return (uint)(MainOffset + (pointer - MainPtr));
			if ((pointer >= VectorsPtr) && (pointer < VectorsPtr + VectorsSize)) return (uint)(VectorsOffset + (pointer - VectorsPtr));
			throw (new InvalidAddressException($"Pointer 0x{(ulong) pointer:X} doesn't belong to PSP Memory. Main: 0x{(ulong) MainPtr:X}-0x{(ulong) MainSize:X}"));
		}

		public virtual void SetPCWriteAddress(uint address, uint pc)
		{
		}

		public virtual uint GetPCWriteAddress(uint address)
		{
			return 0xFFFFFFFF;
		}

		public static bool IsRangeValid(uint address, int size)
		{
			if (!IsAddressValid(address)) return false;
			if (!IsAddressValid((uint)(address + size - 1))) return false;
			return true;
		}

		public static void ValidateRange(uint address, int size)
		{
			if (!IsAddressValid((uint)(address + 0))) throw (new InvalidAddressException(address));
			if (size > 1)
			{
				if (!IsAddressValid((uint)(address + size - 1))) throw (new InvalidAddressException(address));
			}
		}

		public virtual void* PspAddressToPointerSafe(uint address, int size = 0, bool canBeNull = true, bool invalidAsNull = false)
		{
			if (address == 0 && canBeNull) return null;
			ValidateRange(address, size);
			return PspAddressToPointerUnsafe(address);
		}

		public static void CheckAndEnforceAddressValid(uint address)
		{
			ValidateRange(address, 0);
		}

		public static bool IsAddressValid(uint address)
		{
			var faddress = address & MemoryMask;
			if (MainSegment.Contains(faddress)) return true;
			if (FrameBufferSegment.Contains(faddress)) return true;
			if (ScratchPadSegment.Contains(faddress)) return true;
			if (VectorsSegment.Contains(faddress)) return true;
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

		public void WriteSafe<TType>(uint address, TType value) where TType : struct
		{
			WriteStruct(address, value);
		}

		public void WriteBytes(uint address, byte[] dataIn)
		{
			fixed (byte* dataInPtr = dataIn)
			{
				WriteBytes(address, dataInPtr, dataIn.Length);
			}
		}

		//public delegate void WriteBytesDelegate(uint Address, byte* DataInPointer, int DataInLength);
		//public event WriteBytesDelegate WriteBytesHook;

		public void WriteBytes(uint address, byte* dataInPointer, int dataInLength)
		{
			//if (WriteBytesHook != null) WriteBytesHook(Address, DataInPointer, DataInLength);
			PointerUtils.Memcpy((byte*)PspAddressToPointerSafe(address, dataInLength), dataInPointer, dataInLength);
		}

		public void WriteStruct<TType>(uint address, TType value) where TType : struct
		{
			WriteBytes(address, StructUtils.StructToBytes(value));
		}

		public void WriteRepeated1(byte value, uint address, int count)
		{
			PointerUtils.Memset((byte*)PspAddressToPointerSafe(address, count), value, count);
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

		public TType ReadSafe<TType>(uint address) where TType : struct
		{
			return StructUtils.BytesToStruct<TType>(ReadBytes(address, PointerUtils.Sizeof<TType>()));
		}

		public byte[] ReadBytes(uint address, int count)
		{
			var output = new byte[count];
			fixed (byte* outputPtr = output)
			{
				ReadBytes(address, outputPtr, count);
			}
			return output;
		}

		public virtual byte Read1(uint address) { return *(byte *)PspAddressToPointerNotNull(address); }
		public virtual ushort Read2(uint address) { return *(ushort*)PspAddressToPointerNotNull(address); }
		public virtual uint Read4(uint address) { return *(uint*)PspAddressToPointerNotNull(address); }
		public virtual ulong Read8(uint address) { return *(ulong*)PspAddressToPointerNotNull(address); }

		public virtual void Write1(uint address, byte value) { *(byte*)PspAddressToPointerNotNull(address) = value; }
		public virtual void Write2(uint address, ushort value) { *(ushort*)PspAddressToPointerNotNull(address) = value; }
		public virtual void Write4(uint address, uint value) { *(uint*)PspAddressToPointerNotNull(address) = value; }
		public virtual void Write8(uint address, ulong value) { *(ulong*)PspAddressToPointerNotNull(address) = value; }

		public void ReadBytes(uint address, byte* dataOutPointer, int dataOutLength)
		{
			PointerUtils.Memcpy(dataOutPointer, (byte*)PspAddressToPointerSafe(address, dataOutLength), dataOutLength);
		}

		public TType ReadStruct<TType>(uint address) where TType : struct
		{
			return StructUtils.BytesToStruct<TType>(ReadBytes(address, PointerUtils.Sizeof<TType>()));
		}

		public abstract void Dispose();

		public void Copy(uint sourceAddress, uint destinationAddress, int size)
		{
			var source = PspAddressToPointerSafe(sourceAddress, size);
			var destination = PspAddressToPointerSafe(destinationAddress, size);
			PointerUtils.Memcpy((byte*)destination, (byte*)source, size);
		}

		public string ReadStringz(uint address, Encoding encoding)
		{
			return new PspMemoryStream(this).SliceWithLength(address).ReadStringz(-1, encoding);
		}

		public uint WriteStringz(uint address, string String)
		{
			var bytes = Encoding.UTF8.GetBytes(String + "\0");
			WriteBytes(address, bytes);
			return (uint)bytes.Length;
		}

		public uint WriteStringz(uint address, string[] strings)
		{
			var startAddress = address;
			foreach (var String in strings)
			{
				address += WriteStringz(address, String);
			}
			return address - startAddress;
		}

		public void Dump(string outputFile)
		{
			try
			{
				using (var stream = File.OpenWrite(outputFile))
				{
					stream.WriteStream(new PspMemoryStream(this).SliceWithBounds(MainSegment.Low, MainSegment.High - 1));
					stream.Flush();
					stream.Close();
				}
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception);
			}
		}

		bool IPspMemoryInfo.IsAddressValid(uint address)
		{
			return PspMemory.IsAddressValid(address);
		}
	}
}
