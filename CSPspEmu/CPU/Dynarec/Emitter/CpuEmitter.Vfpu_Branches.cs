using System;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;
using SafeILGenerator.Ast;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu.Table;

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

        [InstructionName(InstructionNames.Vcmp)]
        public AstNodeStm Vcmp()
        {
            var vectorSize = _instruction.OneTwo;
            var cond = _instruction.Imm4;
            var cond2 = (ConditionEnum) cond;
            //bool NormalFlag = (Cond & 8) == 0;
            //bool NotFlag = (Cond & 4) != 0;
            //uint TypeFlag = (Cond & 3);

            var localCcTemp = _ast.Local(AstLocal.Create<bool>());
            var localCcOr = _ast.Local(AstLocal.Create<bool>());
            var localCcAnd = _ast.Local(AstLocal.Create<bool>());

            return _ast.Statements(
                _ast.Assign(localCcOr, false),
                _ast.Assign(localCcAnd, true),

                // TODO: CHECK THIS!
                _ast.AssignVcc(0, true),
                _ast.AssignVcc(1, true),
                _ast.AssignVcc(2, true),
                _ast.AssignVcc(3, true),
                _List(vectorSize, index =>
                {
                    AstNodeExpr expr;
                    //bool UsedForAggregate;

                    var left = VecVs[index];
                    var right = VecVt[index];
                    switch (cond2)
                    {
                        case ConditionEnum.VcFl:
                            expr = _ast.Immediate(false);
                            break;
                        case ConditionEnum.VcEq:
                            expr = _ast.Binary(left, "==", right);
                            break;
                        case ConditionEnum.VcLt:
                            expr = _ast.Binary(left, "<", right);
                            break;
                        //case ConditionEnum.VC_LE: Expr = ast.Binary(Left, "<=", Right); break;
                        case ConditionEnum.VcLe:
                            expr = _ast.CallStatic((Func<float, float, bool>) MathFloat.IsLessOrEqualsThan, left,
                                right);
                            break;

                        case ConditionEnum.VcTr:
                            expr = _ast.Immediate(true);
                            break;
                        case ConditionEnum.VcNe:
                            expr = _ast.Binary(left, "!=", right);
                            break;
                        //case ConditionEnum.VC_GE: Expr = ast.Binary(Left, ">=", Right); break;
                        case ConditionEnum.VcGe:
                            expr = _ast.CallStatic((Func<float, float, bool>) MathFloat.IsGreatOrEqualsThan, left,
                                right);
                            break;
                        case ConditionEnum.VcGt:
                            expr = _ast.Binary(left, ">", right);
                            break;

                        case ConditionEnum.VcEz:
                            expr = _ast.Binary(_ast.Binary(left, "==", 0.0f), "||", _ast.Binary(left, "==", -0.0f));
                            break;
                        case ConditionEnum.VcEn:
                            expr = _ast.CallStatic((Func<float, bool>) MathFloat.IsNan, left);
                            break;
                        case ConditionEnum.VcEi:
                            expr = _ast.CallStatic((Func<float, bool>) MathFloat.IsInfinity, left);
                            break;
                        case ConditionEnum.VcEs:
                            expr = _ast.CallStatic((Func<float, bool>) MathFloat.IsNanOrInfinity, left);
                            break; // Tekken Dark Resurrection

                        case ConditionEnum.VcNz:
                            expr = _ast.Binary(left, "!=", 0f);
                            break;
                        case ConditionEnum.VcNn:
                            expr = _ast.Unary("!", _ast.CallStatic((Func<float, bool>) MathFloat.IsNan, left));
                            break;
                        case ConditionEnum.VcNi:
                            expr = _ast.Unary("!", _ast.CallStatic((Func<float, bool>) MathFloat.IsInfinity, left));
                            break;
                        case ConditionEnum.VcNs:
                            expr = _ast.Unary("!",
                                _ast.CallStatic((Func<float, bool>) MathFloat.IsNanOrInfinity, left));
                            break;

                        default: throw (new InvalidOperationException());
                    }

                    return _ast.Statements(new List<AstNodeStm>
                    {
                        _ast.Assign(localCcTemp, expr),
                        _ast.AssignVcc(index, localCcTemp),
                        _ast.Assign(localCcOr, _ast.Binary(localCcOr, "||", localCcTemp)),
                        _ast.Assign(localCcAnd, _ast.Binary(localCcAnd, "&&", localCcTemp))
                    });
                }),
                _ast.AssignVcc(4, localCcOr),
                _ast.AssignVcc(5, localCcAnd)
            );
        }

        [InstructionName(InstructionNames.Vslt)]
        public AstNodeStm Vslt() => VecVd.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) CpuEmitterUtils._vslt_impl, VecVs[index],
                VecVt[index]), _pc);

        [InstructionName(InstructionNames.Vsge)]
        public AstNodeStm Vsge() => VecVd.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) CpuEmitterUtils._vsge_impl, VecVs[index],
                VecVt[index]), _pc);

        [InstructionName(InstructionNames.Vscmp)]
        public AstNodeStm Vscmp() => VecVd.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) MathFloat.Sign2, VecVs[index], VecVt[index]), _pc);

        public AstNodeStm _vcmovtf(bool True)
        {
            var register = (int) _instruction.Imm3;

            Func<int, AstNodeExpr> vcc = index =>
            {
                AstNodeExpr ret = _ast.Vcc(index);
                if (!True) ret = _ast.Unary("!", ret);
                return ret;
            };

            if (register < 6)
            {
                // TODO: CHECK THIS!
                return _ast.IfElse(
                    vcc(register),
                    VecVd.SetVector(index => VecVs[index], _pc),
                    _ast.Statements(
                        _ast.Assign(_ast.PrefixSourceEnabled(), false),
                        //ast.If(ast.PrefixDestinationEnabled(), VEC_VD.SetVector(Index => VEC_VD[Index], PC))
                        _ast.If(_ast.PrefixDestinationEnabled(), VecVd.SetVector(index => VecVd[index], _pc))
                    )
                );
            }

            if (register == 6)
            {
                return VecVd.SetVector(index => _ast.Ternary(vcc(index), VecVs[index], VecVd[index]), _pc);
            }

            // Register == 7

            // Never copy (checked on a PSP)
            return _ast.Statement();
        }

        [InstructionName(InstructionNames.Vcmovf)]
        public AstNodeStm Vcmovf() => _vcmovtf(false);

        [InstructionName(InstructionNames.Vcmovt)]
        public AstNodeStm Vcmovt() => _vcmovtf(true);

        private AstNodeStm _bvtf(bool True)
        {
            var register = (int) _instruction.Imm3;
            AstNodeExpr branchExpr = _ast.Vcc(register);
            if (!True) branchExpr = _ast.Unary("!", branchExpr);
            return AssignBranchFlag(branchExpr);
        }

        [InstructionName(InstructionNames.Bvf)]
        public AstNodeStm Bvf() => _bvtf(false);

        [InstructionName(InstructionNames.Bvfl)]
        public AstNodeStm Bvfl() => Bvf();

        [InstructionName(InstructionNames.Bvt)]
        public AstNodeStm Bvt() => _bvtf(true);

        [InstructionName(InstructionNames.Bvtl)]
        public AstNodeStm Bvtl() => Bvt();
    }
}