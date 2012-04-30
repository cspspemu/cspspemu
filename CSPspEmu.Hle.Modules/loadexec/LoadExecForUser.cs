using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.loadexec
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public class LoadExecForUser : HleModuleHost
	{
		Logger Logger = Logger.GetLogger("LoadExecForUser");

		[Inject]
		HleCallbackManager CallbackManager;

		/// <summary>
		/// Exit game and go back to the PSP browser.
		/// </summary>
		/// <remarks>
		///		You need to be in a thread in order for this function to work
		/// </remarks>
		[HlePspFunction(NID = 0xBD2F1094, FirmwareVersion = 150)]
		[HlePspFunction(NID = 0x05572A5F, FirmwareVersion = 150)]
		public void sceKernelExitGame()
		{
			Logger.Error("sceKernelExitGame");
		}

		/// <summary>
		/// Register callback
		/// </summary>
		/// <remarks>
		///		By installing the exit callback the home button becomes active.
		///		However if sceKernelExitGame is not called in the callback it is likely that the psp will just crash.
		///	</remarks>
		///	<example>
		///		int exit_callback(void) { sceKernelExitGame(); }
		///		cbid = sceKernelCreateCallback("ExitCallback", exit_callback, NULL);
		///		sceKernelRegisterExitCallback(cbid);
		///	</example>
		/// <param name="CallbackId">Callback id</param>
		/// <returns>&lt; 0 on error</returns>
		[HlePspFunction(NID = 0x4AC57943, FirmwareVersion = 150)]
		public int sceKernelRegisterExitCallback(int CallbackId)
		{
			var Callback = CallbackManager.Callbacks.Get(CallbackId);
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
