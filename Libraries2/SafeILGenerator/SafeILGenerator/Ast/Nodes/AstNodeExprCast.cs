using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprCast : AstNodeExpr
	{
		public Type CastedType;
		public AstNodeExpr Expr;
		public bool Explicit = true;

		public AstNodeExprCast(Type CastedType, AstNodeExpr Expr, bool Explicit = true)
		{
			this.CastedType = CastedType;
			this.Expr = Expr;
			this.Explicit = Explicit;
		}

		protected override Type UncachedType
		{
			get { return CastedType; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Expr);
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Cast", CastedType.Name },
				};
			}
		}
	}
}
