using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.utility
{
	unsafe public partial class sceUtility
	{
		/// <summary>
		/// Get the status of a running Network Configuration Dialog
		/// </summary>
		/// <returns>one of pspUtilityDialogState on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x6332AA39, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public pspUtilityDialogState sceUtilityNetconfGetStatus()
		{
			return pspUtilityDialogState.Finished;
		}
	}
}
