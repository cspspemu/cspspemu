using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public enum LwMutexAttributes : uint
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workAreaAddr"></param>
		/// <param name="Name"></param>
		/// <param name="Attributes"></param>
		/// <param name="count"></param>
		/// <param name="OptionAddress"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x19CFF145, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelCreateLwMutex(uint workAreaAddr, string Name, LwMutexAttributes Attributes, int count, int OptionAddress)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workAreaAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x60107536, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelDeleteLwMutex(uint workAreaAddr)
		{
			//throw (new NotImplementedException());
			return 0;
		}
	}
}
