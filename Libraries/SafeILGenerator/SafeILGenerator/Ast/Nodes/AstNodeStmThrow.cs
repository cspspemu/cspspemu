namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmThrow : AstNodeStm
    {
        public AstNodeExpr AstNodeExpr;

        public AstNodeStmThrow(AstNodeExpr astNodeExpr)
        {
            AstNodeExpr = astNodeExpr;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref AstNodeExpr);
        }
    }
}