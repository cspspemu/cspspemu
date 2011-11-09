using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public class IoFileMgrForUser : HleModuleHost
	{
		public enum EmulatorDevclEnum : int
		{
			GetHasDisplay = 1,
			SendOutput = 2,
			IsEmulator = 3,
		}

		/// <summary>
		/// Send a devctl command to a device.
		/// </summary>
		/// <example>
		///		Example: Sending a simple command to a device (not a real devctl)
		///		sceIoDevctl("ms0:", 0x200000, indata, 4, NULL, NULL); 
		/// </example>
		/// <param name="Device">String for the device to send the devctl to (e.g. "ms0:")</param>
		/// <param name="Command">The command to send to the device</param>
		/// <param name="InputPtr">A data block to send to the device, if NULL sends no data</param>
		/// <param name="InputLength">Length of indata, if 0 sends no data</param>
		/// <param name="OutputPtr">A data block to receive the result of a command, if NULL receives no data</param>
		/// <param name="OutputLength">Length of outdata, if 0 receives no data</param>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0x54F5FB11, FirmwareVersion = 150)]
		public int sceIoDevctl(string Device, int Command, byte* InputPtr, int InputLength, byte* OutputPtr, int OutputLength)
		{
			Console.WriteLine("sceIoDevctl('{0}', {1}, {2}, {3}, {4}, {5})", Device, Command, (uint)InputPtr, InputLength, (uint)OutputPtr, OutputLength);

			if (Device == "emulator:")
			{
				Console.WriteLine("     {0}", (EmulatorDevclEnum)Command);
				switch ((EmulatorDevclEnum)Command)
				{
					case EmulatorDevclEnum.GetHasDisplay:
						*((int*)OutputPtr) = HleState.Processor.HasDisplay ? 1 : 0;
						break;
					case EmulatorDevclEnum.SendOutput:
						Console.WriteLine("   OUTPUT:  {0}", new String((sbyte*)InputPtr, 0, InputLength, Encoding.ASCII));
						break;
					case EmulatorDevclEnum.IsEmulator:
						return 0;
				}
				return 0;
			}

			throw(new NotImplementedException());
			//return 0;
			/*
			try {
				return devices[dev].sceIoDevctl(cmd, (cast(ubyte*)indata)[0..inlen], (cast(ubyte*)outdata)[0..outlen]);
			} catch (Exception e) {
				writefln("sceIoDevctl: %s", e);
				return -1;
			}
			*/
		}
	}
}
