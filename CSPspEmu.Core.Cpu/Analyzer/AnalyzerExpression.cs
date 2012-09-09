using System;

namespace CSPspEmu.Core.Cpu.Analyzer
{
	public abstract class AnalyzerExpression
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
