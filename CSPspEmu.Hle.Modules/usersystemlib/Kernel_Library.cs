using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Modules.threadman;

namespace CSPspEmu.Hle.Modules.usersystemlib
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe class Kernel_Library : HleModuleHost
	{
		[Inject]
		public HleInterruptManager HleInterruptManager;

		[Inject]
		public ThreadManForUser ThreadManForUser;

		public struct SceLwMutexWorkarea
		{
			public int count;
			public SceUID thread;
			public int attr;
			public int numWaitThreads;
			public SceUID uid;
			private fixed int pad[3];
		}

		public struct SceKernelLwMutexInfo
		{
			public SceSize size;
			public fixed byte _name[32];
			public uint attr;
			public SceUID uid;
			public void *workarea;
			public int initCount;
			public int currentCount;
			public SceUID lockThread;
			public int numWaitThreads;
		}

		private int _sceKernelLockLwMutexCB(SceLwMutexWorkarea* WorkAreaPointer, int Count, int* TimeOut, bool HandleCallbacks)
		{
			return 0;
		}

		private int _sceKernelTryLockLwMutex(SceLwMutexWorkarea* WorkAreaPointer, int Count)
		{
			return 0;
		}

		/// <summary>
		/// Suspend all interrupts.
		/// </summary>
		/// <returns>The current state of the interrupt controller, to be used with <see cref="sceKernelCpuResumeIntr()"/></returns>
		[HlePspFunction(NID = 0x092968F4, FirmwareVersion = 150)]
		public uint sceKernelCpuSuspendIntr()
		{
			return HleInterruptManager.sceKernelCpuSuspendIntr();
		}

		private void _sceKernelCpuResumeIntr(uint Flags)
		{
			HleInterruptManager.sceKernelCpuResumeIntr(Flags);
		}

		/// <summary>
		/// Resume/Enable all interrupts.
		/// </summary>
		/// <param name="Flags">The value returned from <see cref="sceKernelCpuSuspendIntr()"/>.</param>
		[HlePspFunction(NID = 0x5F10D406, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntr(uint Flags)
		{
			_sceKernelCpuResumeIntr(Flags);
		}

		/// <summary>
		/// Resume all interrupts (using sync instructions).
		/// </summary>
		/// <param name="Flags">The value returned from <see cref="sceKernelCpuSuspendIntr()"/></param>
		[HlePspFunction(NID = 0x3B84732D, FirmwareVersion = 150)]
		public void sceKernelCpuResumeIntrWithSync(uint Flags)
		{
			_sceKernelCpuResumeIntr(Flags);
		}

		/// <summary>
		/// Determine if interrupts are suspended or active, based on the given flags.
		/// </summary>
		/// <param name="Flags">The value returned from <see cref="sceKernelCpuSuspendIntr()"/>.</param>
		/// <returns>1 if flags indicate that interrupts were not suspended, 0 otherwise.</returns>
		[HlePspFunction(NID = 0x47A0B729, FirmwareVersion = 150)]
		public bool sceKernelIsCpuIntrSuspended(int Flags)
		{
			return (Flags != 0);
		}

		/// <summary>
		/// Unlocks a LigthWeight Mutex
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x15B6446B, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelUnlockLwMutex(SceLwMutexWorkarea* WorkAreaPointer, int Count)
		{
			return 0;
		}

		/// <summary>
		/// Locks a LightWeight Mutex
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <param name="TimeOut"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xBEA46419, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelLockLwMutex(SceLwMutexWorkarea* WorkAreaPointer, int Count, int* TimeOut)
		{
			return _sceKernelLockLwMutexCB(WorkAreaPointer, Count, TimeOut, HandleCallbacks: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x37431849, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelTryLockLwMutex_600(SceLwMutexWorkarea* WorkAreaPointer, int Count)
		{
			return _sceKernelTryLockLwMutex(WorkAreaPointer, Count);
		}


		/// <summary>
		/// Locks a LightWeight Mutex (with callback)
		/// </summary>
		/// <param name="WorkAreaPointer"></param>
		/// <param name="Count"></param>
		/// <param name="TimeOut"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1FC64E09, FirmwareVersion = 380)]
		[HlePspNotImplemented]
		public int sceKernelLockLwMutexCB(SceLwMutexWorkarea* WorkAreaPointer, int Count, int* TimeOut)
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
		/// Get the current thread ID
		/// </summary>
		/// <returns>The thread ID of the calling thread.</returns>
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
		public uint sceKernelMemset(uint PspPointer, int Data, int Size)
		{
			try
			{
				PointerUtils.Memset((byte*)PspMemory.PspAddressToPointerSafe(PspPointer, Size), (byte)Data, Size);
			}
			catch
			{
			}
			return PspPointer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Destination"></param>
		/// <param name="Source"></param>
		/// <param name="Size"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1839852A, FirmwareVersion = 150)]
		public uint sceKernelMemcpy(uint DestinationPointer, uint SourcePointer, int Size)
		{
			try
			{
				var Destination = (byte*)PspMemory.PspAddressToPointerSafe(DestinationPointer, Size);
				var Source = (byte*)PspMemory.PspAddressToPointerSafe(SourcePointer, Size);
				PointerUtils.Memcpy(Destination, Source, Size);
			}
			catch
			{
			}

			return DestinationPointer;
		}
	}
}
