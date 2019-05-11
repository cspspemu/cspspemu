using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprNewArray : AstNodeExpr
    {
        public Type ElementType;
        public List<AstNodeExpr> Values;

        public AstNodeExprNewArray(Type elementType, params AstNodeExpr[] values)
        {
            ElementType = elementType;
            Values = values.ToList();
        }

        public AstNodeExprNewArray AddValue(AstNodeExpr astNodeExpr)
        {
            Values.Add(astNodeExpr);
            return this;
        }

        public int Length => Values.Count;

        public override void TransformNodes(TransformNodesDelegate transformer) => transformer.Ref(ref Values);
        protected override Type UncachedType => ElementType.MakeArrayType();
    }
}