using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Analyzer
{
	public class AnalyzerValue
	{
		/// <summary>
		/// 
		/// </summary>
		public int UpdateCount = 0;

		/// <summary>
		/// 
		/// </summary>
		public int UsedCount = 0;

		/// <summary>
		/// 
		/// </summary>
		public AnalyzerExpression Expression = new AnalyzerExpressionUndefined();
	}
}
