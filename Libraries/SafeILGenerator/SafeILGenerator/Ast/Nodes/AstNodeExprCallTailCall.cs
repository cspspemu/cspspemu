using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprCallTailCall : AstNodeExpr
	{
		public AstNodeExprCall Call;

		public AstNodeExprCallTailCall(AstNodeExprCall Call)
		{
			this.Call = Call;
			Call.Parent = this;
		}

		protected override Type UncachedType
		{
			get { return Call.Type; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Call);
		}
	}
}
