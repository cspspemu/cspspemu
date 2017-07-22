using System;

namespace SafeILGenerator.Ast.Nodes
{
    public abstract class AstNodeExpr : AstNode
    {
        public virtual Type Type => UncachedType;
        protected abstract Type UncachedType { get; }

        public static AstNodeExprBinop operator +(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "+", right);

        public static AstNodeExprBinop operator -(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "-", right);

        public static AstNodeExprBinop operator *(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "*", right);

        public static AstNodeExprBinop operator /(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "/", right);

        public static AstNodeExprBinop operator %(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "%", right);

        public static AstNodeExprBinop operator &(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "&", right);

        public static AstNodeExprBinop operator |(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "|", right);

        public static AstNodeExprBinop operator ^(AstNodeExpr left, AstNodeExpr right) =>
            new AstNodeExprBinop(left, "^", right);

        //public static AstNodeExprBinop operator <<(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "<<", Right); }
        //public static AstNodeExprBinop operator >>(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, ">>", Right); }

        public static AstNodeExprUnop operator +(AstNodeExpr right) => new AstNodeExprUnop("+", right);
        public static AstNodeExprUnop operator -(AstNodeExpr right) => new AstNodeExprUnop("-", right);
        public static AstNodeExprUnop operator ~(AstNodeExpr right) => new AstNodeExprUnop("~", right);
        public static implicit operator AstNodeExpr(sbyte value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(byte value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(short value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(ushort value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(int value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(uint value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(long value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(ulong value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(bool value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(float value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(double value) => new AstNodeExprImm(value);
        public static implicit operator AstNodeExpr(string value) => new AstNodeExprImm(value);

        //public static AstNodeExprBinop operator ==(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "==", Right); }
        //public static AstNodeExprBinop operator !=(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "!=", Right); }
        //
        //public static AstNodeExprBinop operator >(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, ">", Right); }
        //public static AstNodeExprBinop operator <(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "<", Right); }
        //public static AstNodeExprBinop operator >=(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, ">=", Right); }
        //public static AstNodeExprBinop operator <=(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "<=", Right); }
    }
}