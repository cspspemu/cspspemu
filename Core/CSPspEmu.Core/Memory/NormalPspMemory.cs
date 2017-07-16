//#define ENABLE_LOG_MEMORY
#define ADDITIONAL_CHECKS
#define USE_ARRAY_BYTES

using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Memory
{
	public unsafe class NormalPspMemory : PspMemory
	{
		override public bool HasFixedGlobalAddress { get { return false; } }
		override public IntPtr FixedGlobalAddress { get { return IntPtr.Zero; } }

		public NormalPspMemory()
		{
			AllocateMemory();
		}

#if USE_ARRAY_BYTES
		byte[] _scratchPad;
		byte[] _frameBuffer;
		byte[] _main;
		byte[] _vectors;
		uint[] _logMain;

		GCHandle _scratchPadHandle;
		GCHandle _frameBufferHandle;
		GCHandle _mainHandle;
		GCHandle _vectorsHandle;
		GCHandle _logMainHandle;
#endif

		protected void AllocateMemory()
		{
			if (ScratchPadPtr == null)
			{
				NullPtr = null;

#if USE_ARRAY_BYTES
				//if (ScratchPad != null)
				{
					_scratchPad = new byte[ScratchPadSize];
					_frameBuffer = new byte[FrameBufferSize];
					_main = new byte[MainSize];
					_vectors = new byte[VectorsSize];
#if ENABLE_LOG_MEMORY
					LogMain = new uint[MainSize];
#else
					_logMain = new uint[0];
#endif
				}

				_scratchPadHandle = GCHandle.Alloc(_scratchPad, GCHandleType.Pinned);
				_frameBufferHandle = GCHandle.Alloc(_frameBuffer, GCHandleType.Pinned);
				_mainHandle = GCHandle.Alloc(_main, GCHandleType.Pinned);
				_vectorsHandle = GCHandle.Alloc(_vectors, GCHandleType.Pinned);
				_logMainHandle = GCHandle.Alloc(_logMain, GCHandleType.Pinned);

				ScratchPadPtr = (byte*)_scratchPadHandle.AddrOfPinnedObject().ToPointer();
				FrameBufferPtr = (byte*)_frameBufferHandle.AddrOfPinnedObject().ToPointer();
				MainPtr = (byte*)_mainHandle.AddrOfPinnedObject().ToPointer();
				VectorsPtr = (byte*)_vectorsHandle.AddrOfPinnedObject().ToPointer();
				LogMainPtr = (uint*)_logMainHandle.AddrOfPinnedObject().ToPointer();
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

				//GCHandle.Alloc();
			}
		}

		protected void FreeMemory()
		{
			if (ScratchPadPtr != null)
			{
#if USE_ARRAY_BYTES
				_scratchPad = null;
				_frameBuffer = null;
				_main = null;
				_vectors = null;
				_logMain = null;

				_scratchPadHandle.Free();
				_frameBufferHandle.Free();
				_mainHandle.Free();
				_vectorsHandle.Free();
				_logMainHandle.Free();
#else
				Marshal.FreeHGlobal(new IntPtr(ScratchPadPtr));
				Marshal.FreeHGlobal(new IntPtr(FrameBufferPtr));
				Marshal.FreeHGlobal(new IntPtr(MainPtr));
				Marshal.FreeHGlobal(new IntPtr(LogMainPtr));
#endif
				ScratchPadPtr = null;
				FrameBufferPtr = null;
				MainPtr = null;
				VectorsPtr = null;
				LogMainPtr = null;
			}
		}

		public override void SetPCWriteAddress(uint address, uint pc)
		{
#if ENABLE_LOG_MEMORY
			var Address = _Address & PspMemory.MemoryMask;
			if (Address >= MainOffset && Address < MainOffset + MainSize)
			{
				LogMainPtr[Address - MainOffset] = PC;
			}
#endif
		}

		public override uint GetPCWriteAddress(uint address)
		{
#if ENABLE_LOG_MEMORY
			var Address = _Address & PspMemory.MemoryMask;
			if (Address >= MainOffset && Address < MainOffset + MainSize)
			{
				return LogMainPtr[Address - MainOffset];
			}
#endif
			return 0xFFFFFFFF;
		}

		public override uint PointerToPspAddressUnsafe(void* pointer)
		{
			/*
			if (Pointer == null) return 0;
			if (Pointer >= &ScratchPadPtr[0] && Pointer < &ScratchPadPtr[ScratchPadSize]) return (uint)((byte*)Pointer - ScratchPadPtr);
			if (Pointer >= &FrameBufferPtr[0] && Pointer < &FrameBufferPtr[FrameBufferSize]) return (uint)((byte*)Pointer - FrameBufferPtr);
			if (Pointer >= &MainPtr[0] && Pointer < &MainPtr[MainSize]) return (uint)((byte*)Pointer - MainPtr);
			if (Pointer >= &HardwareVectorsPtr[0] && Pointer < &HardwareVectorsPtr[MainSize]) return (uint)((byte*)Pointer - HardwareVectorsPtr);
			throw (new InvalidOperationException(String.Format("Address 0x{0:X} is not a pointer to the PspMemory", (uint)Pointer)));
			 * */
			return PointerToPspAddressSafe(pointer);
		}

		public override void* PspAddressToPointerUnsafe(uint address)
		{
			if (address == 0) return null;
			//if (IsAddressValid(_Address))
			{
				// Ignore last 3 bits (cache / kernel)
				var faddress = address & MemoryMask;
				switch (faddress >> 24)
				{
					/////// hp
					case 0x00: //case 0b_00000:
						{
							if (faddress < ScratchPadOffset)
							{
								break;
							}
							var offset = faddress - ScratchPadOffset;
#if ADDITIONAL_CHECKS
							if (offset >= ScratchPadSize) throw (new Exception(String.Format("Outside! 0x{0:X}", faddress)));
#endif
							return &ScratchPadPtr[faddress - ScratchPadOffset];
						}
					/////// hp
					case 0x04: //case 0b_00100:
						{
							var offset = faddress - FrameBufferOffset;
#if ADDITIONAL_CHECKS
							if (offset >= FrameBufferSize) throw (new Exception(String.Format("Outside! 0x{0:X}", faddress)));
#endif

							return &FrameBufferPtr[offset];
						}
					/////// hp
					case 0x08: //case 0b_01000:
					case 0x09: //case 0b_01001:
					case 0x0A: //case 0b_01010: // SLIM ONLY
					case 0x0B: //case 0b_01011: // SLIM ONLY
						{
							var offset = faddress - MainOffset;
#if ADDITIONAL_CHECKS
							if (offset >= MainSize) throw (new Exception(String.Format("Outside! 0x{0:X}", faddress)));
#endif

							return &MainPtr[offset];
							//return &Main[Offset];
						}
					/////// hp
					case 0x1F: //case 0b_011111
					case 0x37: //case 0b_111111: // HO IO2
						{
							//return &Vectors[Address - 0x1fc00000];
							//return HardwareVectors
							var offset = faddress - VectorsOffset;
#if ADDITIONAL_CHECKS
							if (offset >= VectorsSize) throw (new Exception(String.Format("Outside! 0x{0:X}", faddress)));
#endif
							return &VectorsPtr[offset];
						}
					case 0x1C: //case 0b_11100: // HW IO1
						break;
				}
				Console.Error.WriteLine("0x{0:X2}", (faddress >> 24));
			}
			throw (new InvalidAddressException(address));
		}

		public override void Dispose()
		{
			FreeMemory();
		}
	}
}