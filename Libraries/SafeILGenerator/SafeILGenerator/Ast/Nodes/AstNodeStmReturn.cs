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
                transformer.Ref(ref Expression);
        }
    }
}