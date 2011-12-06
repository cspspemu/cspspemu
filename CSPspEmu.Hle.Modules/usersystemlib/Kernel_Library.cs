using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.usersystemlib
{
	public class Kernel_Library : HleModuleHost
	{
		/*
		void initNids() {
			
			
		}
	
		const int Enabled  = 1;
		const int Disabled = 0;
	
		ref bool enabledInterrupts() {
			return hleEmulatorState.emulatorState.enabledInterrupts;
		}
		*/

		/// <summary>
		/// Suspend all interrupts.
		/// </summary>
		/// <returns>The current state of the interrupt controller, to be used with ::sceKernelCpuResumeIntr().</returns>
		[HlePspFunction(NID = 0x092968F4, FirmwareVersion = 150)]
		public uint sceKernelCpuSuspendIntr()
		{
			return HleState.HleInterruptManager.sceKernelCpuSuspendIntr();
		}

		/// <summary>
		/// Resume/Enable all interrupts.
		/// </summary>
		/// <param name="Flags">The value returned from ::sceKernelCpuSuspendIntr().</param>
		[HlePspFunction(NID = 0x5F10D406, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntr(uint Flags)
		{
			HleState.HleInterruptManager.sceKernelCpuResumeIntr(Flags);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Flags"></param>
		[HlePspFunction(NID = 0x3B84732D, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntrWithSync(uint Flags)
		{
			sceKernelCpuResumeIntr(Flags);
		}
	}
}
