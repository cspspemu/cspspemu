using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmReturn : AstNodeStm
    {
        public AstNodeExpr Expression;

        public AstNodeStmReturn(AstNodeExpr expression = null)
        {
            Expression = expression;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            if (Expression != null)
            {
                transformer.Ref(ref Expression);
            }
        }
    }
}