using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SafeILGenerator.Ast
{
    public class AstGenerator
    {
        protected AstGenerator()
        {
        }

        public AstGenerator Unimplemented(string message)
        {
            Console.WriteLine(message);
            return this;
        }

        public static readonly AstGenerator Instance = new AstGenerator();

        public AstNodeStmComment Comment(string comment) => new AstNodeStmComment(comment);

        public AstNodeExprArgument Argument(Type type, int index, string name = null) =>
            new AstNodeExprArgument(new AstArgument(index, type, name));

        public AstNodeExprArgument Argument<T>(int index, string name = null) => Argument(typeof(T), index, name);
        public AstNodeExprArgument Argument(AstArgument astArgument) => new AstNodeExprArgument(astArgument);
        public AstNodeExprLocal Local(AstLocal astLocal) => new AstNodeExprLocal(astLocal);
        public AstNodeStmGoto GotoAlways(AstLabel astLabel) => new AstNodeStmGotoAlways(astLabel);

        public AstNodeStmGotoIfTrue GotoIfTrue(AstLabel astLabel, AstNodeExpr condition) =>
            new AstNodeStmGotoIfTrue(astLabel, condition);

        public AstNodeStmGotoIfFalse GotoIfFalse(AstLabel astLabel, AstNodeExpr condition) =>
            new AstNodeStmGotoIfFalse(astLabel, condition);

        public AstNodeStmLabel Label(AstLabel astLabel) => new AstNodeStmLabel(astLabel);

        public AstNodeExprFieldAccess FieldAccess(AstNodeExpr instance, FieldInfo fieldInfo, string fieldName = "") =>
            new AstNodeExprFieldAccess(instance, fieldInfo, fieldName);

        public AstNodeExprFieldAccess FieldAccess(AstNodeExpr instance, string fieldName) =>
            new AstNodeExprFieldAccess(instance, fieldName);

        public AstNodeExprLValue FieldPropertyAccess(AstNodeExpr instance, string fieldPropertyName)
        {
            if (instance.Type.GetField(fieldPropertyName) != null) return FieldAccess(instance, fieldPropertyName);
            if (instance.Type.GetProperty(fieldPropertyName) != null)
                return PropertyAccess(instance, fieldPropertyName);
            throw (new InvalidOperationException(String.Format("Can't find Field/Property '{0}' for type '{1}'",
                fieldPropertyName, instance.Type)));
        }

        public AstNodeExprPropertyAccess PropertyAccess(AstNodeExpr instance, PropertyInfo propertyInfo) =>
            new AstNodeExprPropertyAccess(instance, propertyInfo);

        public AstNodeExprPropertyAccess PropertyAccess(AstNodeExpr instance, string propertyName) =>
            new AstNodeExprPropertyAccess(instance, propertyName);

        public AstNodeExprArrayAccess ArrayAccess(AstNodeExpr instance, AstNodeExpr index) =>
            new AstNodeExprArrayAccess(instance, index);

        public AstNodeExprImm Immediate<TType>(TType value) => new AstNodeExprImm(value, typeof(TType));
        public AstNodeExprImm Immediate(object value) => new AstNodeExprImm(value);
        public AstNodeExprNull Null<TType>() => new AstNodeExprNull(typeof(TType));

        //public AstNodeExprStaticFieldAccess ImmediateObject<TType>(ILInstanceHolderPool<TType> Pool, TType Value)
        //{
        //	return new AstNodeExprImm(Value);
        //}

        public AstNodeExprCallTailCall TailCall(AstNodeExprCall call) => new AstNodeExprCallTailCall(call);

        public AstNodeExprCallStatic CallStatic(Delegate Delegate, params AstNodeExpr[] parameters) =>
            new AstNodeExprCallStatic(Delegate, parameters);

        public AstNodeExprCallStatic CallStatic(MethodInfo methodInfo, params AstNodeExpr[] parameters) =>
            new AstNodeExprCallStatic(methodInfo, parameters);

        public AstNodeExprCallInstance CallInstance(AstNodeExpr instance, Delegate Delegate,
            params AstNodeExpr[] parameters) => new AstNodeExprCallInstance(instance, Delegate, parameters);

        public AstNodeExprCallDelegate CallDelegate(AstNodeExpr instance, params AstNodeExpr[] parameters) =>
            new AstNodeExprCallDelegate(instance, parameters);

        public AstNodeExprCallInstance CallInstance(AstNodeExpr instance, MethodInfo methodInfo,
            params AstNodeExpr[] parameters) => new AstNodeExprCallInstance(instance, methodInfo, parameters);

        public AstNodeExprUnop Unary(string Operator, AstNodeExpr right) => new AstNodeExprUnop(Operator, right);

        public AstNodeExprBinop Binary(AstNodeExpr left, string Operator, AstNodeExpr right) =>
            new AstNodeExprBinop(left, Operator, right);

        public AstNodeExprTerop Ternary(AstNodeExpr condition, AstNodeExpr True, AstNodeExpr False) =>
            new AstNodeExprTerop(condition, True, False);

        public AstNodeStmIfElse If(AstNodeExpr condition, AstNodeStm True) => new AstNodeStmIfElse(condition, True);

        public AstNodeStmIfElse IfElse(AstNodeExpr condition, AstNodeStm True, AstNodeStm False) =>
            new AstNodeStmIfElse(condition, True, False);

        public AstNodeStmContainer Statements(IEnumerable<AstNodeStm> statements) =>
            new AstNodeStmContainer(statements.ToArray());

        public AstNodeStmContainer Statements(params AstNodeStm[] statements) => new AstNodeStmContainer(statements);

        public AstNodeStmContainer StatementsInline(IEnumerable<AstNodeStm> statements) =>
            new AstNodeStmContainer(true, statements.ToArray());

        public AstNodeStmContainer StatementsInline(params AstNodeStm[] statements) =>
            new AstNodeStmContainer(true, statements);

        public AstNodeStmEmpty Statement() => new AstNodeStmEmpty();
        public AstNodeStmExpr Statement(AstNodeExpr expr) => new AstNodeStmExpr(expr);
        public AstNodeStmReturn Return(AstNodeExpr expr = null) => new AstNodeStmReturn(expr);
        public AstNodeStmAssign Assign(AstNodeExprLValue left, AstNodeExpr expr) => new AstNodeStmAssign(left, expr);

        public AstNodeExprCast Cast(Type type, AstNodeExpr expr, bool Explicit = true) =>
            new AstNodeExprCast(type, expr, Explicit);

        public AstNodeExprCast Cast<T>(AstNodeExpr expr, bool Explicit = true) => Cast(typeof(T), expr, Explicit);
        public AstNodeExprGetAddress GetAddress(AstNodeExprLValue expr) => new AstNodeExprGetAddress(expr);
        public AstNodeExprLValue Reinterpret<TType>(AstNodeExprLValue value) => Reinterpret(typeof(TType), value);

        public AstNodeExprLValue Reinterpret(Type type, AstNodeExprLValue value) =>
            Indirect(Cast(type.MakePointerType(), GetAddress(value), Explicit: false));

        public AstNodeExprIndirect Indirect(AstNodeExpr pointerExpr) => new AstNodeExprIndirect(pointerExpr);
        public AstNodeStm DebugWrite(string text) => Statement(CallStatic((Action<string>) Console.WriteLine, text));
        public AstNodeExprLValue StaticFieldAccess(FieldInfo fieldInfo) => new AstNodeExprStaticFieldAccess(fieldInfo);

        public AstNodeExprLValue StaticFieldAccess<T>(Expression<Func<T>> expression) =>
            StaticFieldAccess(_fieldof(expression));

        private static FieldInfo _fieldof<T>(Expression<Func<T>> expression) =>
            (FieldInfo) ((MemberExpression) expression.Body).Member;

        public AstNodeCase Case(object value, AstNodeStm code) => new AstNodeCase(value, code);
        public AstNodeCaseDefault Default(AstNodeStm code) => new AstNodeCaseDefault(code);

        public AstNodeStmSwitch Switch(AstNodeExpr valueToCheck, params AstNodeCase[] cases) =>
            new AstNodeStmSwitch(valueToCheck, cases);

        public AstNodeStmSwitch
            Switch(AstNodeExpr valueToCheck, AstNodeCaseDefault Default, params AstNodeCase[] cases) =>
            new AstNodeStmSwitch(valueToCheck, cases, Default);

        public AstNodeExprNewArray NewArray(Type type, params AstNodeExpr[] elements) =>
            new AstNodeExprNewArray(type, elements);

        public AstNodeExprNewArray NewArray<TType>(params AstNodeExpr[] elements) => NewArray(typeof(TType), elements);
        public AstNodeExprNew New(Type type, params AstNodeExpr[] Params) => new AstNodeExprNew(type, Params);
        public AstNodeExprNew New<TType>(params AstNodeExpr[] Params) => New(typeof(TType), Params);

        public AstNodeExprSetGetLValuePlaceholder SetGetLValuePlaceholder<TType>() =>
            SetGetLValuePlaceholder(typeof(TType));

        public AstNodeExprSetGetLValuePlaceholder SetGetLValuePlaceholder(Type type) =>
            new AstNodeExprSetGetLValuePlaceholder(type);

        public AstNodeExprSetGetLValue SetGetLValue(AstNodeExpr setExpression, AstNodeExpr getExpression) =>
            new AstNodeExprSetGetLValue(setExpression, getExpression);

        public AstNodeStm Throw(AstNodeExpr expression) => new AstNodeStmThrow(expression);
    }
}