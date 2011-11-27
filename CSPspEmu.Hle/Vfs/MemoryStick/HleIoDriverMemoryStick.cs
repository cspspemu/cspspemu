using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs.MemoryStick
{
	unsafe public class HleIoDriverMemoryStick : ProxyHleIoDriver
	{
		public HleIoDriverMemoryStick(IHleIoDriver HleIoDriver) : base(HleIoDriver)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="DeviceName"></param>
		/// <param name="Command"></param>
		/// <param name="InputPointer"></param>
		/// <param name="InputLength"></param>
		/// <param name="OutputPointer"></param>
		/// <param name="OutputLength"></param>
		/// <returns></returns>
		public override int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			Console.Error.WriteLine("MemoryStick.IoDevctl Not Implemented!");
			return 0;
		}
	}
}
