using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Factory;
using System.Reflection;

namespace CSPspEmu.Core
{
	public class PspConfig
	{
		/// <summary>
		/// 
		/// </summary>
		static public string CultureName = "en";

		/// <summary>
		/// 
		/// </summary>
		public bool VerticalSynchronization = true;

		/// <summary>
		/// 
		/// </summary>
		public bool TraceJIT = false;

		/// <summary>
		/// 
		/// </summary>
		public bool CountInstructionsAndYield = true;

		//public bool ShowInstructionStats = true;
		public bool ShowInstructionStats = false;

		/// <summary>
		/// Specifies if the current emulator has a display.
		/// This will be used for the automated tests.
		/// </summary>
		public bool HasDisplay = true;

		/// <summary>
		/// 
		/// </summary>
		public bool DebugSyscalls = false;

		/// <summary>
		/// 
		/// </summary>
		public int CpuFrequency = 222;

		/// <summary>
		/// 
		/// </summary>
		//public bool NoticeUnimplementedGpuCommands = true;
		public bool NoticeUnimplementedGpuCommands = false;

		/// <summary>
		/// 
		/// </summary>
		public bool UseFastAndUnsaferMemory = true;
		//public bool UseFastAndUnsaferMemory = false;

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
		public PspConfig()
		{
		}

		//public bool TraceJal = true;
	}
}
