using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.sysmem
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00000011)]
	unsafe public class sceSuspendForUser : HleModuleHost
	{
		/// <summary>
		/// Unknown
		/// </summary>
		[HlePspFunction(NID = 0xEADB1BD7, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public void sceKernelPowerLock()
		{
			//unimplemented_notice();
		}

		/// <summary>
		/// Unknown
		/// </summary>
		[HlePspFunction(NID = 0x3AEE7261, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public void sceKernelPowerUnlock()
		{
			//unimplemented_notice();
		}

		/// <summary>
		/// Will prevent the backlight to turn off.
		/// </summary>
		/// <param name="value"></param>
		[HlePspFunction(NID = 0x090CCB3F, FirmwareVersion = 150)]
		public void sceKernelPowerTick(uint value)
		{
			//logWarning("Not Implemented sceKernelPowerTick");
		}

		bool VolatileMemLocked = false;

		/// <summary>
		/// Allocate the extra 4megs of RAM
		/// </summary>
		/// <param name="Type">No idea as it is never used, set to anything</param>
		/// <param name="OutAddress">Pointer to a pointer to hold the address of the memory</param>
		/// <param name="OutSize">Pointer to an int which will hold the size of the memory</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x3E0271D3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelVolatileMemLock(int Type, uint* OutAddress, int* OutSize)
		{
			if (Type != 0)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT));
			}

			if (VolatileMemLocked)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_POWER_VMEM_IN_USE));
			}

			*OutAddress = 0x08400000;
			*OutSize = 0x400000;    // 4 MB
			VolatileMemLocked = true;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processor"></param>
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
