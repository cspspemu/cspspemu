using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast
{
	public class AstFunction
	{
		public List<AstArgument> Arguments;
		public List<AstLocal> Locals;
		public List<AstLabel> Labels;
		public AstNodeStmContainer Root;
	}
}
