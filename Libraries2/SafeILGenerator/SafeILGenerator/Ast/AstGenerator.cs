using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast
{
	public class AstGenerator
	{
		protected AstGenerator()
		{
		}

		static public readonly AstGenerator Instance = new AstGenerator();

		public AstNodeStmComment Comment(string Comment)
		{
			return new AstNodeStmComment(Comment);
		}

		public AstNodeExprArgument Argument(Type Type, int Index, string Name = null)
		{
			return new AstNodeExprArgument(new AstArgument(Index, Type, Name));
		}

		public AstNodeExprArgument Argument<T>(int Index, string Name = null)
		{
			return Argument(typeof(T), Index, Name);
		}

		public AstNodeExprArgument Argument(AstArgument AstArgument)
		{
			return new AstNodeExprArgument(AstArgument);
		}

		public AstNodeExprLocal Local(AstLocal AstLocal)
		{
			return new AstNodeExprLocal(AstLocal);
		}

		public AstNodeStmGoto GotoAlways(AstLabel AstLabel)
		{
			return new AstNodeStmGotoAlways(AstLabel);
		}

		public AstNodeStmGotoIfTrue GotoIfTrue(AstLabel AstLabel, AstNodeExpr Condition)
		{
			return new AstNodeStmGotoIfTrue(AstLabel, Condition);
		}

		public AstNodeStmGotoIfFalse GotoIfFalse(AstLabel AstLabel, AstNodeExpr Condition)
		{
			return new AstNodeStmGotoIfFalse(AstLabel, Condition);
		}

		public AstNodeStmLabel Label(AstLabel AstLabel)
		{
			return new AstNodeStmLabel(AstLabel);
		}

		public AstNodeExprFieldAccess FieldAccess(AstNodeExpr Instance, FieldInfo FieldInfo, string FieldName = "")
		{
			return new AstNodeExprFieldAccess(Instance, FieldInfo, FieldName);
		}

		public AstNodeExprFieldAccess FieldAccess(AstNodeExpr Instance, string FieldName)
		{
			return new AstNodeExprFieldAccess(Instance, FieldName);
		}

		public AstNodeExprLValue FieldPropertyAccess(AstNodeExpr Instance, string FieldPropertyName)
		{
			if (Instance.Type.GetField(FieldPropertyName) != null) return FieldAccess(Instance, FieldPropertyName);
			if (Instance.Type.GetProperty(FieldPropertyName) != null) return PropertyAccess(Instance, FieldPropertyName);
			throw (new InvalidOperationException(String.Format("Can't find Field/Property '{0}' for type '{1}'", FieldPropertyName, Instance.Type)));
		}

		public AstNodeExprPropertyAccess PropertyAccess(AstNodeExpr Instance, PropertyInfo PropertyInfo)
		{
			return new AstNodeExprPropertyAccess(Instance, PropertyInfo);
		}

		public AstNodeExprPropertyAccess PropertyAccess(AstNodeExpr Instance, string PropertyName)
		{
			return new AstNodeExprPropertyAccess(Instance, PropertyName);
		}

		public AstNodeExprArrayAccess ArrayAccess(AstNodeExpr Instance, AstNodeExpr Index)
		{
			return new AstNodeExprArrayAccess(Instance, Index);
		}

		public AstNodeExprImm Immediate<TType>(TType Value)
		{
			return new AstNodeExprImm(Value, typeof(TType));
		}

		public AstNodeExprImm Immediate(object Value)
		{
			return new AstNodeExprImm(Value);
		}

		public AstNodeExprNull Null<TType>()
		{
			return new AstNodeExprNull(typeof(TType));
		}

		//public AstNodeExprStaticFieldAccess ImmediateObject<TType>(ILInstanceHolderPool<TType> Pool, TType Value)
		//{
		//	return new AstNodeExprImm(Value);
		//}

		public AstNodeExprCallTailCall TailCall(AstNodeExprCall Call)
		{
			return new AstNodeExprCallTailCall(Call);
		}

		public AstNodeExprCallStatic CallStatic(Delegate Delegate, params AstNodeExpr[] Parameters)
		{
			return new AstNodeExprCallStatic(Delegate, Parameters);
		}

		public AstNodeExprCallStatic CallStatic(MethodInfo MethodInfo, params AstNodeExpr[] Parameters)
		{
			return new AstNodeExprCallStatic(MethodInfo, Parameters);
		}

		public AstNodeExprCallInstance CallInstance(AstNodeExpr Instance, Delegate Delegate, params AstNodeExpr[] Parameters)
		{
			return new AstNodeExprCallInstance(Instance, Delegate, Parameters);
		}

		public AstNodeExprCallDelegate CallDelegate(AstNodeExpr Instance, params AstNodeExpr[] Parameters)
		{
			return new AstNodeExprCallDelegate(Instance, Parameters);
		}

		public AstNodeExprCallInstance CallInstance(AstNodeExpr Instance, MethodInfo MethodInfo, params AstNodeExpr[] Parameters)
		{
			return new AstNodeExprCallInstance(Instance, MethodInfo, Parameters);
		}

		public AstNodeExprUnop Unary(string Operator, AstNodeExpr Right)
		{
			return new AstNodeExprUnop(Operator, Right);
		}

		public AstNodeExprBinop Binary(AstNodeExpr Left, string Operator, AstNodeExpr Right)
		{
			return new AstNodeExprBinop(Left, Operator, Right);
		}

		public AstNodeExprTerop Ternary(AstNodeExpr Condition, AstNodeExpr True, AstNodeExpr False)
		{
			return new AstNodeExprTerop(Condition, True, False);
		}

		public AstNodeStmIfElse If(AstNodeExpr Condition, AstNodeStm True)
		{
			return new AstNodeStmIfElse(Condition, True);
		}

		public AstNodeStmIfElse IfElse(AstNodeExpr Condition, AstNodeStm True, AstNodeStm False)
		{
			return new AstNodeStmIfElse(Condition, True, False);
		}

		public AstNodeStmContainer Statements(IEnumerable<AstNodeStm> Statements)
		{
			return new AstNodeStmContainer(Statements.ToArray());
		}

		public AstNodeStmContainer Statements(params AstNodeStm[] Statements)
		{
			return new AstNodeStmContainer(Statements);
		}

		public AstNodeStmContainer StatementsInline(IEnumerable<AstNodeStm> Statements)
		{
			return new AstNodeStmContainer(true, Statements.ToArray());
		}

		public AstNodeStmContainer StatementsInline(params AstNodeStm[] Statements)
		{
			return new AstNodeStmContainer(true, Statements);
		}

		public AstNodeStmEmpty Statement()
		{
			return new AstNodeStmEmpty();
		}

		public AstNodeStmExpr Statement(AstNodeExpr Expr)
		{
			return new AstNodeStmExpr(Expr);
		}

		public AstNodeStmReturn Return(AstNodeExpr Expr = null)
		{
			return new AstNodeStmReturn(Expr);
		}

		public AstNodeStmAssign Assign(AstNodeExprLValue Left, AstNodeExpr Expr)
		{
			return new AstNodeStmAssign(Left, Expr);
		}

		public AstNodeExprCast Cast(Type Type, AstNodeExpr Expr, bool Explicit = true)
		{
			return new AstNodeExprCast(Type, Expr, Explicit);
		}

		public AstNodeExprCast Cast<T>(AstNodeExpr Expr, bool Explicit = true)
		{
			return Cast(typeof(T), Expr, Explicit);
		}

		public AstNodeExprGetAddress GetAddress(AstNodeExprLValue Expr)
		{
			return new AstNodeExprGetAddress(Expr);
		}

		public AstNodeExprLValue Reinterpret<TType>(AstNodeExprLValue Value)
		{
			return Reinterpret(typeof(TType), Value);
		}

		public AstNodeExprLValue Reinterpret(Type Type, AstNodeExprLValue Value)
		{
			return Indirect(Cast(Type.MakePointerType(), GetAddress(Value), Explicit: false));
		}

		public AstNodeExprIndirect Indirect(AstNodeExpr PointerExpr)
		{
			return new AstNodeExprIndirect(PointerExpr);
		}

		public AstNodeStm DebugWrite(string Text)
		{
			return Statement(CallStatic((Action<string>)Console.WriteLine, Text));
		}

		public AstNodeExprLValue StaticFieldAccess(FieldInfo FieldInfo)
		{
			return new AstNodeExprStaticFieldAccess(FieldInfo);
		}

		public AstNodeExprLValue StaticFieldAccess<T>(Expression<Func<T>> Expression)
		{
			return StaticFieldAccess(_fieldof(Expression));
		}

		private static FieldInfo _fieldof<T>(Expression<Func<T>> expression)
		{
			MemberExpression body = (MemberExpression)expression.Body;
			return (FieldInfo)body.Member;
		}

		public AstNodeCase Case(object Value, AstNodeStm Code)
		{
			return new AstNodeCase(Value, Code);
		}

		public AstNodeCaseDefault Default(AstNodeStm Code)
		{
			return new AstNodeCaseDefault(Code);
		}

		public AstNodeStmSwitch Switch(AstNodeExpr ValueToCheck, params AstNodeCase[] Cases)
		{
			return new AstNodeStmSwitch(ValueToCheck, Cases);
		}

		public AstNodeStmSwitch Switch(AstNodeExpr ValueToCheck, AstNodeCaseDefault Default, params AstNodeCase[] Cases)
		{
			return new AstNodeStmSwitch(ValueToCheck, Cases, Default);
		}

		public AstNodeExprNewArray NewArray(Type Type, params AstNodeExpr[] Elements)
		{
			return new AstNodeExprNewArray(Type, Elements);
		}

		public AstNodeExprNewArray NewArray<TType>(params AstNodeExpr[] Elements)
		{
			return NewArray(typeof(TType), Elements);
		}

		public AstNodeExprNew New(Type Type, params AstNodeExpr[] Params)
		{
			return new AstNodeExprNew(Type, Params);
		}

		public AstNodeExprNew New<TType>(params AstNodeExpr[] Params)
		{
			return New(typeof(TType), Params);
		}

		public AstNodeExprSetGetLValuePlaceholder SetGetLValuePlaceholder<TType>()
		{
			return SetGetLValuePlaceholder(typeof(TType));
		}

		public AstNodeExprSetGetLValuePlaceholder SetGetLValuePlaceholder(Type Type)
		{
			return new AstNodeExprSetGetLValuePlaceholder(Type);
		}

		public AstNodeExprSetGetLValue SetGetLValue(AstNodeExpr SetExpression, AstNodeExpr GetExpression)
		{
			return new AstNodeExprSetGetLValue(SetExpression, GetExpression);
		}

		public AstNodeStm Throw(AstNodeExpr Expression)
		{
			return new AstNodeStmThrow(Expression);
		}
	}
}
