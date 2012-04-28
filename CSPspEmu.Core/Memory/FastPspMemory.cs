using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;
using System.Diagnostics;

namespace CSPspEmu.Core.Memory
{
	unsafe sealed public class FastPspMemory : PspMemory
	{
		//readonly public byte* Base = (byte*)0x50000000;
		//readonly public byte* Base = (byte*)0x40000000;
		readonly public byte* Base = (byte*)0x20000000;
		static public byte* StaticNullPtr;
		static public byte* StaticScratchPadPtr;
		static public byte* StaticFrameBufferPtr;
		static public byte* StaticMainPtr;

		/*
		// to RESERVE memory in Linux, use mmap with a private, anonymous, non-accessible mapping.
		// The following line reserves 1gb of ram starting at 0x10000000.

		void* result = mmap((void*)0x10000000, 0x40000000, PROT_NONE, MAP_PRIVATE | MAP_ANON, -1, 0);

		// to COMMIT memory in Linux, use mprotect on the range of memory you'd like to commit, and
		// grant the memory READ and/or WRITE access.
		// The following line commits 1mb of the buffer.  It will return -1 on out of memory errors.

		int result3 = mprotect((void*)0x10000000, 0x100000, PROT_READ | PROT_WRITE);
		*/

		public override void InitializeComponent()
		{
			AllocMemoryOnce();
		}

		~FastPspMemory()
		{
			Dispose();
			//FreeMemory();
		}

		static bool AlreadyInitialized = false;

		private void AllocMemoryOnce()
		{
			if (!AlreadyInitialized)
			{
				AlreadyInitialized = true;

				Logger.Info("FastPspMemory.AllocMemory");
				StaticNullPtr = Base;
				StaticScratchPadPtr = (byte *)Platform.AllocRange(Base + ScratchPadOffset, ScratchPadSize);
				StaticFrameBufferPtr = (byte*)Platform.AllocRange(Base + FrameBufferOffset, FrameBufferSize);
				StaticMainPtr = (byte*)Platform.AllocRange(Base + MainOffset, MainSize);
				if (StaticScratchPadPtr == null || StaticFrameBufferPtr == null || StaticMainPtr == null)
				{
					Logger.Fatal("Can't allocate virtual memory!");
					Debug.Fail("Can't allocate virtual memory!");
					throw (new InvalidOperationException());
				}
			}
			NullPtr = StaticNullPtr;
			ScratchPadPtr = StaticScratchPadPtr;
			FrameBufferPtr = StaticFrameBufferPtr;
			MainPtr = StaticMainPtr;
		}

		/*
		private void AllocMemory()
		{
			Logger.Info("FastPspMemory.AllocMemory");
			ScratchPadPtr = VirtualAlloc(Base + ScratchPadOffset, ScratchPadSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			FrameBufferPtr = VirtualAlloc(Base + FrameBufferOffset, FrameBufferSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			MainPtr = VirtualAlloc(Base + MainOffset, MainSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			if (ScratchPadPtr == null || FrameBufferPtr == null || MainPtr == null)
			{
				Logger.Fatal("Can't allocate virtual memory!");
				throw (new InvalidOperationException());
			}
		}

		private void FreeMemory()
		{
			Logger.Info("FastPspMemory.FreeMemory");
			if (ScratchPadPtr != null)
			{
				if (!VirtualFree(ScratchPadPtr, ScratchPadSize, MEM_DECOMMIT | MEM_RELEASE))
				{
				}
				if (!VirtualFree(FrameBufferPtr, FrameBufferSize, MEM_DECOMMIT | MEM_RELEASE))
				{
				}
				if (!VirtualFree(MainPtr, MainSize, MEM_DECOMMIT | MEM_RELEASE))
				{
				}
				ScratchPadPtr = null;
				FrameBufferPtr = null;
				MainPtr = null;
			}
		}
		*/

		public override uint PointerToPspAddressUnsafe(void* Pointer)
		{
			if (Pointer == null) return 0;
			return (uint)((byte*)Pointer - Base);
		}

		public override void* PspAddressToPointerUnsafe(uint _Address)
		{
			var Address = (_Address & PspMemory.MemoryMask);
			if (Address == 0) return null;
			return Base + Address;
		}

		public override void Dispose()
		{
			Reset();
			//FreeMemory();
		}
	}
}