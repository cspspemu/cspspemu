using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Analyzer
{
	public class AnalyzerState
	{
		/// <summary>
		/// 
		/// </summary>
		public Dictionary<uint, AnalyzerValue> Registers = new Dictionary<uint, AnalyzerValue>();

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<uint, AnalyzerValue> Stack = new Dictionary<uint, AnalyzerValue>();
	}
}
