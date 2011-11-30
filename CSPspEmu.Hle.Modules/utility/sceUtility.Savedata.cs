using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.utility
{
	unsafe public partial class sceUtility
	{
		/// <summary>
		/// Saves or Load savedata to/from the passed structure
		/// After having called this continue calling sceUtilitySavedataGetStatus to
		/// check if the operation is completed
		/// </summary>
		/// <param name="Params">Savedata parameters</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x50C4CD57, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilitySavedataInitStart(SceUtilitySavedataParam* Params)
		{
			//throw(new NotImplementedException());
			CurrentDialogStep = DialogStepEnum.SUCCESS;
			return 0;
		}

		/// <summary>
		/// Shutdown the savedata utility. after calling this continue calling
		/// ::sceUtilitySavedataGetStatus to check when it has shutdown
		/// </summary>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x9790B33C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilitySavedataShutdownStart()
		{
			//throw(new NotImplementedException());
			CurrentDialogStep = DialogStepEnum.SHUTDOWN;
			return 0;
		}

		/// <summary>
		/// Refresh status of the savedata function
		/// </summary>
		/// <param name="unknown">unknown, pass 1</param>
		[HlePspFunction(NID = 0xD4B95FFB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceUtilitySavedataUpdate(int unknown)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Check the current status of the saving/loading/shutdown process
		/// Continue calling this to check current status of the process
		/// before calling this call also sceUtilitySavedataUpdate
		/// </summary>
		/// <returns>
		///		2 if the process is still being processed.
		///		3 on save/load success, then you can call sceUtilitySavedataShutdownStart.
		///		4 on complete shutdown.
		/// </returns>
		[HlePspFunction(NID = 0x8874DBE0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public DialogStepEnum sceUtilitySavedataGetStatus()
		{
			try
			{
				return CurrentDialogStep;
			}
			finally
			{
				if (CurrentDialogStep == DialogStepEnum.SHUTDOWN) {
					CurrentDialogStep = DialogStepEnum.NONE;
				}
			}
		}
	}
}
