using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeStmEmpty : AstNodeStm
	{
		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
		}
	}
}
