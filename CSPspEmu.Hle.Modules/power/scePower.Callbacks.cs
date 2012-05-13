using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.power
{
	unsafe public partial class scePower
	{
		//PspCallback[16] callbacks;

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xDB9D28DD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void scePowerUnregitserCallback()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Register Power Callback Function
		/// </summary>
		/// <param name="slot">slot of the callback in the list, 0 to 15, pass -1 to get an auto assignment.</param>
		/// <param name="cbid">callback id from calling sceKernelCreateCallback</param>
		/// <returns> 0 on success, the slot number if -1 is passed, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x04B7766E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePowerRegisterCallback(int slot, int cbid)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Unregister Power Callback Function
		/// </summary>
		/// <param name="slot">slot of the callback</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xDFA8BAF8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePowerUnregisterCallback(int slot)
		{
			//throw (new NotImplementedException());
			return 0;
		}
	}
}
