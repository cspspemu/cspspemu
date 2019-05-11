using System.Collections.Generic;
using System.Linq;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmContainer : AstNodeStm
    {
        public List<AstNodeStm> Nodes;
        public readonly bool Inline;

        public AstNodeStmContainer(bool inline, params AstNodeStm[] nodes)
        {
            Inline = inline;
            Nodes = nodes.ToList();
        }

        public AstNodeStmContainer(params AstNodeStm[] nodes)
        {
            Inline = false;
            Nodes = nodes.ToList();
        }

        public void AddStatement(AstNodeStm node) => Nodes.Add(node);

        public override void TransformNodes(TransformNodesDelegate transformer) => transformer.Ref(ref Nodes);

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Inline", Inline.ToString()},
        };
    }
}