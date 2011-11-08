﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core
{
	unsafe public class NormalPspMemory : PspMemory
	{
		public NormalPspMemory()
		{
			ScratchPadPtr  = (byte*)(Marshal.AllocHGlobal(ScratchPadSize).ToPointer());
			FrameBufferPtr = (byte*)(Marshal.AllocHGlobal(FrameBufferSize).ToPointer());
			MainPtr        = (byte*)(Marshal.AllocHGlobal(MainSize).ToPointer());
		}

		~NormalPspMemory()
		{
			Marshal.FreeHGlobal(new IntPtr(ScratchPadPtr));
			Marshal.FreeHGlobal(new IntPtr(FrameBufferPtr));
			Marshal.FreeHGlobal(new IntPtr(MainPtr));
		}

		override public uint PointerToPspAddress(void* Pointer)
		{
			if (Pointer >= &ScratchPadPtr[0] && Pointer < &ScratchPadPtr[ScratchPadSize]) return (uint)((byte*)Pointer - &ScratchPadPtr[0]);
			if (Pointer >= &FrameBufferPtr[0] && Pointer < &FrameBufferPtr[FrameBufferSize]) return (uint)((byte*)Pointer - &FrameBufferPtr[0]);
			if (Pointer >= &MainPtr[0] && Pointer < &MainPtr[MainSize]) return (uint)((byte*)Pointer - &MainPtr[0]);
			throw (new InvalidOperationException());
		}

		override public void* PspAddressToPointer(uint Address)
		{
			// Ignore last 3 bits (cache / kernel)
			Address &= 0x1FFFFFFF;
			switch (Address >> 24) {
				/////// hp
				case 0x00: //case 0b_00000:
					return &ScratchPadPtr[Address - ScratchPadOffset];
				/////// hp
				case 0x04: //case 0b_00100:
					return &FrameBufferPtr[Address - FrameBufferOffset];
				/////// hp
				case 0x08: //case 0b_01000:
				case 0x09: //case 0b_01001:
				case 0x0A: //case 0b_01010: // SLIM ONLY
				case 0x0B: //case 0b_01011: // SLIM ONLY
					return &MainPtr[Address - MainOffset];
				/////// hp
				case 0x37: //case 0b_11111: // HO IO2
					//return &Vectors[Address - 0x1fc00000];
				break;
				case 0x1C: //case 0b_11100: // HW IO1
				break;
				default:
				break;
			}
			throw(new InvalidOperationException());
		}
	}
}