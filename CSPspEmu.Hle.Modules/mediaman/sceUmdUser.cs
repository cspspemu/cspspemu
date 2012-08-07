using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.threadman;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.mediaman
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceUmdUser : HleModuleHost
	{
		[Inject]
		HleCallbackManager CallbackManager;

		[Inject]
		ThreadManForUser ThreadManForUser;

		Dictionary<int, HleCallback> RegisteredCallbacks = new Dictionary<int, HleCallback>();

		/// <summary>
		/// Get the error code associated with a failed event
		/// </summary>
		/// <returns>Less than 0 on error, the error code on success</returns>
		[HlePspFunction(NID = 0x20628E6F, FirmwareVersion = 150)]
		public int sceUmdGetErrorStat()
		{
			return 0;
		}

		/// <summary>
		/// Register a callback for the UMD drive
		/// This function schedules a call to the callback with the current UMD status.
		/// So you can expect this to be executed when processing callbacks at least once. 
		/// </summary>
		/// <example>
		/// int umd_callback(int cbid, pspUmdState state, void *argument)
		/// {
		///		//do something
		/// }
		/// int cbid = sceKernelCreateCallback("UMD Callback", umd_callback, argument);
		/// sceUmdRegisterUMDCallBack(cbid);
		/// </example>
		/// <remarks>Callback is of type UmdCallback</remarks>
		/// <param name="CallbackId">A callback ID created from sceKernelCreateCallback</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xAEE7404D, FirmwareVersion = 150)]
		public int sceUmdRegisterUMDCallBack(int CallbackId)
		{
			var Callback = CallbackManager.Callbacks.Get(CallbackId);
			RegisteredCallbacks[CallbackId] = HleCallback.Create(
				"sceUmdRegisterUMDCallBack", Callback.Function,
				1, (int)(PspUmdState.PSP_UMD_READABLE | PspUmdState.PSP_UMD_READY | PspUmdState.PSP_UMD_PRESENT), Callback.Arguments[0]
			);

			CallbackManager.ScheduleCallback(RegisteredCallbacks[CallbackId]);

			return 0;
			//throw(new NotImplementedException());
			/*
			//logWarning("Not implemented: sceUmdRegisterUMDCallBack");
			unimplemented_notice();
		
			umdPspCallback = uniqueIdFactory.get!PspCallback(cbid);
		
			hleEmulatorState.callbacksHandler.register(CallbacksHandler.Type.Umd, umdPspCallback);
			umdPspCallbackId = cbid;
			triggerUmdStatusChange();
		
			return 0;
			*/
		}
	
		/*
		void triggerUmdStatusChange()
		{
			hleEmulatorState.callbacksHandler.trigger(CallbacksHandler.Type.Umd, [umdPspCallbackId, cast(uint)sceUmdGetDriveStat(), 0], 2);
		}
		*/

		/// <summary>
		/// Un-register a callback for the UMD drive
		/// </summary>
		/// <param name="CallbackId">A callback ID created from sceKernelCreateCallback</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xBD2BDE07, FirmwareVersion = 150)]
		public int sceUmdUnRegisterUMDCallBack(int CallbackId)
		{
			if (!RegisteredCallbacks.ContainsKey(CallbackId))
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT));
			}
			RegisteredCallbacks.Remove(CallbackId);
			return 0;
		}

		/// <summary>
		/// Wait for the UMD drive to reach a certain state
		/// </summary>
		/// <param name="PspUmdState">One or more of ::pspUmdState</param>
		/// <param name="Timeout">Timeout value in microseconds</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x56202973, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceUmdWaitDriveStatWithTimer(PspUmdState PspUmdState, uint Timeout)
		{
			return 0;
			//throw(new NotImplementedException());
			/*
			logWarning("Not implemented: sceUmdWaitDriveStatWithTimer");
			return 0;
			*/
		}

		public enum UmdCheckMediumEnum : uint
		{
			NoDisc = 0,
			Inserted = 1,
		}

		/// <summary>
		/// Check whether there is a disc in the UMD drive
		/// </summary>
		/// <returns>0 if no disc present, 1 if the disc is present.</returns>
		[HlePspFunction(NID = 0x46EBB729, FirmwareVersion = 150)]
		public UmdCheckMediumEnum sceUmdCheckMedium()
		{
			return UmdCheckMediumEnum.Inserted;
		}

		/// <summary>
		/// Activates the UMD drive
		/// </summary>
		/// <example>
		///		// Wait for disc and mount to filesystem
		///		int i;
		///		i = sceUmdCheckMedium();
		///		if(i == 0)
		///		{
		///			sceUmdWaitDriveStat(PSP_UMD_PRESENT);
		///		}
		///		sceUmdActivate(1, "disc0:"); // Mount UMD to disc0: file system
		///		sceUmdWaitDriveStat(PSP_UMD_READY);
		///		// Now you can access the UMD using standard sceIo functions
		/// </example>
		/// <param name="Mode">Mode.</param>
		/// <param name="Drive">A prefix string for the fs device to mount the UMD on (e.g. "disc0:")</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xC6183D47, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceUmdActivate(int Mode, string Drive)
		{
			return 0;
		}

		/// <summary>
		/// Deativates the UMD drive
		/// </summary>
		/// <param name="Mode">Mode.</param>
		/// <param name="Drive">A prefix string for the fs device to mount the UMD on (e.g. "disc0:")</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xE83742BA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUmdDeactivate(int Mode, string Drive)
		{
			return 0;
			//throw (new NotImplementedException());
			/*
			unimplemented_notice();
			*/
		}

		/// <summary>
		/// Get (poll) the current state of the UMD drive
		/// </summary>
		/// <returns>Less than 0 on error, one or more of ::PspUmdState on success</returns>
		[HlePspFunction(NID = 0x6B4A146C, FirmwareVersion = 150)]
		public PspUmdState sceUmdGetDriveStat()
		{
			return PspUmdState.PSP_UMD_PRESENT | PspUmdState.PSP_UMD_READY | PspUmdState.PSP_UMD_READABLE;
			//throw (new NotImplementedException());
			/*
			logTrace("Partially implemented: sceUmdGetDriveStat");
			return PspUmdState.PSP_UMD_PRESENT | PspUmdState.PSP_UMD_READY | PspUmdState.PSP_UMD_READABLE;
			*/
		}

		/// <summary>
		/// Wait for the UMD drive to reach a certain state
		/// </summary>
		/// <param name="PspUmdState">One or more of ::pspUmdState</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x8EF08FCE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUmdWaitDriveStat(PspUmdState PspUmdState)
		{
			return 0;
			//throw (new NotImplementedException());
			/*
			logWarning("Not implemented: sceUmdWaitDriveStat(%d)", stat);
			return 0;
			*/
		}

		/// <summary>
		/// Wait for the UMD drive to reach a certain state (plus callback)
		/// </summary>
		/// <param name="stat">One or more of ::pspUmdState</param>
		/// <param name="timeout">Timeout value in microseconds</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x4A9E5E29, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUmdWaitDriveStatCB(CpuThreadState CpuThreadState, PspUmdState stat, uint timeout)
		{
			ThreadManForUser.sceKernelCheckCallback(CpuThreadState);
			return 0;
			//throw(new NotImplementedException());
			/*
			logWarning("Not implemented: sceUmdWaitDriveStatCB(%s:%d, %d)", to!string(stat), stat, timeout);
		
			hleEmulatorState.moduleManager.get!ThreadManForUser.sceKernelCheckCallback();
		
			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x6AF9B50A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUmdCancelWaitDriveStat()
		{
			return 0;
		}

		/// <summary>
		/// Prohibit UMD disc being replaced.
		/// </summary>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x87533940, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUmdReplaceProhibit()
		{
			return 0;
		}

		[HlePspFunction(NID = 0xCBE9F02A, FirmwareVersion = 200)]
		[HlePspNotImplemented]
		public int sceUmdReplacePermit()
		{
			return 0;
		}

		/// <summary>
		/// Enumeration for UMD types
		/// </summary>
		[Flags]
		public enum pspUmdTypes : uint
		{
			/// <summary>
			/// PSP_UMD_TYPE_GAME
			/// </summary>
			Game = 0x10,
			
			/// <summary>
			/// PSP_UMD_TYPE_VIDEO
			/// </summary>
			Video = 0x20,
			
			/// <summary>
			/// PSP_UMD_TYPE_AUDIO
			/// </summary>
			Audio = 0x40
		}

		/// <summary>
		/// UMD Info struct
		/// </summary>
		public struct pspUmdInfo
		{
			/// <summary>
			/// Set to sizeof(pspUmdInfo)
			/// </summary>
			public uint Size;
			
			/// <summary>
			/// One or more of ::pspUmdTypes
			/// </summary>
			public pspUmdTypes Type;
		}

		/// <summary>
		/// Get the disc info
		/// </summary>
		/// <param name="pspUmdInfo">A pointer to a ::pspUmdInfo struct</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x340B7686, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUmdGetDiscInfo(pspUmdInfo* pspUmdInfo)
		{
			//pspUmdInfo->Size = sizeof(pspUmdInfo);
			pspUmdInfo->Type = pspUmdTypes.Game;
			return 0;
		}
	}

	/// <summary>
	/// Enumeration for UMD drive state
	/// </summary>
	[Flags]
	public enum PspUmdState : uint
	{
		PSP_UMD_INIT = 0x00,
		PSP_UMD_NOT_PRESENT = 0x01,
		PSP_UMD_PRESENT = 0x02,
		PSP_UMD_CHANGED = 0x04,
		PSP_UMD_NOT_READY = 0x08,
		PSP_UMD_READY = 0x10,
		PSP_UMD_READABLE = 0x20,
	}

}
