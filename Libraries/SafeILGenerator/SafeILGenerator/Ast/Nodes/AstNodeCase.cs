using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeCase : AstNode
	{
		public object CaseValue;
		public AstNodeStm Code;

		public AstNodeCase(object Value, AstNodeStm Code)
		{
			this.CaseValue = Value;
			this.Code = Code;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Code);
		}
	}

	public class AstNodeCaseDefault : AstNode
	{
		public AstNodeStm Code;

		public AstNodeCaseDefault(AstNodeStm Code)
		{
			this.Code = Code;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Code);
		}
	}
}
