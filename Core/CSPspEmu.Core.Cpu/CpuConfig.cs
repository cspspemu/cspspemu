using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public class CpuConfig
	{
		/// <summary>
		/// 
		/// </summary>
		public bool EnableAstOptimizations = true;
		
		/// <summary>
		/// 
		/// </summary>
		public bool TrackCallStack = false;
		
		/// <summary>
		/// 
		/// </summary>
		public bool CountInstructionsAndYield = false;
		
		/// <summary>
		/// 
		/// </summary>
		public bool LogInstructionStats = true;
		
		/// <summary>
		/// 
		/// </summary>
		public bool DebugSyscalls = false;

		/// <summary>
		/// 
		/// </summary>
		public bool ShowInstructionStats = true;

		/// <summary>
		/// 
		/// </summary>
		public bool ShowInstructionStatsJustNew = true;

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
	}
}
