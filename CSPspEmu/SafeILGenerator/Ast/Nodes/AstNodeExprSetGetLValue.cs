using System;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprSetGetLValuePlaceholder : AstNodeExpr
    {
        public AstNodeExprSetGetLValuePlaceholder(Type type)
        {
            UncachedType = type;
        }

        protected override Type UncachedType { get; }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            //throw new NotImplementedException();
        }
    }

    public class AstNodeExprSetGetLValue : AstNodeExprLValue
    {
        public AstNodeExpr SetExpression;
        public AstNodeExpr GetExpression;

        public AstNodeExprSetGetLValue(AstNodeExpr setExpression, AstNodeExpr getExpression)
        {
            SetExpression = setExpression;
            GetExpression = getExpression;
        }

        protected override Type UncachedType => GetExpression.Type;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref SetExpression);
            transformer.Ref(ref GetExpression);
        }
    }
}