﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Memory
{
	unsafe abstract public class PspMemory : PspEmulatorComponent, IResetable, IDisposable
	{
		public const uint MemoryMask = 0x1FFFFFFF;

		public class InvalidAddressException : Exception {
			public InvalidAddressException(string message) : base (message) { }
			public InvalidAddressException(string message, Exception innerException) : base(message, innerException) { }
			//public InvalidAddressException(uint Address) : base(String.Format("Invalid Address : 0x%08X".Sprintf(Address))) { }
			//public InvalidAddressException(uint Address, Exception innerException) : base(String.Format("Invalid Address : 0x%08X".Sprintf(Address)), innerException) { }
			public InvalidAddressException(uint Address) : base(String.Format("Invalid Address : 0x{0:X}", Address)) { }
			public InvalidAddressException(uint Address, Exception innerException) : base(String.Format("Invalid Address : 0x{0:X}", Address), innerException) { }
		}

		sealed public class Segment
		{
			public uint Low { get; private set; }
			public uint High { get; private set; }
			public int Size { get { return (int)(High - Low); } }

			public Segment(uint Offset, uint Size)
			{
				this.Low = Offset;
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
			PointerUtils.Memset((byte *)PspAddressToPointer(Segment.Low), 0, Segment.Size);
		}

		public readonly Segment ScratchPadSegment = new Segment(ScratchPadOffset, ScratchPadSize);
		public readonly Segment MainSegment = new Segment(MainOffset, MainSize);
		public readonly Segment FrameBufferSegment = new Segment(FrameBufferOffset, FrameBufferSize);

		//static public Segment ScratchPadSegment = new Segment() { Offset = 0x00010000, Size = 4 * 1024 };

		public const int ScratchPadSize = 4 * 1024; // 4KB
		public const int FrameBufferSize = 2 * 0x100000; // 2MB
		public const int MainSize = 64 * 0x100000; // 64MB (SLIM)

		public const uint ScratchPadOffset = 0x00010000;
		public const uint FrameBufferOffset = 0x04000000;
		public const uint MainOffset = 0x08000000;

		protected byte* ScratchPadPtr; // 4KB
		protected byte* FrameBufferPtr; // 2MB
		protected byte* MainPtr;
		protected uint* LogMainPtr;

		abstract public uint PointerToPspAddress(void* Pointer);
		abstract public void* PspAddressToPointer(uint Address);

		public PspPointer PointerToPspPointer(void* Pointer)
		{
			return new PspPointer(PointerToPspAddressSafe(Pointer));
		}

		public void* PspPointerToPointerSafe(PspPointer Pointer)
		{
			return PspAddressToPointerSafe(Pointer.Address);
		}

		virtual public uint PointerToPspAddressSafe(void* Pointer)
		{
			return PointerToPspAddressSafe((byte*)Pointer);
		}

		virtual public uint PointerToPspAddressSafe(byte* Pointer)
		{
			if (Pointer == null) return 0;
			if ((Pointer >= ScratchPadPtr) && (Pointer < ScratchPadPtr + ScratchPadSize)) return (uint)(ScratchPadOffset + (Pointer - ScratchPadPtr));
			if ((Pointer >= FrameBufferPtr) && (Pointer < FrameBufferPtr + FrameBufferSize)) return (uint)(FrameBufferOffset + (Pointer - FrameBufferPtr));
			if ((Pointer >= MainPtr) && (Pointer < MainPtr + MainSize)) return (uint)(MainOffset + (Pointer - MainPtr));
			throw (new InvalidAddressException("Pointer doesn't belong to PSP Memory"));
		}

		virtual public void SetPCWriteAddress(uint Address, uint PC)
		{
		}

		virtual public uint GetPCWriteAddress(uint Address)
		{
			return 0xFFFFFFFF;
		}

		virtual public void* PspAddressToPointerSafe(uint Address, bool CanBeNull = true)
		{
			if (Address == 0 && CanBeNull) return null;
			if (!IsAddressValid(Address)) throw(new InvalidAddressException(Address));
			return PspAddressToPointer(Address);
		}

		public void CheckAndEnforceAddressValid(uint Address)
		{
			if (!IsAddressValid(Address)) throw(new InvalidAddressException(Address));
		}

		public bool IsAddressValid(uint _Address)
		{
			var Address = _Address & PspMemory.MemoryMask;
			if (MainSegment.Contains(Address)) return true;
			if (FrameBufferSegment.Contains(Address)) return true;
			if (ScratchPadSegment.Contains(Address)) return true;
			return false;
		}

		public void Write1(uint Address, byte Value)
		{
			*((byte*)PspAddressToPointer(Address)) = Value;
		}

		public void Write2(uint Address, ushort Value)
		{
			*((ushort*)PspAddressToPointer(Address)) = Value;
		}

		public void Write4(uint Address, uint Value)
		{
			*((uint*)PspAddressToPointer(Address)) = Value;
		}

		public void Write8(uint Address, ulong Value)
		{
			*((ulong*)PspAddressToPointer(Address)) = Value;
		}

		public void WriteBytes(uint Address, byte[] DataIn)
		{
			Marshal.Copy(DataIn, 0, new IntPtr(PspAddressToPointer(Address)), DataIn.Length);
		}

		public void WriteBytes(uint Address, byte* DataInPointer, int DataInLength)
		{
			PointerUtils.Memcpy((byte*)PspAddressToPointer(Address), DataInPointer, DataInLength);
		}

		public void WriteStruct<TType>(uint Address, TType Value) where TType : struct
		{
			WriteBytes(Address, StructUtils.StructToBytes(Value));
		}

		public void WriteRepeated1(byte Value, uint Address, int Count)
		{
			for (int n = 0; n < Count; n++)
			{
				Write1((uint)(Address + n), Value);
			}
		}

		public byte Read1(uint Address)
		{
			return *((byte*)PspAddressToPointer(Address));
		}

		public ushort Read2(uint Address)
		{
			return *((ushort*)PspAddressToPointer(Address));
		}

		public uint Read4(uint Address)
		{
			return *((uint*)PspAddressToPointer(Address));
		}

		public ulong Read8(uint Address)
		{
			return *((ulong*)PspAddressToPointer(Address));
		}

		public byte[] ReadBytes(uint Address, int Count)
		{
			var Output = new byte[Count];
			Marshal.Copy(new IntPtr(PspAddressToPointer(Address)), Output, 0, Output.Length);
			return Output;
		}

		public void ReadBytes(uint Address, byte* DataOutPointer, int DataOutLength)
		{
			PointerUtils.Memcpy(DataOutPointer, (byte*)PspAddressToPointer(Address), DataOutLength);
		}

		public TType ReadStruct<TType>(uint Address) where TType : struct
		{
			return StructUtils.BytesToStruct<TType>(ReadBytes(Address, Marshal.SizeOf(typeof(TType))));
		}

		abstract public void Dispose();

		public void Copy(uint SourceAddress, uint DestinationAddress, int Size)
		{
			var Source = PspAddressToPointer(SourceAddress);
			var Destination = PspAddressToPointer(DestinationAddress);
			PointerUtils.Memcpy((byte*)Destination, (byte*)Source, Size);
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
	}
}
