using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprBinop : AstNodeExpr
	{
		public AstNodeExpr LeftNode;
		public string Operator;
		public AstNodeExpr RightNode;

		public AstNodeExprBinop(AstNodeExpr LeftNode, string Operator, AstNodeExpr RightNode)
		{
			this.LeftNode = LeftNode;
			this.Operator = Operator;
			this.RightNode = RightNode;
			CheckCompatibleTypes();
		}

		private void CheckCompatibleTypes()
		{
			bool Compatible = true;

			if (AstUtils.GetTypeSize(LeftNode.Type) < AstUtils.GetTypeSize(RightNode.Type))
			{
				Compatible = false;
			}

			if (OperatorRequireBoolOperands(Operator) && (LeftNode.Type != typeof(bool)) && (RightNode.Type != typeof(bool)))
			{
				Compatible = false;
			}

			if (!Compatible) throw (new Exception(String.Format("Left.Type({0}) Right.Type({1}) are not compatibles Operator: {2}", LeftNode.Type, RightNode.Type, Operator)));
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref LeftNode);
			Transformer.Ref(ref RightNode);
		}

		static public bool OperatorRequireBoolOperands(string Operator)
		{
			switch (Operator)
			{
				case "&&":
				case "||":
					return true;
				default:
					return false;
			}
		}

		static public bool OperatorReturnsBool(string Operator)
		{
			switch (Operator)
			{
				case "==":
				case "!=":
				case "<":
				case "<=":
				case ">":
				case ">=":
				case "&&":
				case "||":
					return true;
				default:
					return false;
			}
		}

		public Type ResultType
		{
			get
			{
				if (OperatorReturnsBool(Operator)) return typeof(bool);
				return LeftNode.Type;
			}
		}

		protected override Type UncachedType
		{
			get
			{
				return ResultType;
			}
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Operator", Operator },
				};
			}
		}
	}
}
