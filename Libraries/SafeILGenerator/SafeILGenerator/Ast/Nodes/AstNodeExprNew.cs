using System;

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