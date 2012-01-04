using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.usersystemlib
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
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
		/// Resume all interrupts (using sync instructions).
		/// </summary>
		/// <param name="Flags">The value returned from ::sceKernelCpuSuspendIntr()</param>
		[HlePspFunction(NID = 0x3B84732D, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntrWithSync(uint Flags)
		{
			sceKernelCpuResumeIntr(Flags);
		}

		/// <summary>
		/// Determine if interrupts are suspended or active, based on the given flags.
		/// </summary>
		/// <param name="flagInterrupts">The value returned from ::sceKernelCpuSuspendIntr().</param>
		/// <returns>1 if flags indicate that interrupts were not suspended, 0 otherwise.</returns>
		[HlePspFunction(NID = 0x47A0B729, FirmwareVersion = 150)]
		public bool sceKernelIsCpuIntrSuspended(int flagInterrupts)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Determine if interrupts are enabled or disabled.
		/// </summary>
		/// <returns>1 if interrupts are currently enabled.</returns>
		[HlePspFunction(NID = 0xB55249D2, FirmwareVersion = 150)]
		public bool sceKernelIsCpuIntrEnable()
		{
			throw(new NotImplementedException());
		}
	}
}
