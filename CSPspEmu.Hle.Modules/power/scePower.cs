using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Battery;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.power
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class scePower : HleModuleHost
	{
		/// <summary>
		/// Power callback flags
		/// </summary>
		[Flags]
		public enum PowerFlagsSet : uint
		{
			/// <summary>
			/// PSP_POWER_CB_POWER_SWITCH
			/// Indicates the power switch it pushed, putting the unit into suspend mode
			/// </summary>
			PowerSwitch = 0x80000000,
			
			/// <summary>
			/// PSP_POWER_CB_HOLD_SWITCH
			/// Indicates the hold switch is on
			/// </summary>
			HoldSwitch = 0x40000000,
			
			/// <summary>
			/// PSP_POWER_CB_STANDBY
			/// What is standby mode?
			/// </summary>
			StandBy = 0x00080000,
			
			/// <summary>
			/// PSP_POWER_CB_RESUME_COMPLETE
			/// Indicates the resume process has been completed (only seems to be triggered when another event happens)
			/// </summary>
			ResumeComplete = 0x00040000,
			
			/// <summary>
			/// PSP_POWER_CB_RESUMING
			/// Indicates the unit is resuming from suspend mode
			/// </summary>
			Resuming = 0x00020000,
			
			/// <summary>
			/// PSP_POWER_CB_SUSPENDING
			/// Indicates the unit is suspending, seems to occur due to inactivity
			/// </summary>
			Suspending = 0x00010000,
			
			/// <summary>
			/// PSP_POWER_CB_AC_POWER
			/// Indicates the unit is plugged into an AC outlet
			/// </summary>
			AcPower = 0x00001000,
			
			/// <summary>
			/// PSP_POWER_CB_BATTERY_LOW
			/// Indicates the battery charge level is low
			/// </summary>
			BatteryLow = 0x00000100,
			
			/// <summary>
			/// PSP_POWER_CB_BATTERY_EXIST
			/// Indicates there is a battery present in the unit
			/// </summary>
			BatteryExists = 0x00000080,
			
			/// <summary>
			/// PSP_POWER_CB_BATTPOWER
			/// Unknown
			/// </summary>
			BatteryPower = 0x0000007F,
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x0074EF9B, FirmwareVersion = 150)]
		public void scePowerGetResumeCount()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x3951AF53, FirmwareVersion = 150)]
		public void scePowerWaitRequestCompletion()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x2875994B, FirmwareVersion = 150)]
		public void scePower_2875994B()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x7FA406DD, FirmwareVersion = 150)]
		public void scePowerIsRequest()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x23436A4A, FirmwareVersion = 150)]
		public void scePower_23436A4A()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x78A1A796, FirmwareVersion = 150)]
		public void scePower_78A1A796()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xE8E4E204, FirmwareVersion = 150)]
		public void scePower_E8E4E204()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x2B51FE2F, FirmwareVersion = 150)]
		public void scePower_2B51FE2F()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x442BFBAC, FirmwareVersion = 150)]
		public void scePower_442BFBAC()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x27F3292C, FirmwareVersion = 150)]
		public void scePowerBatteryUpdateInfo()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xB999184C, FirmwareVersion = 150)]
		public void scePowerGetLowBatteryCapacity()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x94F5A53F, FirmwareVersion = 150)]
		public void scePowerGetBatteryRemainCapacity()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xFD18A0FF, FirmwareVersion = 150)]
		public void scePowerGetBatteryFullCapacity()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x0CD21B1F, FirmwareVersion = 150)]
		public void scePowerSetPowerSwMode()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x165CE085, FirmwareVersion = 150)]
		public void scePowerGetPowerSwMode()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xDB62C9CF, FirmwareVersion = 150)]
		public void scePowerCancelRequest()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Request the PSP to go into standby
		/// </summary>
		/// <returns>0 always</returns>
		[HlePspFunction(NID = 0x2B7C7CF4, FirmwareVersion = 150)]
		public int scePowerRequestStandby()
		{
			return 0;
		}

		/// <summary>
		/// Request the PSP to go into suspend
		/// </summary>
		/// <returns>0 always</returns>
		[HlePspFunction(NID = 0xAC32C9CC, FirmwareVersion = 150)]
		public int scePowerRequestSuspend()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// crashes PSP in usermode
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x862AE1A6, FirmwareVersion = 150)]
		public int scePowerGetBatteryElec()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get Idle timer
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xEDC13FE5, FirmwareVersion = 150)]
		public int scePowerGetIdleTimer()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Enable Idle timer
		/// </summary>
		/// <param name="unknown"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x7F30B3B1, FirmwareVersion = 150)]
		public int scePowerIdleTimerEnable(int unknown)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Disable Idle timer
		/// </summary>
		/// <param name="unknown"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x972CE941, FirmwareVersion = 150)]
		public int scePowerIdleTimerDisable(int unknown)
		{
			throw(new NotImplementedException());
		}

		public enum PspPowerTick : uint
		{
			/// <summary>
			/// All
			/// PSP_POWER_TICK_ALL
			/// </summary>
			All = 0,

			/// <summary>
			/// Suspend
			/// PSP_POWER_TICK_SUSPEND
			/// </summary>
			Suspend = 1,

			/// <summary>
			/// Display
			/// PSP_POWER_TICK_DISPLAY
			/// </summary>
			Display = 6,
		}

		/// <summary>
		/// Generate a power tick, preventing unit from 
		/// powering off and turning off display.
		/// </summary>
		/// <param name="type"></param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xEFD3C963, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePowerTick(PspPowerTick type)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Check if unit is plugged in
		/// </summary>
		/// <returns>1 if plugged in, 0 if not plugged in, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x87440F5E, FirmwareVersion = 150)]
		public int scePowerIsPowerOnline()
		{
			return HleState.PspBattery.IsPowerOnline ? 1 : 0;
		}

		/// <summary>
		/// Check if a battery is present
		/// </summary>
		/// <returns>1 if battery present, 0 if battery not present, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x0AFD0D8B, FirmwareVersion = 150)]
		public int scePowerIsBatteryExist()
		{
			return HleState.PspBattery.BatteryExist ? 1 : 0;
		}

		/// <summary>
		/// Check if the battery is charging
		/// </summary>
		/// <returns>1 if battery charging, 0 if battery not charging, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x1E490401, FirmwareVersion = 150)]
		public int scePowerIsBatteryCharging()
		{
			return PspBattery.IsBatteryCharging ? 1 : 0;
		}

		public Battery PspBattery
		{
			get
			{
				return HleState.PspBattery;
			}
		}

		/// <summary>
		/// Get the status of the battery charging
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB4432BC8, FirmwareVersion = 150)]
		public PowerFlagsSet scePowerGetBatteryChargingStatus()
		{
			var Status = default(PowerFlagsSet);

			if (PspBattery.IsPresent) Status |= PowerFlagsSet.BatteryExists;
			if (PspBattery.IsPlugedIn) Status |= PowerFlagsSet.AcPower;
			if (PspBattery.IsBatteryCharging) Status |= PowerFlagsSet.BatteryPower;

			return Status;
		}

		/// <summary>
		/// Check if the battery is low
		/// </summary>
		/// <returns>1 if the battery is low, 0 if the battery is not low, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xD3075926, FirmwareVersion = 150)]
		public int scePowerIsLowBattery()
		{
			return (HleState.PspBattery.BatteryLifePercent <= HleState.PspBattery.LowPercent) ? 1 : 0;
		}

		/// <summary>
		/// Get battery life as integer percent
		/// </summary>
		/// <returns>Battery charge percentage (0-100), less than 0 on error.</returns>
		[HlePspFunction(NID = 0x2085D15D, FirmwareVersion = 150)]
		public int scePowerGetBatteryLifePercent()
		{
			return HleState.PspBattery.BatteryLifePercent;
		}

		/// <summary>
		/// Get battery life as time
		/// </summary>
		/// <returns>Battery life in minutes, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x8EFB3FA2, FirmwareVersion = 150)]
		public int scePowerGetBatteryLifeTime()
		{
			return HleState.PspBattery.BatteryLifeTimeInMinutes;
		}

		/// <summary>
		/// Get temperature of the battery on deg C
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x28E12023, FirmwareVersion = 150)]
		public int scePowerGetBatteryTemp()
		{
			return HleState.PspBattery.BatteryTemperature;
		}

		/// <summary>
		/// Get battery volt level
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x483CE86B, FirmwareVersion = 150)]
		public int scePowerGetBatteryVolt()
		{
			return HleState.PspBattery.BatteryVoltage;
		}

		/// <summary>
		/// Lock power switch
		/// 
		/// Note: if the power switch is toggled while locked
		/// it will fire immediately after being unlocked.
		/// </summary>
		/// <param name="unknown">pass 0</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xD6D016EF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePowerLock(int unknown)
		{
			return 0;
		}

		/// <summary>
		/// Unlock power switch
		/// </summary>
		/// <param name="unknown">pass 0</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xCA3D34C1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePowerUnlock(int unknown)
		{
			return 0;
		}
	}
}
