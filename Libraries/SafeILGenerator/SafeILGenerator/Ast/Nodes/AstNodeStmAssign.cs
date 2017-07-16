using System;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmAssign : AstNodeStm
    {
        public AstNodeExprLValue LeftValue;
        public AstNodeExpr Value;

        public AstNodeStmAssign(AstNodeExprLValue leftValue, AstNodeExpr value)
        {
            if (leftValue.Type != value.Type)
                throw (new Exception($"Local.Type({leftValue.Type}) != Value.Type({value.Type})"));

            LeftValue = leftValue;
            Value = value;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref LeftValue);
            transformer.Ref(ref Value);
        }
    }
}