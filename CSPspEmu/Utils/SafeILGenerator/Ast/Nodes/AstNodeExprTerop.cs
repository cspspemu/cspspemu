using System;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprTerop : AstNodeExpr
    {
        public AstNodeExpr Cond;
        public AstNodeExpr True;
        public AstNodeExpr False;

        public AstNodeExprTerop(AstNodeExpr cond, AstNodeExpr @true, AstNodeExpr @false)
        {
            if (@true.Type != @false.Type) throw new Exception("Condition mismatch");
            Cond = cond;
            True = @true;
            False = @false;
        }

        protected override Type UncachedType => True.Type;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Cond);
            transformer.Ref(ref True);
            transformer.Ref(ref False);
        }
    }
}