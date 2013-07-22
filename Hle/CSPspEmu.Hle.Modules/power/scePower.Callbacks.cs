using CSPspEmu.Hle.Managers;
using System;
using System.Linq;

namespace CSPspEmu.Hle.Modules.power
{
	public unsafe partial class scePower
	{
		[Inject]
		HleCallbackManager CallbackManager;

		private const int NumberOfCBPowerSlots = 16;
		private const int NumberOfCBPowerSlotsPrivate = 32;

		[Flags]
		public enum PowerCallbackFlags : uint
		{
			/// <summary>
			/// indicates the power switch it pushed, putting the unit into suspend mode
			/// </summary>
			PSP_POWER_CB_POWER_SWITCH	= 0x80000000,
			
			/// <summary>
			/// indicates the hold switch is on
			/// </summary>
			PSP_POWER_CB_HOLD_SWITCH	= 0x40000000,
			
			/// <summary>
			/// what is standby mode?
			/// </summary>
			PSP_POWER_CB_STANDBY		= 0x00080000,
			
			/// <summary>
			/// indicates the resume process has been completed (only seems to be triggered when another event happens)
			/// </summary>
			PSP_POWER_CB_RESUME_COMPLETE	= 0x00040000,
			
			/// <summary>
			/// indicates the unit is resuming from suspend mode
			/// </summary>
			PSP_POWER_CB_RESUMING		= 0x00020000,
			
			/// <summary>
			/// indicates the unit is suspending, seems to occur due to inactivity
			/// </summary>
			PSP_POWER_CB_SUSPENDING		= 0x00010000,
			
			/// <summary>
			/// indicates the unit is plugged into an AC outlet
			/// </summary>
			PSP_POWER_CB_AC_POWER		= 0x00001000,
			
			/// <summary>
			/// indicates the battery charge level is low
			/// </summary>
			PSP_POWER_CB_BATTERY_LOW	= 0x00000100,
			
			/// <summary>
			/// indicates there is a battery present in the unit
			/// </summary>
			PSP_POWER_CB_BATTERY_EXIST	= 0x00000080,
			
			/// <summary>
			/// unknown
			/// </summary>
			PSP_POWER_CB_BATTPOWER		= 0x0000007F,

			/// <summary>
			/// 
			/// </summary>
			PSP_POWER_CB_BATTERY_FULL = 0x00000064,
		}

		public class Slot
		{
			public readonly int Index;
			public int CallbackId;

			public Slot(int Index)
			{
				this.Index = Index;
			}
		}

		private readonly Slot[] Callbacks = Enumerable.Range(0, NumberOfCBPowerSlots).Select(Index => new Slot(Index)).ToArray();

		private void CheckSlotIndex(int SlotIndex, bool AllowMinusOne)
		{
			if (SlotIndex < (AllowMinusOne ? -1 : 0) || SlotIndex >= NumberOfCBPowerSlotsPrivate)
			{
				throw (new SceKernelException(SceKernelErrors.PSP_POWER_ERROR_INVALID_SLOT));
			}
			if (SlotIndex >= NumberOfCBPowerSlots)
			{
				throw (new SceKernelException(SceKernelErrors.PSP_POWER_ERROR_PRIVATE_SLOT));
			}
		}

		private PowerCallbackFlags GetPowerCallbackFlags()
		{
			var Flags = default(PowerCallbackFlags);

			if (PspBattery.BatteryExist) Flags |= PowerCallbackFlags.PSP_POWER_CB_BATTERY_EXIST;
			if (PspBattery.IsStandBy) Flags |= PowerCallbackFlags.PSP_POWER_CB_STANDBY;
			if (PspBattery.BatteryLifePercent == 1) Flags |= PowerCallbackFlags.PSP_POWER_CB_BATTERY_FULL;

			return Flags;
		}

		/// <summary>
		/// Register Power Callback Function
		/// </summary>
		/// <param name="SlotIndex">Slot of the callback in the list, 0 to 15, pass -1 to get an auto assignment.</param>
		/// <param name="cbid">Callback ID from calling sceKernelCreateCallback</param>
		/// <returns> 0 on success, the slot number if -1 is passed, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x04B7766E, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int scePowerRegisterCallback(int SlotIndex, int CallbackId)
		{
			CheckSlotIndex(SlotIndex, AllowMinusOne: true);

			// TODO: If cbId is invalid return PSP_POWER_ERROR_INVALID_CB.
			if (CallbackId == 0)
			{
				throw (new SceKernelException(SceKernelErrors.PSP_POWER_ERROR_INVALID_CB));
			}

			if (SlotIndex == -1)
			{
				try
				{
					SlotIndex = Callbacks.First(Slot => Slot.CallbackId == 0).Index;
				}
				catch (InvalidOperationException)
				{
					throw (new SceKernelException(SceKernelErrors.PSP_POWER_ERROR_SLOTS_FULL));
				}
			}

			if (Callbacks[SlotIndex].CallbackId != 0)
			{
				throw (new SceKernelException(SceKernelErrors.PSP_POWER_ERROR_TAKEN_SLOT));
			}

			Callbacks[SlotIndex].CallbackId = CallbackId;

			var Callback = CallbackManager.Callbacks.Get(CallbackId);
			var RegisteredCallback = HleCallback.Create(
				"scePowerRegisterCallback", Callback.Function,
				SlotIndex + 1, (int)(GetPowerCallbackFlags()), Callback.Arguments[0]
			);

			CallbackManager.ScheduleCallback(RegisteredCallback);

			//throw (new NotImplementedException());
			return SlotIndex;
		}

		/// <summary>
		/// Unregister Power Callback Function
		/// </summary>
		/// <param name="SlotIndex">Slot of the callback</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xDFA8BAF8, FirmwareVersion = 150)]
		[HlePspFunction(NID = 0xDB9D28DD, FirmwareVersion = 150, Name = "scePowerUnregitserCallback")]
		//[HlePspNotImplemented]
		public int scePowerUnregisterCallback(int SlotIndex)
		{
			CheckSlotIndex(SlotIndex, AllowMinusOne: false);

			if (Callbacks[SlotIndex].CallbackId == 0)
			{
				throw (new SceKernelException(SceKernelErrors.PSP_POWER_ERROR_EMPTY_SLOT));
			}

			Callbacks[SlotIndex].CallbackId = 0;

			//throw (new NotImplementedException());
			return 0;
		}
	}
}
