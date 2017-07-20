using System;
using System.Collections.Generic;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprArgument : AstNodeExprLValue
    {
        public readonly AstArgument AstArgument;

        public AstNodeExprArgument(AstArgument astArgument)
        {
            AstArgument = astArgument;
        }

        protected override Type UncachedType => AstArgument.Type;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Argument", AstArgument.Name},
        };
    }
}