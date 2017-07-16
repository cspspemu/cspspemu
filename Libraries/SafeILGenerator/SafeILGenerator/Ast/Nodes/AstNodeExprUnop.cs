using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprUnop : AstNodeExpr
	{
		public string Operator;
		public AstNodeExpr RightNode;

		public AstNodeExprUnop(string Operator, AstNodeExpr RightNode)
		{
			this.Operator = Operator;
			this.RightNode = RightNode;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref RightNode);
		}

		protected override Type UncachedType
		{
			get
			{
				return RightNode.Type;
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
