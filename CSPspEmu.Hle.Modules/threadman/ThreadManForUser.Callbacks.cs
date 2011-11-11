using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser : HleModuleHost
	{
		public enum SceKernelCallbackFunction : uint
		{
		}

		/// <summary>
		/// Create callback
		/// </summary>
		/// <example>
		/// int cbid;
		/// cbid = sceKernelCreateCallback("Exit Callback", exit_cb, NULL);
		/// </example>
		/// <param name="name">A textual name for the callback</param>
		/// <param name="func">A pointer to a function that will be called as the callback</param>
		/// <param name="arg">Argument for the callback ?</param>
		/// <returns>&gt;= 0 A callback id which can be used in subsequent functions, < 0 an error.</returns>
		[HlePspFunction(NID = 0xE81CAF8F, FirmwareVersion = 150)]
		public uint sceKernelCreateCallback(string Name, SceKernelCallbackFunction Function, uint Argument)
		{
			return HleState.CallbackManager.Callbacks.Create(new HleCallback()
			{
				Name = Name,
				Function = (uint)Function,
				Argument = Argument,
			});
		}
	}
}
