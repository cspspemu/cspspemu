using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmThrow : AstNodeStm
	{
		public AstNodeExpr AstNodeExpr;

		public AstNodeStmThrow(AstNodeExpr AstNodeExpr)
		{
			this.AstNodeExpr = AstNodeExpr;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref AstNodeExpr);
		}
	}
}
