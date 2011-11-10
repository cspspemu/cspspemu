using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser : HleModuleHost
	{
		/*
		
		public uint Test(CpuThreadState CpuThreadState, int a, int b, string c)
		{
			//Console.WriteLine(CpuThreadState);
			return (uint)a + (uint)b;
		}
		*/

		/// <summary>
		/// Get the low 32bits of the current system time (microseconds)
		/// </summary>
		/// <returns>The low 32bits of the system time</returns>
		/// 
		[HlePspFunction(NID = 0x369ED59D, FirmwareVersion = 150)]
		public uint sceKernelGetSystemTimeLow()
		{
			HleState.PspRtc.Update();
			return (uint)(long)(HleState.PspRtc.Elapsed.TotalMilliseconds * 1000);
		}

	}
}
