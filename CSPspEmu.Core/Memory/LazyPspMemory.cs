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

		override public uint PointerToPspAddressUnsafe(void* Pointer)
		{
			LazyCreateMemory();
			return base.PointerToPspAddressUnsafe(Pointer);
		}

		override public void* PspAddressToPointerUnsafe(uint _Address)
		{
			LazyCreateMemory();
			return base.PspAddressToPointerUnsafe(_Address);
		}
	}
}
