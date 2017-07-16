using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprNew : AstNodeExpr
    {
        public Type ClassType;
        public AstNodeExpr[] Params;

        public AstNodeExprNew(Type classType, params AstNodeExpr[] @params)
        {
            ClassType = classType;
            Params = @params;
        }

        protected override Type UncachedType => ClassType;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Params);
        }
    }
}