using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprNewArray : AstNodeExpr
	{
		public Type ElementType;
		public List<AstNodeExpr> Values;

		public AstNodeExprNewArray(Type ElementType, params AstNodeExpr[] Values)
		{
			this.ElementType = ElementType;
			this.Values = Values.ToList();
		}

		public AstNodeExprNewArray AddValue(AstNodeExpr AstNodeExpr)
		{
			this.Values.Add(AstNodeExpr);
			return this;
		}

		public int Length
		{
			get
			{
				return Values.Count;
			}
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Values);
		}

		protected override Type UncachedType
		{
			get
			{
				return ElementType.MakeArrayType();
			}
		}
	}
}
