using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Memory
{
	public unsafe sealed class FastPspMemory : PspMemory
	{
		override public bool HasFixedGlobalAddress { get { return true; } }
		override public IntPtr FixedGlobalAddress { get { return new IntPtr(Base); } }

		//public const uint FastMemorySize = 0x0A000000;
		//public const uint FastMemoryMask = 0x1FFFFFFF;
		public const uint FastMemoryMask = 0x0FFFFFFF;
		public const uint FastMemorySize = FastMemoryMask + 1;
		

		public static byte* Base = null;

		public FastPspMemory()
		{
			AllocMemoryOnce();
		}

		~FastPspMemory()
		{
			Dispose();
			//FreeMemory();
		}

		private static bool _alreadyInitialized = false;

		private void AllocMemoryOnce()
		{
			if (!_alreadyInitialized)
			{
				_alreadyInitialized = true;

				Base = (byte *)Marshal.AllocHGlobal((int)FastMemorySize).ToPointer();

				//Console.WriteLine("*****************************");
			}

			NullPtr = Base;
			ScratchPadPtr = Base + ScratchPadOffset;
			FrameBufferPtr = Base + FrameBufferOffset;
			MainPtr = Base + MainOffset;
		}

		public override uint PointerToPspAddressUnsafe(void* pointer)
		{
			if (pointer == null) return 0;
			return (uint)((byte*)pointer - Base);
		}

		public override void* PspAddressToPointerUnsafe(uint address)
		{
			var finalAddress = (address & FastPspMemory.FastMemoryMask);
			//Console.WriteLine("Base: 0x{0:X} ; Address: 0x{1:X}", (ulong)Base, Address);
			if (finalAddress == 0) return null;
#if false
			if (_Base == null) throw(new InvalidProgramException("Base is null"));
#endif
			return Base + finalAddress;
		}

		public override void Dispose()
		{
		}
	}
}