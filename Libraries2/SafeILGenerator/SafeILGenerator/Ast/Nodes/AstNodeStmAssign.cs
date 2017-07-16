using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmAssign : AstNodeStm
	{
		public AstNodeExprLValue LValue;
		public AstNodeExpr Value;

		public AstNodeStmAssign(AstNodeExprLValue LValue, AstNodeExpr Value)
		{
			if (LValue.Type != Value.Type) throw (new Exception(String.Format("Local.Type({0}) != Value.Type({1})", LValue.Type, Value.Type)));

			this.LValue = LValue;
			this.Value = Value;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref LValue);
			Transformer.Ref(ref Value);
		}
	}
}
