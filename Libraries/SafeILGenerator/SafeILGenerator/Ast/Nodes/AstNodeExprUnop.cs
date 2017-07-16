using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprUnop : AstNodeExpr
    {
        public string Operator;
        public AstNodeExpr RightNode;

        public AstNodeExprUnop(string op, AstNodeExpr rightNode)
        {
            Operator = op;
            RightNode = rightNode;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref RightNode);
        }

        protected override Type UncachedType => RightNode.Type;

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Operator", Operator},
        };
    }
}