using System;
using System.Reflection;

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