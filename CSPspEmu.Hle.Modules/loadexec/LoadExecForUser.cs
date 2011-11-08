using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.loadexec
{
	public class LoadExecForUser : HleModuleHost
	{
		/// <summary>
		/// Exit game and go back to the PSP browser.
		/// </summary>
		[HlePspFunction(NID = 0xBD2F1094, FirmwareVersion = 150)]
		public void sceKernelExitGame()
		{
			//throw (new HaltException("sceKernelExitGame"));
			throw(new NotImplementedException());
		}
	}
}
