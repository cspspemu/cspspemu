namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmIfElse : AstNodeStm
    {
        public AstNodeExpr Condition;
        public AstNodeStm True;
        public AstNodeStm False;

        public AstNodeStmIfElse(AstNodeExpr condition, AstNodeStm @true, AstNodeStm @false = null)
        {
            //if (False == null) False = new AstNodeStmEmpty();
            Condition = condition;
            True = @true;
            False = @false;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Condition);
            if (True != null) transformer.Ref(ref True);
            if (False != null) transformer.Ref(ref False);
        }
    }
}