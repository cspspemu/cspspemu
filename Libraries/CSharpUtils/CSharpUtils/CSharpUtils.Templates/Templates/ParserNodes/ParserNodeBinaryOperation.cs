using System;
using CSharpUtils.Templates.Runtime;

namespace CSharpUtils.Templates.ParserNodes
{
	public class ParserNodeBinaryOperation : ParserNode
	{
		public ParserNode LeftNode;
		public ParserNode RightNode;
		public String Operator;

		public ParserNodeBinaryOperation(ParserNode LeftNode, ParserNode RightNode, String Operator)
		{
			this.LeftNode = LeftNode;
			this.RightNode = RightNode;
			this.Operator = Operator;
		}

		public override void Dump(int Level = 0, String Info = "")
		{
			base.Dump(Level, Info);
			LeftNode.Dump(Level + 1, "Left");
			RightNode.Dump(Level + 1, "Right");
		}

		public override ParserNode Optimize(ParserNodeContext Context)
		{
			var LeftNodeOptimized = LeftNode.Optimize(Context);
			var RightNodeOptimized = RightNode.Optimize(Context);

			if ((LeftNodeOptimized is ParserNodeNumericLiteral) && (RightNodeOptimized is ParserNodeNumericLiteral))
			{
				var LeftNodeLiteral = (ParserNodeNumericLiteral)LeftNodeOptimized;
				var RightNodeLiteral = (ParserNodeNumericLiteral)RightNodeOptimized;
				switch (Operator)
				{
					case "+": return new ParserNodeNumericLiteral() { Value = LeftNodeLiteral.Value + RightNodeLiteral.Value, };
					case "-": return new ParserNodeNumericLiteral() { Value = LeftNodeLiteral.Value - RightNodeLiteral.Value, };
					case "*": return new ParserNodeNumericLiteral() { Value = LeftNodeLiteral.Value * RightNodeLiteral.Value, };
					case "/": return new ParserNodeNumericLiteral() { Value = LeftNodeLiteral.Value / RightNodeLiteral.Value, };
				}
			}
			return this;
		}

		public override void WriteTo(ParserNodeContext Context)
		{
			Context.Write("DynamicUtils.BinaryOperation_" + DynamicUtils.GetOpName(Operator) + "(");
			LeftNode.WriteTo(Context);
			Context.Write(",");
			RightNode.WriteTo(Context);
			Context.Write(")");
		}

		public override string ToString()
		{
			return String.Format("ParserNodeBinaryOperation('{0}')", Operator);
		}
	}
}
