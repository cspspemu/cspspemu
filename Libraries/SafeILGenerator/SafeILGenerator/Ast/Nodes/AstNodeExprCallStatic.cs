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
        public AstNodeExprCallStatic(Delegate Delegate, params AstNodeExpr[] parameters)
            : this(Delegate.Method, parameters)
        {
        }

        public AstNodeExprCallStatic(MethodInfo methodInfo, params AstNodeExpr[] parameters)
            : base(methodInfo, parameters)
        {
        }
    }
}