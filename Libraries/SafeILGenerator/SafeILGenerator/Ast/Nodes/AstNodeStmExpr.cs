namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmExpr : AstNodeStm
    {
        public AstNodeExpr AstNodeExpr;

        public AstNodeStmExpr(AstNodeExpr astNodeExpr)
        {
            AstNodeExpr = astNodeExpr;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref AstNodeExpr);
        }
    }
}