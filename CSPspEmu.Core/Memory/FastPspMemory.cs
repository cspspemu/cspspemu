using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils;

namespace CSPspEmu.Core.Memory
{
	unsafe sealed public class FastPspMemory : PspMemory
	{
		readonly public byte* Base = (byte*)0x20000000;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern byte* VirtualAlloc(void* lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool VirtualFree(void* lpAddress, uint dwSize, uint dwFreeType);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern uint GetLastError();

		const uint MEM_RESERVE = 0x2000;
		const uint MEM_COMMIT = 0x1000;
		const uint PAGE_READWRITE = 0x04;

		const uint MEM_DECOMMIT = 0x4000;
		const uint MEM_RELEASE = 0x8000;

		public FastPspMemory(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			//AllocMemory();
			AllocMemoryOnce();
		}

		~FastPspMemory()
		{
			//FreeMemory();
		}

		static bool AlreadyInitialized = false;

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
				ScratchPadPtr = VirtualAlloc(Base + ScratchPadOffset, ScratchPadSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
				FrameBufferPtr = VirtualAlloc(Base + FrameBufferOffset, FrameBufferSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
				MainPtr = VirtualAlloc(Base + MainOffset, MainSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
				if (ScratchPadPtr == null || FrameBufferPtr == null || MainPtr == null)
				{
					Console.WriteLine("Can't allocate virtual memory!");
					throw (new InvalidOperationException());
				}
			}
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