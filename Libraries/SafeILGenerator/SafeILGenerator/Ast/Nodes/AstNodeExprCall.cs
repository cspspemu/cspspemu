using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public abstract class AstNodeExprCall : AstNodeExpr
    {
        public bool IsTail => Parent is AstNodeExprCallTailCall;
        public MethodInfo MethodInfo;
        public AstNodeExpr[] Parameters;

        protected AstNodeExprCall(MethodInfo methodInfo, params AstNodeExpr[] parameters)
        {
            if (methodInfo == null) throw (new Exception("MethodInfo can't be null"));

            var methodParameters = methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            var parametersTypes = parameters.Select(parameter => parameter.Type).ToArray();

            if (!methodParameters.SequenceEqual(parametersTypes))
            {
                throw (new Exception(
                    $"Parameters mismatch Function({string.Join(",", (IEnumerable<Type>) methodParameters)}) != TryingToCall({string.Join(",", (IEnumerable<Type>) parametersTypes)}) for Method({methodInfo})")
                );
            }

            MethodInfo = methodInfo;
            Parameters = parameters;
        }

        protected override Type UncachedType => MethodInfo.ReturnType;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Parameters);
        }
    }
}