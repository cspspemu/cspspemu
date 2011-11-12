using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core
{
	unsafe abstract public class PspMemory : IResetable
	{
		public class InvalidAddressException : Exception {
			public InvalidAddressException(string message) : base (message) { }
			public InvalidAddressException(string message, Exception innerException) : base(message, innerException) { }
			public InvalidAddressException(uint Address) : base(String.Format("Invalid Address : 0x%08X".Sprintf(Address))) {
			}
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

		public Segment ScratchPadSegment = new Segment(ScratchPadOffset, ScratchPadSize);
		public Segment MainSegment = new Segment(MainOffset, MainSize);
		public Segment FrameBufferSegment = new Segment(FrameBufferOffset, FrameBufferSize);

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

		virtual public void* PspAddressToPointerSafe(uint Address)
		{
			if (Address == 0) return null;
			if (!IsAddressValid(Address)) throw(new InvalidAddressException(Address));
			return PspAddressToPointer(Address);
		}

		public bool IsAddressValid(uint _Address)
		{
			var Address = _Address & 0x1FFFFFFF;
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
