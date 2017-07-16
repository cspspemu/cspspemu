using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		static public string GenerateString(AstNode AstNode)
		{
			return GenerateString<GeneratorCSharp>(AstNode);
		}

		static public string GenerateString<TGeneratorCSharp>(AstNode AstNode) where TGeneratorCSharp : GeneratorCSharp, new()
		{
			if (AstNode == null) return "";
			return new TGeneratorCSharp().Reset().GenerateRoot(AstNode).ToString();
		}

		protected virtual void _Generate(AstNodeExprLocal Local)
		{
			Output.Write(Local.AstLocal.Name);
		}

		protected virtual void _Generate(AstNodeExprNull Null)
		{
			Output.Write("null");
		}

		private String ValueAsString(object Value, Type Type = null)
		{
			if (Type == null) Type = (Value != null) ? Value.GetType() : typeof(Object);
			if (Value == null) return "null";
			
			if (Value is bool)
			{
				return Value.ToString().ToLower();
			}
			else if (Value is IntPtr)
			{
				return String.Format("0x{0:X}", ((IntPtr)Value).ToInt64());
			}
			else if (Value is string)
			{
				return String.Format("{0}", AstStringUtils.ToLiteral(Value as string));
			}
			else if (!AstUtils.IsTypeSigned(Type))
			{
				//StringValue = String.Format("0x{0:X8}", ItemValue);
				if (Convert.ToInt64(Value) > 9) return String.Format("0x{0:X}", Value);
			}

			return Value.ToString();
		}

		protected virtual void _Generate(AstNodeExprImm Item)
		{
			Output.Write(ValueAsString(Item.Value, Item.Type));
		}

		protected virtual void _Generate(AstNodeStmComment Comment)
		{
			Output.Write("// " + Comment.CommentText);
			Output.WriteNewLine();
		}

		protected virtual void _Generate(AstNodeExprBinop Item)
		{
			Output.Write("(");
			this.Generate(Item.LeftNode);
			Output.Write(" " + Item.Operator + " ");
			this.Generate(Item.RightNode);
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
			Output.Indent(() =>
			{
				Generate(Case.Code);
			});
			Output.WriteNewLine();
			Output.Write("break;");
			Output.WriteNewLine();
		}

		protected virtual void _Generate(AstNodeCaseDefault Case)
		{
			Output.Write("default:");
			Output.WriteNewLine();
			Output.Indent(() =>
			{
				Generate(Case.Code);
			});
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

		protected virtual void _Generate(AstNodeStmLabel Label)
		{
			Output.UnIndent(() =>
			{
				Output.Write(Label.AstLabel.Name);
				Output.Write(":;");
			});
		}

		protected virtual void _Generate(AstNodeStmExpr Stat)
		{
			Generate(Stat.AstNodeExpr);
			Output.Write(";");
		}

		protected virtual void _Generate(AstNodeExprTerop Item)
		{
			Output.Write("(");
			this.Generate(Item.Cond);
			Output.Write(" ? ");
			this.Generate(Item.True);
			Output.Write(" : ");
			this.Generate(Item.False);
			Output.Write(")");
		}

		protected virtual void _Generate(AstNodeStmEmpty Empty)
		{
		}

		protected virtual void _Generate(AstNodeExprUnop Item)
		{
			Output.Write("(" + Item.Operator);
			this.Generate(Item.RightNode);
			Output.Write(")");
		}

		protected virtual void _Generate(AstNodeStmIfElse IfElse)
		{
			Output.Write("if (");
			this.Generate(IfElse.Condition);
			Output.Write(") ");
			this.Generate(IfElse.True);
			if (IfElse.False != null)
			{
				Output.Write(" else ");
				this.Generate(IfElse.False);
			}
		}

		protected virtual void _Generate(AstNodeStmReturn Return)
		{
			Output.Write("return");
			if (Return.Expression != null)
			{
				Output.Write(" ");
				this.Generate(Return.Expression);
			}
			Output.Write(";");
		}

		private Stack<AstNodeExpr> PlaceholderStack = new Stack<AstNodeExpr>();

		protected virtual void _Generate(AstNodeExprSetGetLValuePlaceholder Placeholder)
		{
			var AstExpr = PlaceholderStack.Pop();
			if (AstExpr.Type != Placeholder.Type) throw (new Exception("Invalid Expression for placeholder " + AstExpr.Type + " != " + Placeholder.Type + "."));
			Generate(AstExpr);
		}

		protected virtual void _Generate(AstNodeExprSetGetLValue SetGetLValue)
		{
			Generate(SetGetLValue.GetExpression);
		}

		protected virtual void _Generate(AstNodeStmAssign Assign)
		{
			var AstNodeExprSetGetLValue = Assign.LValue as AstNodeExprSetGetLValue;
			
			if (AstNodeExprSetGetLValue != null)
			{
				PlaceholderStack.Push(Assign.Value);
				Generate(AstNodeExprSetGetLValue.SetExpression);
				Output.Write(";");
			}
			else
			{
				Generate(Assign.LValue);
				Output.Write(" = ");
				Generate(Assign.Value);
				Output.Write(";");
			}
		}

		protected virtual void _Generate(AstNodeExprIndirect Assign)
		{
			Output.Write("*(");
			Generate(Assign.PointerExpression);
			Output.Write(")");
		}

		protected virtual void _Generate(AstNodeExprGetAddress GetAddress)
		{
			Output.Write("&");
			Generate(GetAddress.Expression);
		}

		protected virtual void _Generate(AstNodeExprPropertyAccess PropertyAccess)
		{
			Generate(PropertyAccess.Instance);
			Output.Write(".");
			Output.Write(PropertyAccess.Property.Name);
		}

		protected virtual void _Generate(AstNodeExprFieldAccess FieldAccess)
		{
			Generate(FieldAccess.Instance);
			Output.Write(".");
			Output.Write(FieldAccess.Field.Name);
		}

		protected virtual void _Generate(AstNodeExprStaticFieldAccess FieldAccess)
		{
			Output.Write(FieldAccess.Field.DeclaringType.Name);
			Output.Write(".");
			Output.Write(FieldAccess.Field.Name);
		}

		protected virtual void _Generate(AstNodeExprArrayAccess ArrayAccess)
		{
			Generate(ArrayAccess.ArrayInstance);
			Output.Write("[");
			Generate(ArrayAccess.Index);
			Output.Write("]");
		}

		protected virtual void _Generate(AstNodeExprArgument Argument)
		{
			Output.Write(Argument.AstArgument.Name);
		}

		protected virtual void _Generate(AstNodeExprCast Cast)
		{
			if (Cast.Explicit)
			{
				Output.Write("(");
				Output.Write("(" + Cast.CastedType.Name + ")");
				Generate(Cast.Expr);
				Output.Write(")");
			}
			else
			{
				Generate(Cast.Expr);
			}
		}

		protected virtual void _Generate(AstNodeExprCallTailCall Tail)
		{
			Output.Write("__tail_call(");
			Generate(Tail.Call);
			Output.Write(")");
		}

		private void GenerateCallParameters(AstNodeExpr[] Parameters)
		{
			Output.Write("(");
			for (int n = 0; n < Parameters.Length; n++)
			{
				if (n != 0) Output.Write(", ");
				Generate(Parameters[n]);
			}
			Output.Write(")");
		}

		protected virtual void _Generate(AstNodeExprCallStatic Call)
		{
			Output.Write(Call.MethodInfo.DeclaringType.Name + "." + Call.MethodInfo.Name);
			GenerateCallParameters(Call.Parameters);
		}

		protected virtual void _Generate(AstNodeExprCallInstance Call)
		{
			Generate(Call.Instance);
			Output.Write("." + Call.MethodInfo.Name);
			GenerateCallParameters(Call.Parameters);
		}

		protected virtual void _Generate(AstNodeExprCallDelegate Call)
		{
			Generate(Call.Instance);
			GenerateCallParameters(Call.Parameters);
		}

		protected virtual void _Generate(AstNodeStmContainer Container)
		{
			if (Container.Inline)
			{
				foreach (var Node in Container.Nodes)
				{
					Generate(Node);
					Output.Write(" ");
				}
				//Output.WriteNewLine();
			}
			else
			{
				Output.Write("{").WriteNewLine();
				Output.Indent(() =>
				{
					foreach (var Node in Container.Nodes)
					{
						Generate(Node);
						Output.WriteNewLine();
					}
				});
				Output.Write("}").WriteNewLine();
			}
		}

		protected virtual void _Generate(AstNodeExprNewArray Array)
		{
			Output.Write("new " + Array.ElementType + "[] { ");
			for (int n = 0; n < Array.Length; n++)
			{
				if (n != 0) Output.Write(", ");
				Generate(Array.Values[n]);
			}
			Output.Write(" }");
		}

		protected virtual void _Generate(AstNodeExprNew AstNodeExprNew)
		{
			var Params = AstNodeExprNew.Params;
			Output.Write("new " + AstNodeExprNew.Type + "(");
			for (int n = 0; n < Params.Length; n++)
			{
				if (n != 0) Output.Write(", ");
				Generate(Params[n]);
			}
			Output.Write(")");
		}

		protected virtual void _Generate(AstNodeStmThrow AstNodeStmThrow)
		{
			Output.Write("throw ");
			Generate(AstNodeStmThrow.AstNodeExpr);
			Output.Write(";");
		}

		public override string ToString()
		{
			return Output.ToString();
		}
	}
}
