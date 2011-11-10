using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.power
{
	public class scePower : HleModuleHost
	{
		/// <summary>
		/// Set CPU Frequency
		/// </summary>
		/// <param name="CpuFrequency">new CPU frequency, valid values are 1 - 333</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x843FBF43, FirmwareVersion = 150)]
		public int scePowerSetCpuClockFrequency(int CpuFrequency)
		{
			HleState.PspConfig.CpuFrequency = CpuFrequency;
			return 0;
		}


		/// <summary>
		/// Get CPU Frequency as Integer
		/// </summary>
		/// <returns>frequency as int</returns>
		[HlePspFunction(NID = 0xFDB5BFE9, FirmwareVersion = 150)]
		public int scePowerGetCpuClockFrequencyInt()
		{
			return HleState.PspConfig.CpuFrequency;
		}
	}
}
