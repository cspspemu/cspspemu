using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmComment : AstNodeStm
	{
		public readonly string CommentText;

		public AstNodeStmComment(string CommentText)
		{
			this.CommentText = CommentText;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Comment", CommentText },
				};
			}
		}
	}
}
