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
			/*
			synchronized (this) {
				if (enabledInterrupts()) {
					ThreadState.suspendAllCpuThreadsButThis();
					enabledInterrupts() = false;
					return Enabled;
				} else {
					return Disabled;
				}
			}
			*/
			return 0;
		}

		/// <summary>
		/// Resume/Enable all interrupts.
		/// </summary>
		/// <param name="set">The value returned from ::sceKernelCpuSuspendIntr().</param>
		[HlePspFunction(NID = 0x5F10D406, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntr(bool set)
		{
			/*
			synchronized (this) {
				if (set == Enabled) {
					enabledInterrupts() = true;
					ThreadState.resumeAllCpuThreadsButThis();
				} else {
					ThreadState.suspendAllCpuThreadsButThis();
					enabledInterrupts() = false;
				}
			}
			*/
		}
	}
}
