using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.utility
{
	unsafe public partial class sceUtility
	{
		/// <summary>
		/// Create an on-screen keyboard
		/// </summary>
		/// <param name="Params">OSK parameters.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xF6269B82, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityOskInitStart(SceUtilityOskParams* Params)
		{
			return 0;
		}

		/// <summary>
		/// Remove a currently active keyboard. After calling this function you must
		/// poll sceUtilityOskGetStatus() until it returns PSP_UTILITY_DIALOG_NONE.
		/// </summary>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x3DFAEBA9, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityOskShutdownStart()
		{
			return 0;
		}

		/// <summary>
		/// Refresh the GUI for a keyboard currently active
		/// </summary>
		/// <param name="n">Unknown, pass 1.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x4B85C861, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilityOskUpdate(int n)
		{
			return 0;
		}

		/// <summary>
		/// Get the status of a on-screen keyboard currently active.
		/// </summary>
		/// <returns>the current status of the keyboard. See ::pspUtilityDialogState for details.</returns>
		[HlePspFunction(NID = 0xF3F76017, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public pspUtilityDialogState sceUtilityOskGetStatus()
		{
			return pspUtilityDialogState.Finished;
		}
	}
}
