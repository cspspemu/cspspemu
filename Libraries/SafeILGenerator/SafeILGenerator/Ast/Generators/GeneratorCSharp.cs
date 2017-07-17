using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Utils;
using System;
using System.Collections.Generic;

namespace SafeILGenerator.Ast.Generators
{
    public class GeneratorCSharp : Generator<GeneratorCSharp>
    {
        protected IndentedStringBuilder Output;

        public override GeneratorCSharp Reset()
        {
            Output = new IndentedStringBuilder();
            return this;
        }

        public static string GenerateString(AstNode astNode)
        {
            return GenerateString<GeneratorCSharp>(astNode);
        }

        public static string GenerateString<TGeneratorCSharp>(AstNode astNode)
            where TGeneratorCSharp : GeneratorCSharp, new()
        {
            if (astNode == null) return "";
            return new TGeneratorCSharp().Reset().GenerateRoot(astNode).ToString();
        }

        protected virtual void _Generate(AstNodeExprLocal local)
        {
            Output.Write(local.AstLocal.Name);
        }

        protected virtual void _Generate(AstNodeExprNull Null)
        {
            Output.Write("null");
        }

        private static string ValueAsString(object value, Type type = null)
        {
            if (type == null) type = value?.GetType() ?? typeof(Object);
            if (value == null) return "null";
            if (value is bool) return value.ToString().ToLower();
            if (value is IntPtr) return $"0x{((IntPtr) value).ToInt64():X}";
            if (value is string) return $"{AstStringUtils.ToLiteral(value as string)}";
            if (AstUtils.IsTypeSigned(type)) return value.ToString();
            if (Convert.ToInt64(value) > 9) return String.Format("0x{0:X}", value);
            return value.ToString();
        }

        protected virtual void _Generate(AstNodeExprImm item)
        {
            Output.Write(ValueAsString(item.Value, item.Type));
        }

        protected virtual void _Generate(AstNodeStmComment comment)
        {
            Output.Write("// " + comment.CommentText);
            Output.WriteNewLine();
        }

        protected virtual void _Generate(AstNodeExprBinop item)
        {
            Output.Write("(");
            Generate(item.LeftNode);
            Output.Write(" " + item.Operator + " ");
            Generate(item.RightNode);
            Output.Write(")");
        }

        protected virtual void _Generate(AstNodeStmGotoIfTrue Goto)
        {
            Output.Write("if (");
            Generate(Goto.Condition);
            Output.Write(") ");
            Output.Write("goto ");
            Output.Write(Goto.AstLabel.Name);
            Output.Write(";");
        }

        protected virtual void _Generate(AstNodeCase Case)
        {
            Output.Write("case ");
            //Output.Write(String.Join(", ", Case.CaseValues.Select(Item => ValueAsString(Item))));
            Output.Write(String.Join(", ", ValueAsString(Case.CaseValue)));
            Output.Write(":");
            Output.WriteNewLine();
            Output.Indent(() => { Generate(Case.Code); });
            Output.WriteNewLine();
            Output.Write("break;");
            Output.WriteNewLine();
        }

        protected virtual void _Generate(AstNodeCaseDefault Case)
        {
            Output.Write("default:");
            Output.WriteNewLine();
            Output.Indent(() => { Generate(Case.Code); });
            Output.WriteNewLine();
            Output.Write("break;");
            Output.WriteNewLine();
        }

        protected virtual void _Generate(AstNodeStmSwitch Switch)
        {
            Output.Write("switch (");
            Generate(Switch.SwitchValue);
            Output.Write(") {").WriteNewLine();
            Output.Indent(() =>
            {
                foreach (var Case in Switch.Cases) Generate(Case);
                if (Switch.CaseDefault != null) Generate(Switch.CaseDefault);
            });
            Output.Write("}");
        }

        protected virtual void _Generate(AstNodeStmGotoIfFalse Goto)
        {
            Output.Write("if (!(");
            Generate(Goto.Condition);
            Output.Write(")) ");
            Output.Write("goto ");
            Output.Write(Goto.AstLabel.Name);
            Output.Write(";");
        }

        protected virtual void _Generate(AstNodeStmGotoAlways Goto)
        {
            Output.Write("goto ");
            Output.Write(Goto.AstLabel.Name);
            Output.Write(";");
        }

        protected virtual void _Generate(AstNodeStmLabel label)
        {
            Output.UnIndent(() =>
            {
                Output.Write(label.AstLabel.Name);
                Output.Write(":;");
            });
        }

        protected virtual void _Generate(AstNodeStmExpr stat)
        {
            Generate(stat.AstNodeExpr);
            Output.Write(";");
        }

        protected virtual void _Generate(AstNodeExprTerop item)
        {
            Output.Write("(");
            Generate(item.Cond);
            Output.Write(" ? ");
            Generate(item.True);
            Output.Write(" : ");
            Generate(item.False);
            Output.Write(")");
        }

        protected virtual void _Generate(AstNodeStmEmpty empty)
        {
        }

        protected virtual void _Generate(AstNodeExprUnop item)
        {
            Output.Write("(" + item.Operator);
            Generate(item.RightNode);
            Output.Write(")");
        }

        protected virtual void _Generate(AstNodeStmIfElse ifElse)
        {
            Output.Write("if (");
            Generate(ifElse.Condition);
            Output.Write(") ");
            Generate(ifElse.True);
            if (ifElse.False != null)
            {
                Output.Write(" else ");
                Generate(ifElse.False);
            }
        }

        protected virtual void _Generate(AstNodeStmReturn Return)
        {
            Output.Write("return");
            if (Return.Expression != null)
            {
                Output.Write(" ");
                Generate(Return.Expression);
            }
            Output.Write(";");
        }

        private Stack<AstNodeExpr> PlaceholderStack = new Stack<AstNodeExpr>();

        protected virtual void _Generate(AstNodeExprSetGetLValuePlaceholder placeholder)
        {
            var astExpr = PlaceholderStack.Pop();
            if (astExpr.Type != placeholder.Type)
                throw (new Exception("Invalid Expression for placeholder " + astExpr.Type + " != " + placeholder.Type +
                                     "."));
            Generate(astExpr);
        }

        protected virtual void _Generate(AstNodeExprSetGetLValue setGetLValue)
        {
            Generate(setGetLValue.GetExpression);
        }

        protected virtual void _Generate(AstNodeStmAssign assign)
        {
            var astNodeExprSetGetLValue = assign.LeftValue as AstNodeExprSetGetLValue;

            if (astNodeExprSetGetLValue != null)
            {
                PlaceholderStack.Push(assign.Value);
                Generate(astNodeExprSetGetLValue.SetExpression);
                Output.Write(";");
            }
            else
            {
                Generate(assign.LeftValue);
                Output.Write(" = ");
                Generate(assign.Value);
                Output.Write(";");
            }
        }

        protected virtual void _Generate(AstNodeExprIndirect assign)
        {
            Output.Write("*(");
            Generate(assign.PointerExpression);
            Output.Write(")");
        }

        protected virtual void _Generate(AstNodeExprGetAddress getAddress)
        {
            Output.Write("&");
            Generate(getAddress.Expression);
        }

        protected virtual void _Generate(AstNodeExprPropertyAccess propertyAccess)
        {
            Generate(propertyAccess.Instance);
            Output.Write(".");
            Output.Write(propertyAccess.Property.Name);
        }

        protected virtual void _Generate(AstNodeExprFieldAccess fieldAccess)
        {
            Generate(fieldAccess.Instance);
            Output.Write(".");
            Output.Write(fieldAccess.Field.Name);
        }

        protected virtual void _Generate(AstNodeExprStaticFieldAccess fieldAccess)
        {
            Output.Write(fieldAccess.Field.DeclaringType?.Name);
            Output.Write(".");
            Output.Write(fieldAccess.Field.Name);
        }

        protected virtual void _Generate(AstNodeExprArrayAccess arrayAccess)
        {
            Generate(arrayAccess.ArrayInstance);
            Output.Write("[");
            Generate(arrayAccess.Index);
            Output.Write("]");
        }

        protected virtual void _Generate(AstNodeExprArgument argument)
        {
            Output.Write(argument.AstArgument.Name);
        }

        protected virtual void _Generate(AstNodeExprCast cast)
        {
            if (cast.Explicit)
            {
                Output.Write("(");
                Output.Write("(" + cast.CastedType.Name + ")");
                Generate(cast.Expr);
                Output.Write(")");
            }
            else
            {
                Generate(cast.Expr);
            }
        }

        protected virtual void _Generate(AstNodeExprCallTailCall tail)
        {
            Output.Write("__tail_call(");
            Generate(tail.Call);
            Output.Write(")");
        }

        private void GenerateCallParameters(AstNodeExpr[] parameters)
        {
            Output.Write("(");
            for (int n = 0; n < parameters.Length; n++)
            {
                if (n != 0) Output.Write(", ");
                Generate(parameters[n]);
            }
            Output.Write(")");
        }

        protected virtual void _Generate(AstNodeExprCallStatic call)
        {
            Output.Write(call.MethodInfo.DeclaringType?.Name + "." + call.MethodInfo.Name);
            GenerateCallParameters(call.Parameters);
        }

        protected virtual void _Generate(AstNodeExprCallInstance call)
        {
            Generate(call.Instance);
            Output.Write("." + call.MethodInfo.Name);
            GenerateCallParameters(call.Parameters);
        }

        protected virtual void _Generate(AstNodeExprCallDelegate call)
        {
            Generate(call.Instance);
            GenerateCallParameters(call.Parameters);
        }

        protected virtual void _Generate(AstNodeStmContainer container)
        {
            if (container.Inline)
            {
                foreach (var node in container.Nodes)
                {
                    Generate(node);
                    Output.Write(" ");
                }
                //Output.WriteNewLine();
            }
            else
            {
                Output.Write("{").WriteNewLine();
                Output.Indent(() =>
                {
                    foreach (var node in container.Nodes)
                    {
                        Generate(node);
                        Output.WriteNewLine();
                    }
                });
                Output.Write("}").WriteNewLine();
            }
        }

        protected virtual void _Generate(AstNodeExprNewArray array)
        {
            Output.Write("new " + array.ElementType + "[] { ");
            for (int n = 0; n < array.Length; n++)
            {
                if (n != 0) Output.Write(", ");
                Generate(array.Values[n]);
            }
            Output.Write(" }");
        }

        protected virtual void _Generate(AstNodeExprNew astNodeExprNew)
        {
            var Params = astNodeExprNew.Params;
            Output.Write("new " + astNodeExprNew.Type + "(");
            for (int n = 0; n < Params.Length; n++)
            {
                if (n != 0) Output.Write(", ");
                Generate(Params[n]);
            }
            Output.Write(")");
        }

        protected virtual void _Generate(AstNodeStmThrow astNodeStmThrow)
        {
            Output.Write("throw ");
            Generate(astNodeStmThrow.AstNodeExpr);
            Output.Write(";");
        }

        public override string ToString()
        {
            return Output.ToString();
        }
    }
}