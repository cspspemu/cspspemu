using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.threadman;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.usersystemlib
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public class Kernel_Library : HleModuleHost
	{
		[Inject]
		public HleInterruptManager HleInterruptManager;

		[Inject]
		public ThreadManForUser ThreadManForUser;

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
			return HleInterruptManager.sceKernelCpuSuspendIntr();
		}

		/// <summary>
		/// Resume/Enable all interrupts.
		/// </summary>
		/// <param name="Flags">The value returned from ::sceKernelCpuSuspendIntr().</param>
		[HlePspFunction(NID = 0x5F10D406, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntr(uint Flags)
		{
			HleInterruptManager.sceKernelCpuResumeIntr(Flags);
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
		/// <param name="Flags">The value returned from ::sceKernelCpuSuspendIntr().</param>
		/// <returns>1 if flags indicate that interrupts were not suspended, 0 otherwise.</returns>
		[HlePspFunction(NID = 0x47A0B729, FirmwareVersion = 150)]
		public bool sceKernelIsCpuIntrSuspended(int Flags)
		{
			return (Flags != 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x15B6446B, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelUnlockLwMutex(void* WorkAreaPointer, int Count)
		{
			return 0;
		}

		private int _sceKernelLockLwMutexCB(void* WorkAreaPointer, int Count, int* TimeOut, bool HandleCallbacks)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <param name="TimeOut"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xBEA46419, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelLockLwMutex(void* WorkAreaPointer, int Count, int* TimeOut)
		{
			return _sceKernelLockLwMutexCB(WorkAreaPointer, Count, TimeOut, HandleCallbacks: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <param name="TimeOut"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1FC64E09, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelLockLwMutexCB(void* WorkAreaPointer, int Count, int* TimeOut)
		{
			return _sceKernelLockLwMutexCB(WorkAreaPointer, Count, TimeOut, HandleCallbacks: true);
		}

		/// <summary>
		/// Determine if interrupts are enabled or disabled.
		/// </summary>
		/// <returns>1 if interrupts are currently enabled.</returns>
		[HlePspFunction(NID = 0xB55249D2, FirmwareVersion = 150)]
		public bool sceKernelIsCpuIntrEnable()
		{
			return HleInterruptManager.Enabled;
		}

		/// <summary>
		/// Get the current thread Id
		/// </summary>
		/// <returns>The thread id of the calling thread.</returns>
		[HlePspFunction(NID = 0x293B45B8, FirmwareVersion = 150)]
		public int sceKernelGetThreadId()
		{
			return ThreadManForUser.sceKernelGetThreadId();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Data"></param>
		/// <param name="Size"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA089ECA4, FirmwareVersion = 150)]
		public int sceKernelMemset(byte* Pointer, int Data, int Size)
		{
			PointerUtils.Memset(Pointer, (byte)Data, Size);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Destination"></param>
		/// <param name="Source"></param>
		/// <param name="Size"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1839852A, FirmwareVersion = 150)]
		public uint sceKernelMemcpy(byte* Destination, byte* Source, int Size)
		{
			PointerUtils.Memcpy(Destination, Source, Size);
			return PspMemory.PointerToPspAddressSafe(Destination);
		}
	}
}
