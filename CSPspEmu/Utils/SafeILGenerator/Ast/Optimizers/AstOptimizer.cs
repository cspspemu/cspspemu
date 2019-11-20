using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SafeILGenerator.Ast.Optimizers
{
    public class AstOptimizer
    {
        private readonly Dictionary<Type, MethodInfo> _generateMappings = new Dictionary<Type, MethodInfo>();

        public AstOptimizer()
        {
            foreach (var method in GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(method => method.ReturnType == typeof(AstNode))
                .Where(method => method.GetParameters().Length == 1)
            )
            {
                _generateMappings[method.GetParameters().First().ParameterType] = method;
            }
        }

        public AstNode Optimize(AstNode astNode)
        {
            //if (AstNode != null)
            {
                //Console.WriteLine("Optimize.AstNode: {0}", AstNode);
                astNode.TransformNodes(Optimize);

                var astNodeType = astNode.GetType();

                if (_generateMappings.ContainsKey(astNodeType))
                {
                    astNode = (AstNode) _generateMappings[astNodeType].Invoke(this, new object[] {astNode});
                }
                else
                {
                    //throw(new NotImplementedException(String.Format("Don't know how to optimize {0}", AstNodeType)));
                }
            }

            return astNode;
        }

        protected virtual AstNode _Optimize(AstNodeStmContainer container)
        {
            if (container.Nodes.Count == 1) return container.Nodes[0];

            var newContainer = new AstNodeStmContainer(container.Inline);

            foreach (var node in container.Nodes)
            {
                if (node == null) continue;

                if (node is AstNodeStmContainer)
                {
                    foreach (var node2 in (node as AstNodeStmContainer).Nodes)
                    {
                        if (!(node2 is AstNodeStmEmpty))
                        {
                            newContainer.AddStatement(node2);
                        }
                    }
                }
                else
                {
                    if (!(node is AstNodeStmEmpty))
                    {
                        newContainer.AddStatement(node);
                    }
                }
            }

            var rebuild = false;
            for (var n = 0; n < newContainer.Nodes.Count - 1; n++)
            {
                var currentNode = newContainer.Nodes[n];
                var nextNode = newContainer.Nodes[n + 1];
                if (!(currentNode is AstNodeStmGotoAlways) || !(nextNode is AstNodeStmLabel)) continue;
                if ((currentNode as AstNodeStmGotoAlways).AstLabel != (nextNode as AstNodeStmLabel).AstLabel) continue;
                newContainer.Nodes[n] = null;
                //NewContainer.Nodes[n + 1] = null;
                rebuild = true;
            }

            if (rebuild)
            {
                return new AstNodeStmContainer(container.Inline,
                    newContainer.Nodes.Where(node => node != null).ToArray());
            }
            else
            {
                return newContainer;
            }
        }

        protected virtual AstNode _Optimize(AstNodeExprCast cast)
        {
            //Console.WriteLine("Optimize.AstNodeExprCast: {0} : {1}", Cast.CastedType, Cast.Expr);

            // Dummy cast
            if (cast.CastedType == cast.Expr.Type)
            {
                //Console.WriteLine("Dummy Cast");
                return cast.Expr;
            }

            // Double Cast
            var expr = cast.Expr as AstNodeExprCast;
            if (expr != null)
            {
                //Console.WriteLine("Double Cast");
                var firstCastType = expr.CastedType;
                var secondCastType = cast.CastedType;
                if (firstCastType.IsPrimitive && secondCastType.IsPrimitive)
                {
                    if (AstUtils.GetTypeSize(firstCastType) >= AstUtils.GetTypeSize(secondCastType))
                    {
                        return Optimize(new AstNodeExprCast(cast.CastedType, expr.Expr));
                    }
                }

                return cast;
            }

            // Cast to immediate
            var imm = cast.Expr as AstNodeExprImm;
            if (imm != null)
            {
                //Console.WriteLine("Cast to immediate");
                return new AstNodeExprImm(AstUtils.CastType(imm.Value, cast.CastedType));
            }

            return cast;
        }

        protected virtual AstNode _Optimize(AstNodeExprImm immediate)
        {
            return immediate;
        }

        protected virtual AstNode _Optimize(AstNodeExprBinop binary)
        {
            //Console.WriteLine("Optimize.AstNodeExprBinop: {0} {1} {2}", Binary.LeftNode, Binary.Operator, Binary.RightNode);
            var leftImm = binary.LeftNode as AstNodeExprImm;
            var rightImm = binary.RightNode as AstNodeExprImm;
            var leftType = binary.LeftNode.Type;
            var rightType = binary.RightNode.Type;
            var Operator = binary.Operator;

            if (leftType == rightType)
            {
                if (AstUtils.IsTypeFloat(leftType))
                {
                    var type = leftType;

                    if (leftImm != null && rightImm != null)
                    {
                        var leftValue = Convert.ToDouble(leftImm.Value);
                        var rightValue = Convert.ToDouble(rightImm.Value);

                        switch (Operator)
                        {
                            case "+": return new AstNodeExprImm(AstUtils.CastType(leftValue + rightValue, type));
                            case "-": return new AstNodeExprImm(AstUtils.CastType(leftValue - rightValue, type));
                            case "*": return new AstNodeExprImm(AstUtils.CastType(leftValue * rightValue, type));
                            case "/": return new AstNodeExprImm(AstUtils.CastType(leftValue / rightValue, type));
                        }
                    }
                    else if (leftImm != null)
                    {
                        var leftValue = Convert.ToInt64(leftImm.Value);
                        switch (Operator)
                        {
                            case "|":
                                if (leftValue == 0) return binary.RightNode;
                                break;
                            case "+":
                                if (leftValue == 0) return binary.RightNode;
                                break;
                            case "-":
                                if (leftValue == 0) return new AstNodeExprUnop("-", binary.RightNode);
                                break;
                            case "*":
                                //if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
                                if (leftValue == 1) return binary.RightNode;
                                break;
                            case "/":
                                //if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
                                break;
                        }
                    }
                    else if (rightImm != null)
                    {
                        var rightValue = Convert.ToInt64(rightImm.Value);
                        switch (Operator)
                        {
                            case "|":
                                if (rightValue == 0) return binary.LeftNode;
                                break;
                            case "+":
                                if (rightValue == 0) return binary.LeftNode;
                                break;
                            case "-":
                                if (rightValue == 0) return binary.LeftNode;
                                break;
                            case "*":
                                if (rightValue == 1) return binary.LeftNode;
                                break;
                            case "/":
                                //if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
                                break;
                        }
                    }
                }
                else
                {
                    var unop = binary.RightNode as AstNodeExprUnop;
                    if (unop != null)
                    {
                        var rightUnary = unop;
                        if (Operator == "+" || Operator == "-")
                        {
                            if (rightUnary.Operator == "-")
                            {
                                return new AstNodeExprBinop(binary.LeftNode, Operator == "+" ? "-" : "+",
                                    rightUnary.RightNode);
                            }
                        }
                    }

                    var type = leftType;
                    // Can optimize just literal values.
                    if (leftImm != null && rightImm != null)
                    {
                        if (AstUtils.IsTypeSigned(leftType))
                        {
                            var leftValue = Convert.ToInt64(leftImm.Value);
                            var rightValue = Convert.ToInt64(rightImm.Value);

                            switch (Operator)
                            {
                                case "+": return new AstNodeExprImm(AstUtils.CastType(leftValue + rightValue, type));
                                case "-": return new AstNodeExprImm(AstUtils.CastType(leftValue - rightValue, type));
                                case "*": return new AstNodeExprImm(AstUtils.CastType(leftValue * rightValue, type));
                                case "/": return new AstNodeExprImm(AstUtils.CastType(leftValue / rightValue, type));
                                case "<<":
                                    return new AstNodeExprImm(AstUtils.CastType(leftValue << (int) rightValue, type));
                                case ">>":
                                    return new AstNodeExprImm(AstUtils.CastType(leftValue >> (int) rightValue, type));
                            }
                        }
                        else
                        {
                            var leftValue = Convert.ToUInt64(leftImm.Value);
                            var rightValue = Convert.ToUInt64(rightImm.Value);

                            // Optimize adding 0
                            switch (Operator)
                            {
                                case "+": return new AstNodeExprImm(AstUtils.CastType(leftValue + rightValue, type));
                                case "-": return new AstNodeExprImm(AstUtils.CastType(leftValue - rightValue, type));
                                case "*": return new AstNodeExprImm(AstUtils.CastType(leftValue * rightValue, type));
                                case "/": return new AstNodeExprImm(AstUtils.CastType(leftValue / rightValue, type));
                                case "<<":
                                    return new AstNodeExprImm(AstUtils.CastType(leftValue << (int) rightValue, type));
                                case ">>":
                                    return new AstNodeExprImm(AstUtils.CastType(leftValue >> (int) rightValue, type));
                            }
                        }
                    }
                    else if (leftImm != null)
                    {
                        var leftValue = Convert.ToInt64(leftImm.Value);
                        switch (Operator)
                        {
                            case "&":
                                if (leftValue == 0) return new AstNodeExprImm(0);
                                break;
                            case "|":
                                if (leftValue == 0) return binary.RightNode;
                                break;
                            case "+":
                                if (leftValue == 0) return binary.RightNode;
                                break;
                            case "-":
                                if (leftValue == 0) return new AstNodeExprUnop("-", binary.RightNode);
                                break;
                            case "*":
                                //if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
                                if (leftValue == 1) return binary.RightNode;
                                break;
                            case "/":
                                //if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
                                break;
                        }
                    }
                    else if (rightImm != null)
                    {
                        var rightValue = Convert.ToInt64(rightImm.Value);
                        switch (Operator)
                        {
                            case "0":
                                if (rightValue == 0) return new AstNodeExprImm(0);
                                break;
                            case "|":
                                if (rightValue == 0) return binary.LeftNode;
                                break;
                            case "+":
                                if (rightValue == 0) return binary.LeftNode;
                                if (rightValue < 0)
                                    return new AstNodeExprBinop(binary.LeftNode, "-",
                                        new AstNodeExprImm(AstUtils.Negate(rightImm.Value)));
                                break;
                            case "-":
                                if (rightValue == 0) return binary.LeftNode;
                                break;
                            case "*":
                                if (rightValue == 1) return binary.LeftNode;
                                break;
                            case "/":
                                //if (RightValue == 0) throw(new Exception("Can't divide by 0"));
                                if (rightValue == 1) return binary.LeftNode;
                                break;
                        }
                    }
                } // !AstUtils.IsTypeFloat(LeftType)
            }

            // Special optimizations
            if ((leftType == typeof(uint) || leftType == typeof(int)) && rightType == typeof(int) && rightImm != null)
            {
                var rightValue = Convert.ToInt64(rightImm.Value);
                if (Operator == ">>" && rightValue == 0) return binary.LeftNode;
            }

            return binary;
        }
    }
}