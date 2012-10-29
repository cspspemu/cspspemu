using System;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	public unsafe partial class IoFileMgrForUser
	{
		/// <summary>
		/// Perform an ioctl on a device.
		/// </summary>
		/// <param name="FileHandle">Opened file descriptor to ioctl to</param>
		/// <param name="Command">The command to send to the device</param>
		/// <param name="InputPointer">A data block to send to the device, if NULL sends no data</param>
		/// <param name="InputLength">Length of indata, if 0 sends no data</param>
		/// <param name="OutputPointer">A data block to receive the result of a command, if NULL receives no data</param>
		/// <param name="OutputLength">Length of outdata, if 0 receives no data</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x63632449, FirmwareVersion = 150)]
		public int sceIoIoctl(SceUID FileHandle, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
			try
			{
				return HleIoDrvFileArg.HleIoDriver.IoIoctl(HleIoDrvFileArg, Command, InputPointer, InputLength, OutputPointer, OutputLength);
			}
			finally
			{
				_DelayIo(IoDelayType.Ioctl);
			}
		}

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
			try
			{
				var Info = HleIoManager.ParseDeviceName(DeviceName);
				return Info.HleIoDrvFileArg.HleIoDriver.IoDevctl(Info.HleIoDrvFileArg, DeviceName, Command, InputPointer, InputLength, OutputPointer, OutputLength);
			}
			catch (NotImplementedException NotImplementedException)
			{
				Console.Error.WriteLine(NotImplementedException);
				return -1;
			}
			finally
			{
				_DelayIo(IoDelayType.Devctl);
			}
		}
	}
}
