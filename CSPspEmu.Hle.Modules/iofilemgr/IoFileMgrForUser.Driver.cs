using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		/// <summary>
		/// Adds a new IO driver to the system.
		/// 
		/// @note This is only exported in the kernel version of IoFileMgr
		/// </summary>
		/// <example>
		///		PspIoDrvFuncs host_funcs = { ... };
		///		PspIoDrv host_driver = { "host", 0x10, 0x800, "HOST", &host_funcs };
		///		sceIoDelDrv("host");
		///		sceIoAddDrv(&host_driver);
		/// </example>
		/// <param name="drv">Pointer to a filled out driver structure</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x8E982A74, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoAddDrv(PspIoDrv* drv)
		{
			/*
			string name  = to!string(cast(char *)currentCpuThread().memory.getPointer(cast(uint)drv.name));
			string name2 = to!string(cast(char *)currentCpuThread().memory.getPointer(cast(uint)drv.name2));
			PspIoDrvFuncs* funcs = cast(PspIoDrvFuncs*)currentCpuThread().memory.getPointer(cast(uint)drv.funcs);
			rootFileSystem().addDriver(name, new IoDevice(hleEmulatorState, new PspVirtualFileSystem(hleEmulatorState, name, drv, funcs)));
			logWarning("sceIoAddDrv('%s', '%s', ...)", name, name2);
			return 0;
			*/
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Deletes a IO driver from the system.
		/// 
		/// @note This is only exported in the kernel version of IoFileMgr
		/// </summary>
		/// <param name="drv_name">Name of the driver to delete.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xC7F35804, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoDelDrv(string drv_name)
		{
			/*
			logWarning("sceIoDelDrv('%s')", drv_name);
			rootFileSystem().delDriver(drv_name);
			return 0;
			*/
			//throw (new NotImplementedException());
			return 0;
		}
	}
}
