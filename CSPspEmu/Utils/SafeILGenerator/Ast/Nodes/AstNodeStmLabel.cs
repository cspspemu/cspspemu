using System.Collections.Generic;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmLabel : AstNodeStm
    {
        public readonly AstLabel AstLabel;

        public AstNodeStmLabel(AstLabel astLabel) => AstLabel = astLabel;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Label", AstLabel.Name},
        };
    }
}