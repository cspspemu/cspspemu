using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprArrayAccess : AstNodeExprLValue
	{
		public AstNodeExpr ArrayInstance;
		public AstNodeExpr Index;

		public AstNodeExprArrayAccess(AstNodeExpr ArrayInstance, AstNodeExpr Index)
		{
			this.ArrayInstance = ArrayInstance;
			this.Index = Index;
		}

		public Type ElementType
		{
			get
			{
				return ArrayInstance.Type;
			}
		}

		protected override Type UncachedType
		{
			get { return ArrayInstance.Type.GetElementType(); }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref ArrayInstance);
			Transformer.Ref(ref Index);
		}
	}
}
