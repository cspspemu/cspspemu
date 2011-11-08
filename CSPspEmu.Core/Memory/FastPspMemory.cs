using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core
{
	unsafe sealed public class FastPspMemory : PspMemory
	{
		readonly public byte* Base = (byte*)0x20000000;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern byte* VirtualAlloc(void* lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool VirtualFree(void* lpAddress, uint dwSize, uint dwFreeType);

		const uint MEM_RESERVE = 0x2000;
		const uint MEM_COMMIT = 0x1000;
		const uint PAGE_READWRITE = 0x04;

		const uint MEM_DECOMMIT = 0x4000;
		const uint MEM_RELEASE = 0x8000;

		public FastPspMemory()
		{
			ScratchPadPtr  = VirtualAlloc(Base + ScratchPadOffset , ScratchPadSize , MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			FrameBufferPtr = VirtualAlloc(Base + FrameBufferOffset, FrameBufferSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			MainPtr        = VirtualAlloc(Base + MainOffset       , MainSize       , MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			if (ScratchPadPtr == null || FrameBufferPtr == null || MainPtr == null) throw(new InvalidOperationException());
		}

		~FastPspMemory()
		{
			VirtualFree(Base + ScratchPadOffset, 0, MEM_DECOMMIT | MEM_RELEASE);
			VirtualFree(Base + FrameBufferOffset, 0, MEM_DECOMMIT | MEM_RELEASE);
			VirtualFree(Base + MainOffset, 0, MEM_DECOMMIT | MEM_RELEASE);
		}

		public override uint PointerToPspAddress(void* Pointer)
		{
			if (Pointer == null) return 0;
			return (uint)((byte*)Pointer - Base);
		}

		public override void* PspAddressToPointer(uint Address)
		{
			if (Address == 0) return null;
			return Base + (Address & 0x1FFFFFFF);
		}
	}
}