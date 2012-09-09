using System.Text;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	public unsafe partial class IoFileMgrForUser
	{
		public struct PspIoDrv
		{
			/// <summary>
			/// The name of the device to add
			/// </summary>
			public PspPointer name;

			/// <summary>
			/// Device type, this 0x10 is for a filesystem driver
			/// </summary>
			public uint dev_type;

			/// <summary>
			/// Unknown, set to 0x800
			/// </summary>
			public uint unk2;
			
			/// <summary>
			/// This seems to be the same as name but capitalised :/
			/// </summary>
			public PspPointer name2;

			/// <summary>
			/// Pointer to a filled out functions table
			/// </summary>
			public PspIoDrvFuncs* funcs;
		}

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
		/// <param name="PspIoDrv">Pointer to a filled out driver structure</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x8E982A74, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoAddDrv(PspIoDrv* PspIoDrv)
		{
			return 0;

			var Name = PspMemory.ReadStringz(PspIoDrv->name, Encoding.UTF8);
			HleIoManager.SetDriver(Name + ":", new GuestHleIoDriver(PspEmulatorContext, PspIoDrv));
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
		public int sceIoDelDrv(string DriverName)
		{
			HleIoManager.RemoveDriver(DriverName + ":");
			return 0;
		}
	}
}
