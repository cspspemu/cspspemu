using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Analyzer
{
	abstract public class AnalyzerExpression
	{
		abstract public String GetExpressionString();
	}

	public class AnalyzerExpressionUndefined : AnalyzerExpression
	{
		public override string GetExpressionString()
		{
			return "undefined";
		}
	}

	public class AnalyzerExpressionConstantInteger : AnalyzerExpression
	{
		public int Value;

		public override string GetExpressionString()
		{
			return String.Format("{0}", Value);
		}
	}

	public class AnalyzerExpressionBinaryOperation : AnalyzerExpression
	{
		public AnalyzerExpression Left;
		public AnalyzerExpression Right;
		public string Operator;

		public override string GetExpressionString()
		{
			return String.Format("({0} {1} {2})", Left.GetExpressionString(), Operator, Right.GetExpressionString());
		}
	}
}
