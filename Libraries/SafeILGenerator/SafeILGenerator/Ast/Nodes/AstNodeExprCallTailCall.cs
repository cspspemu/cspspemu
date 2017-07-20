using System;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprCallTailCall : AstNodeExpr
    {
        public AstNodeExprCall Call;

        public AstNodeExprCallTailCall(AstNodeExprCall call)
        {
            Call = call;
            call.Parent = this;
        }

        protected override Type UncachedType => Call.Type;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Call);
        }
    }
}