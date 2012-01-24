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
		//readonly public byte* Base = (byte*)0x40000000;
		readonly public byte* Base = (byte*)0x20000000;
		static public byte* StaticNullPtr;
		static public byte* StaticScratchPadPtr;
		static public byte* StaticFrameBufferPtr;
		static public byte* StaticMainPtr;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern byte* VirtualAlloc(void* lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool VirtualFree(void* lpAddress, uint dwSize, uint dwFreeType);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern uint GetLastError();

		[DllImport("libc", EntryPoint = "mmap")]
		internal static extern void* mmap(void* addr, uint len, uint prot, uint flags, uint off_t);

		[DllImport("libc", EntryPoint = "mprotect")]
		public static extern int mprotect(void* start, ulong len, uint prot);

		/*
		// to RESERVE memory in Linux, use mmap with a private, anonymous, non-accessible mapping.
		// The following line reserves 1gb of ram starting at 0x10000000.

		void* result = mmap((void*)0x10000000, 0x40000000, PROT_NONE, MAP_PRIVATE | MAP_ANON, -1, 0);

		// to COMMIT memory in Linux, use mprotect on the range of memory you'd like to commit, and
		// grant the memory READ and/or WRITE access.
		// The following line commits 1mb of the buffer.  It will return -1 on out of memory errors.

		int result3 = mprotect((void*)0x10000000, 0x100000, PROT_READ | PROT_WRITE);
		*/

		const uint MEM_RESERVE = 0x2000;
		const uint MEM_COMMIT = 0x1000;
		const uint PAGE_READWRITE = 0x04;

		const uint MEM_DECOMMIT = 0x4000;
		const uint MEM_RELEASE = 0x8000;

		const uint MAP_ANON = 1;
		const uint MAP_ANONYMOUS = 1;
		const uint MAP_FILE = 2;
		const uint MAP_PRIVATE = 4;
		const uint MAP_SHARED = 8;
		const uint PROT_NONE = 0;
		const uint PROT_READ = 1;
		const uint PROT_WRITE = 2;
		const uint PROT_EXEC = 4;

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

		private void* AllocRange(void* Address, uint Size)
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				case PlatformID.Win32S:
					return VirtualAlloc(Address, Size, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
				case PlatformID.Unix:
					void* result = mmap(Address, Size, PROT_NONE, MAP_PRIVATE | MAP_ANON, 0);
					int result3 = mprotect(Address, Size, PROT_READ | PROT_WRITE);
					return result;
			}
			throw(new NotImplementedException());
		}

		private void AllocMemoryOnce()
		{
			if (!AlreadyInitialized)
			{
				AlreadyInitialized = true;
				ConsoleUtils.SaveRestoreConsoleState(() =>
				{
					Console.BackgroundColor = ConsoleColor.Yellow;
					Console.ForegroundColor = ConsoleColor.Black;
					Console.WriteLine("FastPspMemory.AllocMemory");
				});
				StaticNullPtr = Base;
				StaticScratchPadPtr = (byte *)AllocRange(Base + ScratchPadOffset, ScratchPadSize);
				StaticFrameBufferPtr = (byte*)AllocRange(Base + FrameBufferOffset, FrameBufferSize);
				StaticMainPtr = (byte*)AllocRange(Base + MainOffset, MainSize);
				if (StaticScratchPadPtr == null || StaticFrameBufferPtr == null || StaticMainPtr == null)
				{
					Console.WriteLine("Can't allocate virtual memory!");
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
			ConsoleUtils.SaveRestoreConsoleState(() =>
			{
				Console.BackgroundColor = ConsoleColor.Yellow;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.WriteLine("FastPspMemory.AllocMemory");
			});
			ScratchPadPtr = VirtualAlloc(Base + ScratchPadOffset, ScratchPadSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			FrameBufferPtr = VirtualAlloc(Base + FrameBufferOffset, FrameBufferSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			MainPtr = VirtualAlloc(Base + MainOffset, MainSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
			if (ScratchPadPtr == null || FrameBufferPtr == null || MainPtr == null)
			{
				Console.WriteLine("Can't allocate virtual memory!");
				throw (new InvalidOperationException());
			}
		}

		private void FreeMemory()
		{
			ConsoleUtils.SaveRestoreConsoleState(() =>
			{
				Console.BackgroundColor = ConsoleColor.Yellow;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.WriteLine("FastPspMemory.FreeMemory");
			});
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

		public override uint PointerToPspAddress(void* Pointer)
		{
			if (Pointer == null) return 0;
			return (uint)((byte*)Pointer - Base);
		}

		public override void* PspAddressToPointer(uint _Address)
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