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

		/**
		 * Allocate the extra 4megs of RAM
		 *
		 * @param unk  - No idea as it is never used, set to anything
		 * @param ptr  - Pointer to a pointer to hold the address of the memory
		 * @param size - Pointer to an int which will hold the size of the memory
		 *
		 * @return 0 on success
		 */
		[HlePspFunction(NID = 0x3E0271D3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelVolatileMemLock(int unk, uint* ptr, int* size)
		{
			*ptr = 0x08400000;
			*size = 0x400000;    // 4 MB
			return 0;
		}
	}
}
