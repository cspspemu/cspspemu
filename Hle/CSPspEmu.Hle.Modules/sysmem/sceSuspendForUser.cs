using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.sysmem
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00000011)]
	public unsafe class sceSuspendForUser : HleModuleHost
	{
		[Inject]
		HleMemoryManager MemoryManager;

		/// <summary>
		/// Unknown
		/// </summary>
		[HlePspFunction(NID = 0xEADB1BD7, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceKernelPowerLock(int LockType)
		{
			if (LockType != 0) throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_MODE));
			//unimplemented_notice();
			return 0;
		}

		/// <summary>
		/// Unknown
		/// </summary>
		[HlePspFunction(NID = 0x3AEE7261, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceKernelPowerUnlock(int LockType)
		{
			if (LockType != 0) throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_MODE));
			//unimplemented_notice();
			return 0;
		}

		/// <summary>
		/// Will prevent the backlight to turn off.
		/// </summary>
		/// <param name="value"></param>
		[HlePspFunction(NID = 0x090CCB3F, FirmwareVersion = 150)]
		public int sceKernelPowerTick(uint value)
		{
			//logWarning("Not Implemented sceKernelPowerTick");
			return 0;
		}

		bool VolatileMemLocked = false;

		/// <summary>
		/// Allocate the extra 4 megs of RAM
		/// </summary>
		/// <param name="Type">No idea as it is never used, set to anything</param>
		/// <param name="OutAddress">Pointer to a pointer to hold the address of the memory</param>
		/// <param name="OutSize">Pointer to an int which will hold the size of the memory</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x3E0271D3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelVolatileMemLock(int Type, out uint OutAddress, out int OutSize)
		{
			if (Type != 0)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT));
			}

			if (VolatileMemLocked)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_POWER_VMEM_IN_USE));
			}

			var Partition = MemoryManager.GetPartition(MemoryPartitions.VolatilePartition);
			OutAddress = Partition.Low;
			OutSize = Partition.Size;    // 4 MB
			VolatileMemLocked = true;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Type"></param>
		/// <param name="OutAddress"></param>
		/// <param name="OutSize"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA14F40B2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelVolatileMemTryLock(int Type, out uint OutAddress, out int OutSize)
		{
			return sceKernelVolatileMemLock(Type, out OutAddress, out OutSize);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Type"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA569E425, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelVolatileMemUnlock(int Type)
		{
			if (Type != 0)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT));
			}

			if (!VolatileMemLocked)
			{
				throw(new SceKernelException((SceKernelErrors)(-1)));
			}

			VolatileMemLocked = false;

			return 0;
		}
	}
}
