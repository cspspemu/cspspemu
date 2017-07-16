using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprGetAddress : AstNodeExprLValue
	{
		public AstNodeExprLValue Expression;

		public AstNodeExprGetAddress(AstNodeExprLValue Expression)
		{
			this.Expression = Expression;
		}

		protected override Type UncachedType
		{
			get { return Expression.Type.MakePointerType(); }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Expression);
		}
	}
}
