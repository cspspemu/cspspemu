using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	unsafe abstract public class PspMemory
	{
		/*
		public struct Segment
		{
			public uint Offset;
			public uint Size;
		}
		*/

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

		abstract public uint PointerToPspAddress(void* Pointer);
		abstract public void* PspAddressToPointer(uint Address);

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
	}
}
