using System;
using System.Collections.Generic;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprCast : AstNodeExpr
    {
        public Type CastedType;
        public AstNodeExpr Expr;
        public bool Explicit;

        public AstNodeExprCast(Type castedType, AstNodeExpr expr, bool Explicit = true)
        {
            CastedType = castedType;
            Expr = expr;
            this.Explicit = Explicit;
        }

        protected override Type UncachedType => CastedType;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Expr);
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Cast", CastedType.Name},
        };
    }
}