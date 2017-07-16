using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprSetGetLValuePlaceholder : AstNodeExpr
	{
		private Type Type;

		public AstNodeExprSetGetLValuePlaceholder(Type Type)
		{
			this.Type = Type;
		}

		protected override Type UncachedType
		{
			get { return Type; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			//throw new NotImplementedException();
		}
	}

	public class AstNodeExprSetGetLValue : AstNodeExprLValue
	{
		public AstNodeExpr SetExpression;
		public AstNodeExpr GetExpression;

		public AstNodeExprSetGetLValue(AstNodeExpr SetExpression, AstNodeExpr GetExpression)
		{
			this.SetExpression = SetExpression;
			this.GetExpression = GetExpression;
		}

		protected override Type UncachedType
		{
			get { return GetExpression.Type; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref SetExpression);
			Transformer.Ref(ref GetExpression);
		}
	}
}
