using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Battery
{
	public class Battery : PspEmulatorComponent
	{
		public override void InitializeComponent()
		{
		}

		public bool IsPlugedIn = true;
		public bool IsPresent = true;
		public bool BatteryExist = true;
		public bool IsBatteryCharging = true;
		public int BatteryLifeTimeInMinutes = 5 * 60;
		public bool IsPowerOnline = true;
		public int BatteryLifePercent = 100;

		/// <summary>
		/// Some standard battery temperature 28 deg C
		/// </summary>
		public int BatteryTemperature = 28;
		
		/// <summary>
		/// Battery voltage 4,135 in slim
		/// </summary>
		public int BatteryVoltage = 4135;

		/// <summary>
		/// Led starts flashing at 12%
		/// </summary>
		public int LowPercent = 12;
	}
}
