#define USE_ARRAY_BYTES

#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;
using CSharpUtils;

namespace CSPspEmu.Core.Memory
{
	unsafe public class NormalPspMemory : PspMemory
	{
		public override void InitializeComponent()
		{
			Initialize();
		}

		/*
		~NormalPspMemory()
		{
			Dispose();
		}
		*/

		virtual protected void Initialize()
		{
			AllocateMemory();
		}

#if USE_ARRAY_BYTES
		byte[] ScratchPad;
		byte[] FrameBuffer;
		byte[] Main;
		uint[] LogMain;

		GCHandle ScratchPadHandle;
		GCHandle FrameBufferHandle;
		GCHandle MainHandle;
		GCHandle LogMainHandle;
#endif

		protected void AllocateMemory()
		{
			if (ScratchPadPtr == null)
			{
				NullPtr = null;

#if USE_ARRAY_BYTES
				ScratchPad = new byte[ScratchPadSize];
				FrameBuffer = new byte[FrameBufferSize];
				Main = new byte[MainSize];
				LogMain = new uint[MainSize];

				ScratchPadHandle = GCHandle.Alloc(ScratchPad, GCHandleType.Pinned);
				FrameBufferHandle = GCHandle.Alloc(FrameBuffer, GCHandleType.Pinned);
				MainHandle = GCHandle.Alloc(Main, GCHandleType.Pinned);
				LogMainHandle = GCHandle.Alloc(LogMain, GCHandleType.Pinned);

				ScratchPadPtr = (byte*)ScratchPadHandle.AddrOfPinnedObject().ToPointer();
				FrameBufferPtr = (byte*)FrameBufferHandle.AddrOfPinnedObject().ToPointer();
				MainPtr = (byte*)MainHandle.AddrOfPinnedObject().ToPointer();
				LogMainPtr = (uint*)LogMainHandle.AddrOfPinnedObject().ToPointer();
#else
				ScratchPadPtr = (byte*)(Marshal.AllocHGlobal(ScratchPadSize).ToPointer());
				FrameBufferPtr = (byte*)(Marshal.AllocHGlobal(FrameBufferSize).ToPointer());
				MainPtr = (byte*)(Marshal.AllocHGlobal(MainSize).ToPointer());
				LogMainPtr = (uint*)(Marshal.AllocHGlobal(MainSize * 4).ToPointer());

				/*
				PointerUtils.Memset(ScratchPadPtr, 0, ScratchPadSize);
				PointerUtils.Memset(FrameBufferPtr, 0, FrameBufferSize);
				PointerUtils.Memset(MainPtr, 0, MainSize);
				PointerUtils.Memset((byte*)LogMainPtr, 0, MainSize * 4);
				*/
#endif

				//for (int n = 0; n < 1000; n++) Console.WriteLine("{0}", ScratchPadPtr[n]);
				//GCHandle.Alloc();
			}
		}

		protected void FreeMemory()
		{
			if (ScratchPadPtr != null)
			{
#if USE_ARRAY_BYTES
				ScratchPad = null;
				FrameBuffer = null;
				Main = null;
				LogMain = null;

				ScratchPadHandle.Free();
				FrameBufferHandle.Free();
				MainHandle.Free();
				LogMainHandle.Free();
#else
				Marshal.FreeHGlobal(new IntPtr(ScratchPadPtr));
				Marshal.FreeHGlobal(new IntPtr(FrameBufferPtr));
				Marshal.FreeHGlobal(new IntPtr(MainPtr));
				Marshal.FreeHGlobal(new IntPtr(LogMainPtr));
#endif
				ScratchPadPtr = null;
				FrameBufferPtr = null;
				MainPtr = null;
				LogMainPtr = null;
			}
		}

		override public void SetPCWriteAddress(uint _Address, uint PC)
		{
			var Address = _Address & PspMemory.MemoryMask;
			if (Address >= MainOffset && Address < MainOffset + MainSize)
			{
				LogMainPtr[Address - MainOffset] = PC;
			}
		}

		override public uint GetPCWriteAddress(uint _Address)
		{
			var Address = _Address & PspMemory.MemoryMask;
			if (Address >= MainOffset && Address < MainOffset + MainSize)
			{
				return LogMainPtr[Address - MainOffset];
			}
			return 0xFFFFFFFF;
		}

		override public uint PointerToPspAddress(void* Pointer)
		{
			if (Pointer == null) return 0;
			if (Pointer >= &ScratchPadPtr[0] && Pointer < &ScratchPadPtr[ScratchPadSize]) return (uint)((byte*)Pointer - ScratchPadPtr);
			if (Pointer >= &FrameBufferPtr[0] && Pointer < &FrameBufferPtr[FrameBufferSize]) return (uint)((byte*)Pointer - FrameBufferPtr);
			if (Pointer >= &MainPtr[0] && Pointer < &MainPtr[MainSize]) return (uint)((byte*)Pointer - MainPtr);
			throw (new InvalidOperationException(String.Format("Address 0x{0:X} is not a pointer to the PspMemory", (uint)Pointer)));
		}

		override public void* PspAddressToPointer(uint _Address)
		{
			if (_Address == 0) return null;
			//if (IsAddressValid(_Address))
			{
				// Ignore last 3 bits (cache / kernel)
				var Address = _Address & PspMemory.MemoryMask;
				switch (Address >> 24)
				{
					/////// hp
					case 0x00: //case 0b_00000:
						if (Address < ScratchPadOffset)
						{
							break;
						}
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
			}
			throw (new InvalidAddressException(_Address));
		}

		public override void Dispose()
		{
			FreeMemory();
		}
	}
}
#endif