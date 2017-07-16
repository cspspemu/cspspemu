using System.Collections.Generic;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeStmComment : AstNodeStm
    {
        public readonly string CommentText;

        public AstNodeStmComment(string commentText)
        {
            CommentText = commentText;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Comment", CommentText},
        };
    }
}