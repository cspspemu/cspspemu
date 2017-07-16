using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmIfElse : AstNodeStm
	{
		public AstNodeExpr Condition;
		public AstNodeStm True;
		public AstNodeStm False;

		public AstNodeStmIfElse(AstNodeExpr Condition, AstNodeStm True, AstNodeStm False = null)
		{
			//if (False == null) False = new AstNodeStmEmpty();
			this.Condition = Condition;
			this.True = True;
			this.False  = False;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Condition);
			if (True != null) Transformer.Ref(ref True);
			if (False != null) Transformer.Ref(ref False);
		}
	}
}
