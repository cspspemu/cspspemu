using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Threading.Semaphores;
using CSharpUtils;

namespace CSPspEmu.Hle.Threading.Semaphores
{
	unsafe public class HleSemaphore
	{
		public SceKernelSemaInfo SceKernelSemaInfo;

		public String Name
		{
			get
			{
				fixed (byte* NamePtr = SceKernelSemaInfo.Name) return PointerUtils.PtrToString(NamePtr, Encoding.ASCII);
			}
			set
			{
				fixed (byte* NamePtr = SceKernelSemaInfo.Name) PointerUtils.StoreStringOnPtr(value, Encoding.ASCII, NamePtr);
			}
		}

		public void IncrementCount(int Signal)
		{
			throw new NotImplementedException();
		}
	}
}
