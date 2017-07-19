using System;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;
using SafeILGenerator.Ast;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        public enum ConditionEnum
        {
            VcFl = 0,
            VcEq = 1,
            VcLt = 2,
            VcLe = 3,
            VcTr = 4,
            VcNe = 5,
            VcGe = 6,
            VcGt = 7,
            VcEz = 8,
            VcEn = 9,
            VcEi = 10,
            VcEs = 11,
            VcNz = 12,
            VcNn = 13,
            VcNi = 14,
            VcNs = 15,
        }

        public AstNodeStm vcmp()
        {
            var vectorSize = Instruction.OneTwo;
            var cond = Instruction.Imm4;
            var cond2 = (ConditionEnum) cond;
            //bool NormalFlag = (Cond & 8) == 0;
            //bool NotFlag = (Cond & 4) != 0;
            //uint TypeFlag = (Cond & 3);

            var localCcTemp = ast.Local(AstLocal.Create<bool>());
            var localCcOr = ast.Local(AstLocal.Create<bool>());
            var localCcAnd = ast.Local(AstLocal.Create<bool>());

            return ast.Statements(
                ast.Assign(localCcOr, false),
                ast.Assign(localCcAnd, true),

                // TODO: CHECK THIS!
                ast.AssignVcc(0, true),
                ast.AssignVcc(1, true),
                ast.AssignVcc(2, true),
                ast.AssignVcc(3, true),
                _List(vectorSize, index =>
                {
                    AstNodeExpr expr;
                    //bool UsedForAggregate;

                    var left = VEC_VS[index];
                    var right = VEC_VT[index];
                    switch (cond2)
                    {
                        case ConditionEnum.VcFl:
                            expr = ast.Immediate(false);
                            break;
                        case ConditionEnum.VcEq:
                            expr = ast.Binary(left, "==", right);
                            break;
                        case ConditionEnum.VcLt:
                            expr = ast.Binary(left, "<", right);
                            break;
                        //case ConditionEnum.VC_LE: Expr = ast.Binary(Left, "<=", Right); break;
                        case ConditionEnum.VcLe:
                            expr = ast.CallStatic((Func<float, float, bool>) MathFloat.IsLessOrEqualsThan, left, right);
                            break;

                        case ConditionEnum.VcTr:
                            expr = ast.Immediate(true);
                            break;
                        case ConditionEnum.VcNe:
                            expr = ast.Binary(left, "!=", right);
                            break;
                        //case ConditionEnum.VC_GE: Expr = ast.Binary(Left, ">=", Right); break;
                        case ConditionEnum.VcGe:
                            expr = ast.CallStatic((Func<float, float, bool>) MathFloat.IsGreatOrEqualsThan, left,
                                right);
                            break;
                        case ConditionEnum.VcGt:
                            expr = ast.Binary(left, ">", right);
                            break;

                        case ConditionEnum.VcEz:
                            expr = ast.Binary(ast.Binary(left, "==", 0.0f), "||", ast.Binary(left, "==", -0.0f));
                            break;
                        case ConditionEnum.VcEn:
                            expr = ast.CallStatic((Func<float, bool>) MathFloat.IsNan, left);
                            break;
                        case ConditionEnum.VcEi:
                            expr = ast.CallStatic((Func<float, bool>) MathFloat.IsInfinity, left);
                            break;
                        case ConditionEnum.VcEs:
                            expr = ast.CallStatic((Func<float, bool>) MathFloat.IsNanOrInfinity, left);
                            break; // Tekken Dark Resurrection

                        case ConditionEnum.VcNz:
                            expr = ast.Binary(left, "!=", 0f);
                            break;
                        case ConditionEnum.VcNn:
                            expr = ast.Unary("!", ast.CallStatic((Func<float, bool>) MathFloat.IsNan, left));
                            break;
                        case ConditionEnum.VcNi:
                            expr = ast.Unary("!", ast.CallStatic((Func<float, bool>) MathFloat.IsInfinity, left));
                            break;
                        case ConditionEnum.VcNs:
                            expr = ast.Unary("!", ast.CallStatic((Func<float, bool>) MathFloat.IsNanOrInfinity, left));
                            break;

                        default: throw (new InvalidOperationException());
                    }

                    return ast.Statements(new List<AstNodeStm>
                    {
                        ast.Assign(localCcTemp, expr),
                        ast.AssignVcc(index, localCcTemp),
                        ast.Assign(localCcOr, ast.Binary(localCcOr, "||", localCcTemp)),
                        ast.Assign(localCcAnd, ast.Binary(localCcAnd, "&&", localCcTemp))
                    });
                }),
                ast.AssignVcc(4, localCcOr),
                ast.AssignVcc(5, localCcAnd)
            );
        }

        public AstNodeStm vslt() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<float, float, float>) CpuEmitterUtils._vslt_impl, VEC_VS[index],
                VEC_VT[index]), PC);

        public AstNodeStm vsge() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<float, float, float>) CpuEmitterUtils._vsge_impl, VEC_VS[index],
                VEC_VT[index]), PC);

        public AstNodeStm vscmp() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<float, float, float>) MathFloat.Sign2, VEC_VS[index], VEC_VT[index]), PC);

        public AstNodeStm _vcmovtf(bool True)
        {
            var register = (int) Instruction.Imm3;

            Func<int, AstNodeExpr> _VCC = index =>
            {
                AstNodeExpr ret = ast.Vcc(index);
                if (!True) ret = ast.Unary("!", ret);
                return ret;
            };

            if (register < 6)
            {
                // TODO: CHECK THIS!
                return ast.IfElse(
                    _VCC(register),
                    VEC_VD.SetVector(index => VEC_VS[index], PC),
                    ast.Statements(
                        ast.Assign(ast.PrefixSourceEnabled(), false),
                        //ast.If(ast.PrefixDestinationEnabled(), VEC_VD.SetVector(Index => VEC_VD[Index], PC))
                        ast.If(ast.PrefixDestinationEnabled(), VEC_VD.SetVector(index => VEC_VD[index], PC))
                    )
                );
            }

            if (register == 6)
            {
                return VEC_VD.SetVector(index => ast.Ternary(_VCC(index), VEC_VS[index], VEC_VD[index]), PC);
            }

            // Register == 7

            // Never copy (checked on a PSP)
            return ast.Statement();
        }

        public AstNodeStm vcmovf() => _vcmovtf(false);
        public AstNodeStm vcmovt() => _vcmovtf(true);

        private AstNodeStm _bvtf(bool True)
        {
            var register = (int) Instruction.Imm3;
            AstNodeExpr branchExpr = ast.Vcc(register);
            if (!True) branchExpr = ast.Unary("!", branchExpr);
            return AssignBranchFlag(branchExpr);
        }

        public AstNodeStm bvf() => _bvtf(false);
        public AstNodeStm bvfl() => bvf();
        public AstNodeStm bvt() => _bvtf(true);
        public AstNodeStm bvtl() => bvt();
    }
}