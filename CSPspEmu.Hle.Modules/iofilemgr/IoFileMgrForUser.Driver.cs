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
		public int sceIoAddDrv(PspIoDrv* PspIoDrv)
		{
			Console.WriteLine("Not implemented sceIoAddDrv(...)");
			return 0;
		}

		/// <summary>
		/// Deletes a IO driver from the system.
		/// 
		/// @note This is only exported in the kernel version of IoFileMgr
		/// </summary>
		/// <param name="DriverName">Name of the driver to delete.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xC7F35804, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoDelDrv(string DriverName)
		{
			Console.WriteLine("Not implemented sceIoDelDrv('{0}')", DriverName);
			return 0;
		}
	}
}
