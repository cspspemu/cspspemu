using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprCallInstance : AstNodeExprCall
    {
        public AstNodeExpr Instance;

        public AstNodeExprCallInstance(AstNodeExpr instance, Delegate mydelegate, params AstNodeExpr[] parameters)
            : this(instance, mydelegate.Method, parameters)
        {
        }

        public AstNodeExprCallInstance(AstNodeExpr instance, MethodInfo methodInfo, params AstNodeExpr[] parameters)
            : base(methodInfo, parameters)
        {
            this.Instance = instance;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Instance);
            base.TransformNodes(transformer);
        }
    }

    public class AstNodeExprCallDelegate : AstNodeExprCallInstance
    {
        private static MethodInfo GetInvoke(Type type) =>
            type.GetMethod("Invoke") ?? throw new Exception($"Can't get Invoke method for Type {type}");

        public AstNodeExprCallDelegate(AstNodeExpr Object, params AstNodeExpr[] parameters)
            : base(Object, GetInvoke(Object.Type), parameters)
        {
        }
    }
}