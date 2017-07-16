using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprNew : AstNodeExpr
	{
		public Type ClassType;
		public AstNodeExpr[] Params;

		public AstNodeExprNew(Type ClassType, params AstNodeExpr[] Params)
		{
			this.ClassType = ClassType;
			this.Params = Params;
		}

		protected override Type UncachedType
		{
			get { return this.ClassType; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref this.Params);
		}
	}
}
