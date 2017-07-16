using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmLabel : AstNodeStm
	{
		public readonly AstLabel AstLabel;

		public AstNodeStmLabel(AstLabel AstLabel)
		{
			this.AstLabel = AstLabel;
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
					{ "Label", AstLabel.Name },
				};
			}
		}
	}
}
