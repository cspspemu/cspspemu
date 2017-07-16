using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprGetAddress : AstNodeExprLValue
    {
        public AstNodeExprLValue Expression;

        public AstNodeExprGetAddress(AstNodeExprLValue expression)
        {
            Expression = expression;
        }

        protected override Type UncachedType => Expression.Type.MakePointerType();

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Expression);
        }
    }
}