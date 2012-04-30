using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Factory;
using System.Reflection;
using CSPspEmu.Resources;

namespace CSPspEmu.Core
{
	public class PspConfig
	{
		/// <summary>
		/// 
		/// </summary>
		public bool UseCoRoutines = false;

		/// <summary>
		/// 
		/// </summary>
		public PspEmulatorContext PspEmulatorContext;

		/// <summary>
		/// 
		/// </summary>
		static public string CultureName = "en-US";

		/// <summary>
		/// 
		/// </summary>
		public bool VerticalSynchronization
		{
			get
			{
				return StoredConfig.LimitVerticalSync;
			}
			set
			{
				StoredConfig.LimitVerticalSync = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool TraceJIT = false;

		/// <summary>
		/// 
		/// </summary>
		public bool CountInstructionsAndYield = true;


		/// <summary>
		/// 
		/// </summary>
		public bool ShowInstructionStats = true;
		//public bool ShowInstructionStats = false;


		/// <summary>
		/// 
		/// </summary>
		public bool ShowInstructionStatsJustNew = true;

		/// <summary>
		/// 
		/// </summary>
		public bool LogInstructionStats = true;

		/// <summary>
		/// Specifies if the current emulator has a display.
		/// This will be used for the automated tests.
		/// </summary>
		public bool HasDisplay = true;

		/// <summary>
		/// 
		/// </summary>
		public bool DebugSyscalls = false;
		//public bool DebugSyscalls = true;

		// http://jpcsp.googlecode.com/svn/trunk/src/jpcsp/HLE/modules150/scePower.java

		/*
		public int BatteryLifeTime = (5 * 60); // 5 hours
		public int BatteryTemp = 28; //some standard battery temperature 28 deg C
		public int BatteryVoltage = 4135; //battery voltage 4,135 in slim
		public bool PluggedIn = true;
		public bool BatteryPresent = true;
		public int BatteryPowerPercent = 100;
		public int BatteryLowPercent = 12;
		public int BatteryForceSuspendPercent = 4;
		public int FullBatteryCapacity = 1800;
		public bool BatteryCharging = false;
		public int BacklightMaximum = 4;
		*/

		/// <summary>
		/// CPU clock:
		/// Operates at variable rates from 1MHz to 333MHz.
		/// Starts at 222MHz.
		/// Note: Cannot have a higher frequency than the PLL clock's frequency.
		/// </summary>
		public int CpuFrequency = 222;

		/// <summary>
		/// PLL clock:
		/// Operates at fixed rates of 148MHz, 190MHz, 222MHz, 266MHz, 333MHz.
		/// Starts at 222MHz.
		/// </summary>
		public int PllFrequency = 222;

		/// <summary>
		/// BUS clock:
		/// Operates at variable rates from 37MHz to 166MHz.
		/// Starts at 111MHz.
		/// Note: Cannot have a higher frequency than 1/2 of the PLL clock's frequency
		/// or lower than 1/4 of the PLL clock's frequency.
		/// </summary>
		public int BusFrequency = 111;

		/// <summary>
		/// 
		/// </summary>
		public bool NoticeUnimplementedGpuCommands = true;
		//public bool NoticeUnimplementedGpuCommands = false;

		/// <summary>
		/// Writes a line each time a JAL is executed
		/// </summary>
		public bool TraceJal = false;
		//public bool TraceJal = true;

		/// <summary>
		/// 
		/// </summary>
		public bool DebugThreadSwitching = false;
		//public bool DebugThreadSwitching = true;

		/// <summary>
		/// 
		/// </summary>
		public bool BreakInstructionThreadSwitchingForSpeed = false;
		//public bool BreakInstructionThreadSwitchingForSpeed = true;

		/// <summary>
		/// 
		/// </summary>
		public Assembly HleModulesDll;

		/// <summary>
		/// 
		/// </summary>
		public readonly PspStoredConfig StoredConfig;

		/// <summary>
		/// 
		/// </summary>
		public PspConfig()
		{
			StoredConfig = PspStoredConfig.Load();
		}

		public bool InfoExeHasRelocation = false;

		/// <summary>
		/// 
		/// </summary>
		public bool TraceThreadLoop = false;
		public uint RelocatedBaseAddress;

		static private string _GameUnknownTitle = Translations.GetString("extra", "UnknownGame");
		private string _GameTitle;
		public string GameTitle
		{
			get
			{
				if (_GameTitle == null) return _GameUnknownTitle;
				return _GameTitle;
			}
			set
			{
				_GameTitle = value;
			}
		}
		public bool TrackCallStack = true;

		// Until more stable.
		//public bool EnableMpeg = true;
		//public bool EnableMpeg = false;
		/*
		public bool EnableMpeg
		{
			get
			{
				return StoredConfig.EnableMpeg;
			}
		}
		*/

		public bool MustLogWrites = false;
		//public bool SetPCWriteAddress

		//public bool TraceThreadLoop = true;

		//public bool TraceJal = true;

		public string FileNameBase;
		public PspLanguages Language;
		public PspConfirmButton ConfirmButton;
		//public bool TraceLastSyscalls = false;
		public bool TraceLastSyscalls = true;

		public bool DebugNotImplemented = true;
		public bool WlanIsOn = false;
	}
}
