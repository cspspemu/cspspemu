using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprTerop : AstNodeExpr
	{
		public AstNodeExpr Cond;
		public AstNodeExpr True;
		public AstNodeExpr False;

		public AstNodeExprTerop(AstNodeExpr Cond, AstNodeExpr True, AstNodeExpr False)
		{
			if (True.Type != False.Type) throw(new Exception("Condition mismatch"));
			this.Cond = Cond;
			this.True = True;
			this.False = False;
		}

		protected override Type UncachedType
		{
			get { return this.True.Type; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Cond);
			Transformer.Ref(ref True);
			Transformer.Ref(ref False);
		}
	}
}
