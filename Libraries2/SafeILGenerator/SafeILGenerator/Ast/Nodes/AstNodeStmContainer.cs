using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmContainer : AstNodeStm
	{
		public List<AstNodeStm> Nodes;
		public readonly bool Inline = false;

		public AstNodeStmContainer(bool Inline, params AstNodeStm[] Nodes)
		{
			this.Inline = Inline;
			this.Nodes = Nodes.ToList();
		}

		public AstNodeStmContainer(params AstNodeStm[] Nodes)
		{
			this.Inline = false;
			this.Nodes = Nodes.ToList();
		}

		public void AddStatement(AstNodeStm Node)
		{
			this.Nodes.Add(Node);
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Nodes);
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Inline", Inline.ToString() },
				};
			}
		}
	}
}
