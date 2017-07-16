using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprIndirect : AstNodeExprLValue
	{
		public AstNodeExpr PointerExpression;

		public AstNodeExprIndirect(AstNodeExpr AstNodeExpr)
		{
			this.PointerExpression = AstNodeExpr;
		}

		protected override Type UncachedType
		{
			get
			{
				if (!PointerExpression.Type.IsPointer) throw(new Exception("Not a pointer"));
				return PointerExpression.Type.GetElementType();
			}
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref PointerExpression);
		}
	}
}
