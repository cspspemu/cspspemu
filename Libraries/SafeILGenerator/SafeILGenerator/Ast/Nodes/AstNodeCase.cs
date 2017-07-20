namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeCase : AstNode
    {
        public object CaseValue;
        public AstNodeStm Code;

        public AstNodeCase(object value, AstNodeStm code)
        {
            CaseValue = value;
            Code = code;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Code);
        }
    }

    public class AstNodeCaseDefault : AstNode
    {
        public AstNodeStm Code;

        public AstNodeCaseDefault(AstNodeStm code)
        {
            this.Code = code;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Code);
        }
    }
}