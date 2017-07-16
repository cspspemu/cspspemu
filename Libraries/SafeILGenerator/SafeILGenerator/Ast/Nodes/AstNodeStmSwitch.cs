using System.Collections.Generic;
using System.Linq;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmSwitch : AstNodeStm
    {
        public AstNodeExpr SwitchValue;
        public AstNodeCase[] Cases;
        public AstNodeCaseDefault CaseDefault;

        public AstNodeStmSwitch(AstNodeExpr switchValue, IEnumerable<AstNodeCase> cases,
            AstNodeCaseDefault caseDefault = null)
        {
            SwitchValue = switchValue;
            Cases = cases.ToArray();
            CaseDefault = caseDefault;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref SwitchValue);
            transformer.Ref(ref Cases);
            if (CaseDefault != null) transformer.Ref(ref CaseDefault);
        }
    }
}