using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.sysmem
{
	public class KDebugForKernel : HleModuleHost
	{
		public enum PspDebugKprintfHandler : uint { }

		/// <summary>
		/// Install a Kprintf handler into the system.
		/// </summary>
		/// <param name="Handler">Function pointer to the handler.</param>
		/// <returns>less than 0 on error.</returns>
		[HlePspFunction(NID = 0x7CEB2C09, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelRegisterKprintfHandler(PspDebugKprintfHandler Handler)
		{
			return 0;
			//throw(new NotImplementedException());
			/*
			Logger.log(Logger.Level.WARNING, "KDebugForKernel", "Not implemented sceKernelRegisterKprintfHandler");
			return -1;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Format"></param>
		/// <param name="CpuThreadState"></param>
		[HlePspFunction(NID = 0x84F370BC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void Kprintf(string Format, CpuThreadState CpuThreadState)
		{
		}
	}
}
