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
		public void scePowerUnregitserCallback()
		{
			throw (new NotImplementedException());
		}

		/**
		 * Register Power Callback Function
		 *
		 * @param slot - slot of the callback in the list, 0 to 15, pass -1 to get an auto assignment.
		 * @param cbid - callback id from calling sceKernelCreateCallback
		 *
		 * @return 0 on success, the slot number if -1 is passed, < 0 on error.
		 */
		[HlePspFunction(NID = 0x04B7766E, FirmwareVersion = 150)]
		public int scePowerRegisterCallback(int slot, int cbid)
		{
			throw (new NotImplementedException());
		}

		/**
		 * Unregister Power Callback Function
		 *
		 * @param slot - slot of the callback
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0xDFA8BAF8, FirmwareVersion = 150)]
		public int scePowerUnregisterCallback(int slot)
		{
			throw (new NotImplementedException());
		}
	}
}
