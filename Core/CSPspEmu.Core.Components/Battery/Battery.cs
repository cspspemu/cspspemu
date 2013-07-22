namespace CSPspEmu.Core.Battery
{
	public class Battery
	{
		/// <summary>
		/// 
		/// </summary>
		public bool IsPlugedIn = true;
		
		/// <summary>
		/// 
		/// </summary>
		public bool IsPresent = true;
		
		/// <summary>
		/// 
		/// </summary>
		public bool BatteryExist = true;

		/// <summary>
		/// 
		/// </summary>
		public bool IsStandBy = false;
		
		/// <summary>
		/// 
		/// </summary>
		public bool IsBatteryCharging = true;
		
		/// <summary>
		/// 
		/// </summary>
		public int BatteryLifeTimeInMinutes = 5 * 60;
		
		/// <summary>
		/// 
		/// </summary>
		public bool IsPowerOnline = true;

		/// <summary>
		/// 
		/// </summary>
		public double BatteryLifePercent = 1.0;

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
		public double LowPercent = 0.12;
	}
}
