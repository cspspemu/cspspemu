using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmSwitch : AstNodeStm
	{
		public AstNodeExpr SwitchValue;
		public AstNodeCase[] Cases;
		public AstNodeCaseDefault CaseDefault;

		public AstNodeStmSwitch(AstNodeExpr SwitchValue, IEnumerable<AstNodeCase> Cases, AstNodeCaseDefault CaseDefault = null)
		{
			this.SwitchValue = SwitchValue;
			this.Cases = Cases.ToArray();
			this.CaseDefault = CaseDefault;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref SwitchValue);
			Transformer.Ref(ref Cases);
			if (CaseDefault != null) Transformer.Ref(ref CaseDefault);
		}
	}
}
