using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.power
{
	unsafe public partial class scePower : HleModuleHost
	{


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

		/**
		 * Request the PSP to go into standby
		 *
		 * @return 0 always
		 */
		[HlePspFunction(NID = 0x2B7C7CF4, FirmwareVersion = 150)]
		public int scePowerRequestStandby()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Request the PSP to go into suspend
		 *
		 * @return 0 always
		 */
		[HlePspFunction(NID = 0xAC32C9CC, FirmwareVersion = 150)]
		public int scePowerRequestSuspend()
		{
			throw(new NotImplementedException());
		}

		/**
		 * unknown? - crashes PSP in usermode
		 */
		[HlePspFunction(NID = 0x862AE1A6, FirmwareVersion = 150)]
		public int scePowerGetBatteryElec()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get Idle timer
		 */
		[HlePspFunction(NID = 0xEDC13FE5, FirmwareVersion = 150)]
		public int scePowerGetIdleTimer()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Enable Idle timer
		 *
		 * @param unknown - pass 0
		 */
		[HlePspFunction(NID = 0x7F30B3B1, FirmwareVersion = 150)]
		public int scePowerIdleTimerEnable(int unknown)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Disable Idle timer
		 *
		 * @param unknown - pass 0
		 */
		[HlePspFunction(NID = 0x972CE941, FirmwareVersion = 150)]
		public int scePowerIdleTimerDisable(int unknown)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Generate a power tick, preventing unit from 
		 * powering off and turning off display.
		 *
		 * @param type - Either PSP_POWER_TICK_ALL, PSP_POWER_TICK_SUSPEND or PSP_POWER_TICK_DISPLAY
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0xEFD3C963, FirmwareVersion = 150)]
		public int scePowerTick(int type)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Check if unit is plugged in
		 *
		 * @return 1 if plugged in, 0 if not plugged in, < 0 on error.
		 */
		[HlePspFunction(NID = 0x87440F5E, FirmwareVersion = 150)]
		public int scePowerIsPowerOnline()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Check if a battery is present
		 *
		 * @return 1 if battery present, 0 if battery not present, < 0 on error.
		 */
		[HlePspFunction(NID = 0x0AFD0D8B, FirmwareVersion = 150)]
		public int scePowerIsBatteryExist()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Check if the battery is charging
		 *
		 * @return 1 if battery charging, 0 if battery not charging, < 0 on error.
		 */
		[HlePspFunction(NID = 0x1E490401, FirmwareVersion = 150)]
		public int scePowerIsBatteryCharging()
		{
			throw(new NotImplementedException());
		}
	
		/**
		 * Get the status of the battery charging
		 */
		[HlePspFunction(NID = 0xB4432BC8, FirmwareVersion = 150)]
		public int scePowerGetBatteryChargingStatus()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Check if the battery is low
		 *
		 * @return 1 if the battery is low, 0 if the battery is not low, < 0 on error.
		 */
		[HlePspFunction(NID = 0xD3075926, FirmwareVersion = 150)]
		public int scePowerIsLowBattery()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get battery life as integer percent
		 *
		 * @return Battery charge percentage (0-100), < 0 on error.
		 */
		[HlePspFunction(NID = 0x2085D15D, FirmwareVersion = 150)]
		public int scePowerGetBatteryLifePercent()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get battery life as time
		 *
		 * @return Battery life in minutes, < 0 on error.
		 */
		[HlePspFunction(NID = 0x8EFB3FA2, FirmwareVersion = 150)]
		public int scePowerGetBatteryLifeTime()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get temperature of the battery on deg C
		 */
		[HlePspFunction(NID = 0x28E12023, FirmwareVersion = 150)]
		public int scePowerGetBatteryTemp()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get battery volt level
		 */
		[HlePspFunction(NID = 0x483CE86B, FirmwareVersion = 150)]
		public int scePowerGetBatteryVolt()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Lock power switch
		 *
		 * Note: if the power switch is toggled while locked
		 * it will fire immediately after being unlocked.
		 *
		 * @param unknown - pass 0
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0xD6D016EF, FirmwareVersion = 150)]
		public int scePowerLock(int unknown)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Unlock power switch
		 *
		 * @param unknown - pass 0
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0xCA3D34C1, FirmwareVersion = 150)]
		public int scePowerUnlock(int unknown)
		{
			throw(new NotImplementedException());
		}
	}
}
