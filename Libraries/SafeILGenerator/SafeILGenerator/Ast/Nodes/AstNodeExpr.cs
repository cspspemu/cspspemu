using System;

namespace SafeILGenerator.Ast.Nodes
{
    public abstract class AstNodeExpr : AstNode
    {
        public virtual Type Type => UncachedType;
        protected abstract Type UncachedType { get; }

        public static AstNodeExprBinop operator +(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "+", right);
        }

        public static AstNodeExprBinop operator -(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "-", right);
        }

        public static AstNodeExprBinop operator *(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "*", right);
        }

        public static AstNodeExprBinop operator /(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "/", right);
        }

        public static AstNodeExprBinop operator %(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "%", right);
        }

        public static AstNodeExprBinop operator &(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "&", right);
        }

        public static AstNodeExprBinop operator |(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "|", right);
        }

        public static AstNodeExprBinop operator ^(AstNodeExpr left, AstNodeExpr right)
        {
            return new AstNodeExprBinop(left, "^", right);
        }

        //public static AstNodeExprBinop operator <<(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "<<", Right); }
        //public static AstNodeExprBinop operator >>(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, ">>", Right); }

        public static AstNodeExprUnop operator +(AstNodeExpr right)
        {
            return new AstNodeExprUnop("+", right);
        }

        public static AstNodeExprUnop operator -(AstNodeExpr right)
        {
            return new AstNodeExprUnop("-", right);
        }

        public static AstNodeExprUnop operator ~(AstNodeExpr right)
        {
            return new AstNodeExprUnop("~", right);
        }

        public static implicit operator AstNodeExpr(sbyte value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(byte value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(short value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(ushort value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(int value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(uint value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(long value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(ulong value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(bool value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(float value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(double value)
        {
            return new AstNodeExprImm(value);
        }

        public static implicit operator AstNodeExpr(string value)
        {
            return new AstNodeExprImm(value);
        }

        //public static AstNodeExprBinop operator ==(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "==", Right); }
        //public static AstNodeExprBinop operator !=(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "!=", Right); }
        //
        //public static AstNodeExprBinop operator >(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, ">", Right); }
        //public static AstNodeExprBinop operator <(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "<", Right); }
        //public static AstNodeExprBinop operator >=(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, ">=", Right); }
        //public static AstNodeExprBinop operator <=(AstNodeExpr Left, AstNodeExpr Right) { return new AstNodeExprBinop(Left, "<=", Right); }
    }
}