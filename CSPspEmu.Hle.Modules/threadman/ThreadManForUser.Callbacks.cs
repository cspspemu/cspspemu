using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

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
		public int sceKernelCreateCallback(string Name, SceKernelCallbackFunction Function, uint Argument)
		{
			return HleState.CallbackManager.Callbacks.Create(
				HleCallback.Create(Name, (uint)Function, Argument)
			);
		}

		/// <summary>
		/// Notify a callback
		/// </summary>
		/// <param name="cb">The UID of the specified callback</param>
		/// <param name="arg2">Passed as arg2 into the callback function</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xC11BA8C4, FirmwareVersion = 150)]
		public int sceKernelNotifyCallback(int cb, int arg2)
		{
			throw(new NotImplementedException());
			/*
			PspCallback pspCallback = uniqueIdFactory.get!PspCallback(cb);
		
			hleEmulatorState.executeGuestCode(currentThreadState, pspCallback.func, [pspCallback.arg, arg2]);
		
			return 0;
			*/
		}

		/// <summary>
		/// Delete a callback
		/// </summary>
		/// <param name="cb">The UID of the specified callback</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xEDBA5844, FirmwareVersion = 150)]
		public int sceKernelDeleteCallback(int cb)
		{
			throw(new NotImplementedException());
			/*
			uniqueIdFactory.remove!PspCallback(cb);
			return 0;
			*/
		}

		/// <summary>
		/// Run all peding callbacks and return if executed any.
		/// </summary>
		/// <remarks>
		/// Callbacks cannot be executed inside a interrupt
		/// Here callbacks can be executed.
		/// </remarks>
		/// <returns>
		///		0 - if the calling thread has no reported callbacks
		///		1 - if the calling thread has reported callbacks which were executed successfully.
		/// </returns>
		[HlePspFunction(NID = 0x349D6D6C, FirmwareVersion = 150)]
		public int sceKernelCheckCallback(CpuThreadState CpuThreadState)
		{
			return (HleState.CallbackManager.ExecuteQueued(CpuThreadState, false) > 0) ? 1 : 0;
		}

		/// <summary>
		/// Get the callback count
		/// </summary>
		/// <param name="cb">The UID of the specified callback</param>
		/// <returns>The callback count, less than 0 on error</returns>
		[HlePspFunction(NID = 0x2A3D44FF, FirmwareVersion = 150)]
		public int sceKernelGetCallbackCount(int cb)
		{
			throw(new NotImplementedException());
			/*
			PspCallback pspCallback = uniqueIdFactory.get!PspCallback(cb);
			return pspCallback.notifyCount;
			*/
		}

	}
}
