using System;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprArrayAccess : AstNodeExprLValue
    {
        public AstNodeExpr ArrayInstance;
        public AstNodeExpr Index;

        public AstNodeExprArrayAccess(AstNodeExpr arrayInstance, AstNodeExpr index)
        {
            ArrayInstance = arrayInstance;
            Index = index;
        }

        public Type ElementType => ArrayInstance.Type;

        protected override Type UncachedType => ArrayInstance.Type.GetElementType();

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref ArrayInstance);
            transformer.Ref(ref Index);
        }
    }
}