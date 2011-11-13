using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.sysmem
{
	public class KDebugForKernel : HleModuleHost
	{
		//mixin(registerFunction!(0x84F370BC, Kprintf));
		[HlePspFunction(NID = 0x84F370BC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void Kprintf(string Format, CpuThreadState CpuThreadState)
		{
		}
	}
}
