using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprBinop : AstNodeExpr
    {
        public AstNodeExpr LeftNode;
        public string Operator;
        public AstNodeExpr RightNode;

        public AstNodeExprBinop(AstNodeExpr leftNode, string op, AstNodeExpr rightNode)
        {
            LeftNode = leftNode;
            Operator = op;
            RightNode = rightNode;
            CheckCompatibleTypes();
        }

        private void CheckCompatibleTypes()
        {
            var compatible = !(AstUtils.GetTypeSize(LeftNode.Type) < AstUtils.GetTypeSize(RightNode.Type));

            if (OperatorRequireBoolOperands(Operator) && (LeftNode.Type != typeof(bool)) &&
                (RightNode.Type != typeof(bool)))
            {
                compatible = false;
            }

            if (!compatible)
                throw (new Exception(
                    $"Left.Type({LeftNode.Type}) Right.Type({RightNode.Type}) are not compatibles Operator: {Operator}")
                );
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref LeftNode);
            transformer.Ref(ref RightNode);
        }

        public static bool OperatorRequireBoolOperands(string Operator)
        {
            switch (Operator)
            {
                case "&&":
                case "||":
                    return true;
                default:
                    return false;
            }
        }

        public static bool OperatorReturnsBool(string Operator)
        {
            switch (Operator)
            {
                case "==":
                case "!=":
                case "<":
                case "<=":
                case ">":
                case ">=":
                case "&&":
                case "||":
                    return true;
                default:
                    return false;
            }
        }

        public Type ResultType => OperatorReturnsBool(Operator) ? typeof(bool) : LeftNode.Type;

        protected override Type UncachedType => ResultType;

        public override Dictionary<string, string> Info => new Dictionary<string, string>()
        {
            {"Operator", Operator},
        };
    }
}