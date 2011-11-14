using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		/// <summary>
		/// Send a devctl command to a device.
		/// </summary>
		/// <example>
		///		Example: Sending a simple command to a device (not a real devctl)
		///		sceIoDevctl("ms0:", 0x200000, indata, 4, NULL, NULL); 
		/// </example>
		/// <param name="DeviceName">String for the device to send the devctl to (e.g. "ms0:")</param>
		/// <param name="Command">The command to send to the device</param>
		/// <param name="InputPointer">A data block to send to the device, if NULL sends no data</param>
		/// <param name="InputLength">Length of indata, if 0 sends no data</param>
		/// <param name="OutputPointer">A data block to receive the result of a command, if NULL receives no data</param>
		/// <param name="OutputLength">Length of outdata, if 0 receives no data</param>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0x54F5FB11, FirmwareVersion = 150)]
		public int sceIoDevctl(string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			var Info = HleState.HleIoManager.ParseDeviceName(DeviceName);
			return Info.HleIoDrvFileArg.HleIoDriver.IoDevctl(Info.HleIoDrvFileArg, DeviceName, Command, InputPointer, InputLength, OutputPointer, OutputLength);
		}
	}
}
