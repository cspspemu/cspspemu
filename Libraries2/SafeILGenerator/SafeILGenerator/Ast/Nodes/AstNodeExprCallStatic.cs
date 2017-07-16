using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprCallStatic : AstNodeExprCall
	{
		public AstNodeExprCallStatic(Delegate Delegate, params AstNodeExpr[] Parameters)
			: this(Delegate.Method, Parameters)
		{
			
		}

		public AstNodeExprCallStatic(MethodInfo MethodInfo, params AstNodeExpr[] Parameters)
			: base(MethodInfo, Parameters)
		{
		}
	}
}
