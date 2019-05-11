using System;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprGetAddress : AstNodeExprLValue
    {
        public AstNodeExprLValue Expression;

        public AstNodeExprGetAddress(AstNodeExprLValue expression)
        {
            Expression = expression;
        }

        protected override Type UncachedType => Expression.Type.MakePointerType();

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Expression);
        }
    }
}