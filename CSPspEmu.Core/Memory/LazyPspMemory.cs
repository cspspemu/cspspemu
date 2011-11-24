using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Memory
{
	unsafe public class LazyPspMemory : NormalPspMemory
	{
		public LazyPspMemory(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
		}

		protected override void Initialize()
		{
		}

		private void LazyCreateMemory()
		{
			if (ScratchPadPtr == null)
			{
				AllocateMemory();
			}
		}

		override public uint PointerToPspAddress(void* Pointer)
		{
			LazyCreateMemory();
			return base.PointerToPspAddress(Pointer);
		}

		override public void* PspAddressToPointer(uint _Address)
		{
			LazyCreateMemory();
			return base.PspAddressToPointer(_Address);
		}
	}
}
