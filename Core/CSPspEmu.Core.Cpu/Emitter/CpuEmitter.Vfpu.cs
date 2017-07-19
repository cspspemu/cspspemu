using System;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Linq;
using System.Numerics;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed unsafe partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Vfpu DOT product
        // Vfpu SCaLe/ROTate
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm vdot() => CEL_VD.Set(
            _Aggregate(0f, ONE_TWO, (aggregated, index) => aggregated + (VEC_VS[index] * VEC_VT[index])), PC);

        public AstNodeStm vscl() => VEC_VD.SetVector(index => VEC_VS[index] * CEL_VT.Get(), PC);

        /// <summary>
        /// Vector ROTate
        /// </summary>
        public AstNodeStm vrot()
        {
            var imm5 = Instruction.Imm5;
            var cosIndex = BitUtils.Extract(imm5, 0, 2);
            var sinIndex = BitUtils.Extract(imm5, 2, 2);
            var negateSin = BitUtils.ExtractBool(imm5, 4);

            var dest = VEC_VD;
            var src = CEL_VS;

            AstNodeExpr sine = ast.CallStatic((Func<float, float>) MathFloat.SinV1, src.Get());
            AstNodeExpr cosine = ast.CallStatic((Func<float, float>) MathFloat.CosV1, src.Get());
            if (negateSin) sine = -sine;

            //Console.WriteLine("{0},{1},{2}", CosIndex, SinIndex, NegateSin);

            return dest.SetVector(index =>
            {
                if (index == cosIndex) return cosine;
                if (index == sinIndex) return sine;
                return (sinIndex == cosIndex) ? sine : 0f;
            }, PC);
        }

        // vzero: Vector ZERO
        // vone : Vector ONE
        public AstNodeStm vzero() => VEC_VD.SetVector(index => 0f, PC);

        public AstNodeStm vone() => VEC_VD.SetVector(index => 1f, PC);

        // vmov  : Vector MOVe
        // vsgn  : Vector SiGN
        // *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
        // @CHECK
        public AstNodeStm vmov()
        {
            PrefixTarget.Consume();
            return VEC_VD.SetVector(index => VEC_VS[index], PC);
        }

        public AstNodeStm vabs() =>
            VEC_VD.SetVector(index => ast.CallStatic((Func<float, float>) MathFloat.Abs, VEC_VS[index]), PC);

        public AstNodeStm vneg() => VEC_VD.SetVector(index => -VEC_VS[index], PC);
        public AstNodeStm vocp() => VEC_VD.SetVector(index => 1f - VEC_VS[index], PC);

        public AstNodeStm vsgn() =>
            VEC_VD.SetVector(index => ast.CallStatic((Func<float, float>) MathFloat.Sign, VEC_VS[index]), PC);

        public AstNodeStm vrcp() => VEC_VD.SetVector(index => 1f / VEC_VS[index], PC);

        private AstNodeStm _vfpu_call_ff(Delegate Delegate) =>
            VEC_VD.SetVector(index => ast.CallStatic(Delegate, VEC_VS[index]), PC);

        // OP_V_INTERNAL_IN_N!(1, "1.0f / sqrt(v)");
        // vcst: Vfpu ConSTant
        public AstNodeStm vsqrt() => _vfpu_call_ff((Func<float, float>) MathFloat.Sqrt);

        public AstNodeStm vrsq() => _vfpu_call_ff((Func<float, float>) MathFloat.RSqrt);
        public AstNodeStm vsin() => _vfpu_call_ff((Func<float, float>) MathFloat.SinV1);
        public AstNodeStm vcos() => _vfpu_call_ff((Func<float, float>) MathFloat.CosV1);
        public AstNodeStm vexp2() => _vfpu_call_ff((Func<float, float>) MathFloat.Exp2);
        public AstNodeStm vlog2() => _vfpu_call_ff((Func<float, float>) MathFloat.Log2);
        public AstNodeStm vasin() => _vfpu_call_ff((Func<float, float>) MathFloat.AsinV1);
        public AstNodeStm vnrcp() => _vfpu_call_ff((Func<float, float>) MathFloat.NRcp);
        public AstNodeStm vnsin() => _vfpu_call_ff((Func<float, float>) MathFloat.NSinV1);
        public AstNodeStm vrexp2() => _vfpu_call_ff((Func<float, float>) MathFloat.RExp2);
        public AstNodeStm vsat0() => _vfpu_call_ff((Func<float, float>) MathFloat.Vsat0);
        public AstNodeStm vsat1() => _vfpu_call_ff((Func<float, float>) MathFloat.Vsat1);

        // Vector -> Cell operations
        public AstNodeStm vcst() => CEL_VD.Set(VfpuConstants.GetConstantValueByIndex((int) Instruction.Imm5).Value, PC);

        public AstNodeStm vhdp()
        {
            var VectorSize = (uint) ONE_TWO;
            return CEL_VD.Set(_Aggregate(0f, (aggregate, index) =>
                aggregate + VEC_VT[index] * ((index == VectorSize - 1) ? 1f : VEC_VS[index])
            ), PC);
        }

        public AstNodeStm vcrs_t()
        {
            var vVd = VEC(VD, VType.VFloat, 3);
            var vVs = VEC(VS, VType.VFloat, 3);
            var vVt = VEC(VT, VType.VFloat, 3);
            return vVd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return vVs[1] * vVt[2];
                    case 1: return vVs[2] * vVt[0];
                    case 2: return vVs[0] * vVt[1];
                    default: throw (new InvalidOperationException("vcrs_t.Assert!"));
                }
            }, PC);
        }

        /// <summary>
        /// Cross product
        /// </summary>
        public AstNodeStm vcrsp_t()
        {
            var d = VEC(VD, VType.VFloat, 3);
            var s = VEC(VS, VType.VFloat, 3);
            var t = VEC(VT, VType.VFloat, 3);

            return d.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return s[1] * t[2] - s[2] * t[1];
                    case 1: return s[2] * t[0] - s[0] * t[2];
                    case 2: return s[0] * t[1] - s[1] * t[0];
                    default: throw (new InvalidOperationException("vcrsp_t.Assert!"));
                }
            }, PC);
        }

        // Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
        public AstNodeStm vmin() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<float, float, float>) MathFloat.Min, VEC_VS[index], VEC_VT[index]), PC);

        public AstNodeStm vmax() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<float, float, float>) MathFloat.Max, VEC_VS[index], VEC_VT[index]), PC);

        public AstNodeStm vadd() => VEC_VD.SetVector(index => VEC_VS[index] + VEC_VT[index], PC);
        public AstNodeStm vsub() => VEC_VD.SetVector(index => VEC_VS[index] - VEC_VT[index], PC);
        public AstNodeStm vdiv() => VEC_VD.SetVector(index => VEC_VS[index] / VEC_VT[index], PC);
        public AstNodeStm vmul() => VEC_VD.SetVector(index => VEC_VS[index] * VEC_VT[index], PC);

        // Vfpu (Matrix) IDenTity
        public AstNodeStm vidt() => VEC_VD.SetVector(index => (index == (Instruction.Imm7 % ONE_TWO)) ? 1f : 0f, PC);

        // Vfpu load Integer IMmediate
        public AstNodeStm viim() => CEL_VT_NoPrefix.Set((float) Instruction.Imm, PC);

        public AstNodeStm vdet()
        {
            var v1 = VEC(VS, VType.VFloat, 2);
            var v2 = VEC(VT, VType.VFloat, 2);
            return CEL_VD.Set(v1[0] * v2[1] - v1[1] * v2[0], PC);
        }

        public AstNodeStm mfvme() => ast.NotImplemented();
        public AstNodeStm mtvme() => ast.NotImplemented();
        public AstNodeStm vfim() => CEL_VT_NoPrefix.Set(Instruction.ImmHf, PC);
        public AstNodeStm vlgb() => ast.NotImplemented();
        public AstNodeStm vsbn() => ast.NotImplemented();
        public AstNodeStm vsbz() => ast.NotImplemented();

        public AstNodeStm vsocp()
        {
            var vectorSize = ONE_TWO;
            //Console.WriteLine("VECTOR_SIZE: {0}", VectorSize);
            var vvd = VEC(VD, VType.VFloat, vectorSize * 2);
            var vvs = VEC(VS, VType.VFloat, vectorSize);
            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0:
                        return ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, 1f - vvs[0], 0f, 1f);
                    case 1: return ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, vvs[0], 0f, 1f);
                    case 2:
                        return ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, 1f - vvs[1], 0f, 1f);
                    case 3: return ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, vvs[1], 0f, 1f);
                    default: throw (new NotImplementedException("vsocp: " + index));
                }
            }, PC);
        }

        public AstNodeStm vus2i() => ast.NotImplemented();

        public static float _vwbn_impl(float source, int imm8)
        {
            return 0f;
#if true
            var exp = new BigInteger((int) Math.Pow(2, 127 - imm8));
            var bn = new BigInteger((int) source);
            if ((int) bn > 0)
            {
                bn = BigInteger.ModPow(bn, exp, bn);
            }
            return (float) (bn + ((source < 0.0f) ? -exp : exp));

#else
			double exp = Math.Pow(2.0, 127 - Imm8);
			double bn = (double)Source;
			if (bn > 0.0) bn = Math.Pow(bn, exp) % bn;
			return (float)((Source < 0.0f) ? (bn - exp) : (bn + exp));
#endif
        }

        public AstNodeStm vwbn() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<float, int, float>) _vwbn_impl, VEC_VS[index], (int) Instruction.Imm8),
            PC);

        public AstNodeStm vnop() => ast.Statement();

        public AstNodeStm vsync() => ast.Statement();

        public AstNodeStm vflush() => ast.Statement();

        public static uint _vt4444_step(uint i0, uint i1)
        {
            uint o0 = 0;
            o0 |= ((i0 >> 4) & 15) << 0;
            o0 |= ((i0 >> 12) & 15) << 4;
            o0 |= ((i0 >> 20) & 15) << 8;
            o0 |= ((i0 >> 28) & 15) << 12;
            o0 |= ((i1 >> 4) & 15) << 16;
            o0 |= ((i1 >> 12) & 15) << 20;
            o0 |= ((i1 >> 20) & 15) << 24;
            o0 |= ((i1 >> 28) & 15) << 28;
            //Console.Error.WriteLine("{0:X8} {1:X8} -> {2:X8}", i0, i1, o0);
            //throw(new Exception("" + i0 + ";" + i1));
            return o0;
        }

        public static uint _vt5551_step(uint i0, uint i1)
        {
            uint o0 = 0;
            o0 |= ((i0 >> 3) & 31) << 0;
            o0 |= ((i0 >> 11) & 31) << 5;
            o0 |= ((i0 >> 19) & 31) << 10;
            o0 |= ((i0 >> 31) & 1) << 15;
            o0 |= ((i1 >> 3) & 31) << 16;
            o0 |= ((i1 >> 11) & 31) << 21;
            o0 |= ((i1 >> 19) & 31) << 26;
            o0 |= ((i1 >> 31) & 1) << 31;
            //throw (new Exception("" + i0 + ";" + i1));
            return o0;
        }

        public static uint _vt5650_step(uint i0, uint i1)
        {
            uint o0 = 0;
            o0 |= ((i0 >> 3) & 31) << 0;
            o0 |= ((i0 >> 10) & 63) << 5;
            o0 |= ((i0 >> 19) & 31) << 11;
            o0 |= ((i1 >> 3) & 31) << 16;
            o0 |= ((i1 >> 10) & 63) << 21;
            o0 |= ((i1 >> 19) & 31) << 27;
            //throw (new Exception("" + i0 + ";" + i1));
            return o0;
        }

        private AstNodeStm _vtXXXX_q(Func<uint, uint, uint> _vtXXXX_stepCallback)
        {
            var vectorSize = Instruction.OneTwo;
            if (vectorSize != 4) throw new Exception("Not implemented _vtXXXX_q for VectorSize=" + vectorSize);
            var dest = VEC(VD_NoPrefix, VUInt, 2);
            var src = VEC(VS_NoPrefix, VUInt, 4);
            //AstLoadVfpuReg

            var node = dest.SetVector(index => ast.CallStatic(
                _vtXXXX_stepCallback,
                src[index * 2 + 0],
                src[index * 2 + 1]
            ), PC);

            //throw(new Exception(GeneratorCSharp.GenerateString<GeneratorCSharp>(Node)));

            return node;
        }

        public AstNodeStm vt4444_q() => _vtXXXX_q(_vt4444_step);
        public AstNodeStm vt5551_q() => _vtXXXX_q(_vt5551_step);
        public AstNodeStm vt5650_q() => _vtXXXX_q(_vt5650_step);

        public AstNodeStm vbfy1() => VEC_VD.SetVector(index =>
        {
            switch (index)
            {
                case 0: return VEC_VS[0] + VEC_VS[1];
                case 1: return VEC_VS[0] - VEC_VS[1];
                case 2: return VEC_VS[2] + VEC_VS[3];
                case 3: return VEC_VS[2] - VEC_VS[3];
                default: throw (new InvalidOperationException("vbfy1.Assert!"));
            }
        }, PC);

        public AstNodeStm vbfy2() => VEC_VD.SetVector(index =>
        {
            switch (index)
            {
                case 0: return VEC_VS[0] + VEC_VS[2];
                case 1: return VEC_VS[1] + VEC_VS[3];
                case 2: return VEC_VS[0] - VEC_VS[2];
                case 3: return VEC_VS[1] - VEC_VS[3];
                default: throw (new InvalidOperationException("vbfy2.Assert!"));
            }
        }, PC);

        public AstNodeStm vsrt1()
        {
            var vectorSize = ONE_TWO;
            if (vectorSize != 4) return ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[1]);
                    case 1: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[1]);
                    case 2: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[2], vvs[3]);
                    case 3: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[2], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt1.Assert!"));
                }
            }, PC);
        }

        public AstNodeStm vsrt2()
        {
            var vectorSize = ONE_TWO;
            if (vectorSize != 4) return ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[3]);
                    case 1: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[1], vvs[2]);
                    case 2: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[1], vvs[2]);
                    case 3: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt2.Assert!"));
                }
            }, PC);
        }

        public AstNodeStm vsrt3()
        {
            var vectorSize = ONE_TWO;
            if (vectorSize != 4) return ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[1]);
                    case 1: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[1]);
                    case 2: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[2], vvs[3]);
                    case 3: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[2], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt3.Assert!"));
                }
            }, PC);
        }

        public AstNodeStm vsrt4()
        {
            var vectorSize = ONE_TWO;
            if (vectorSize != 4) return ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[3]);
                    case 1: return ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[1], vvs[2]);
                    case 2: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[1], vvs[2]);
                    case 3: return ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt4.Assert!"));
                }
            }, PC);
        }

        public AstNodeStm vfad() => CEL_VD.Set(_Aggregate(0f, (value, index) => value + VEC_VS[index]), PC);

        public AstNodeStm vavg() =>
            CEL_VD.Set(_Aggregate(0f, (value, index) => value + VEC_VS[index]) / (float) ONE_TWO, PC);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Prefixes
        /////////////////////////////////////////////////////////////////////////////////////////////////

        private AstNodeStm _vpfx_dst(IVfpuPrefixCommon prefix, Action<CpuThreadState, uint> vpfxDstImpl)
        {
            prefix.EnableAndSetValueAndPc(Instruction.Value, PC);
            return ast.Statement(ast.CallStatic(vpfxDstImpl, ast.CpuThreadStateExpr, Instruction.Value));
        }

        public AstNodeStm vpfxd() => _vpfx_dst(PrefixDestination, CpuEmitterUtils._vpfxd_impl);
        public AstNodeStm vpfxs() => _vpfx_dst(PrefixSource, CpuEmitterUtils._vpfxs_impl);
        public AstNodeStm vpfxt() => _vpfx_dst(PrefixTarget, CpuEmitterUtils._vpfxt_impl);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Random:
        //   Seed
        //   Integer: -2^31 <= value < 2^31 
        //   Float  : 0.0 <= value < 2.0.
        /////////////////////////////////////////////////////////////////////////////////////////////////

        public AstNodeStm vrnds() => ast.Statement(ast.CallStatic((Action<CpuThreadState, int>) CpuEmitterUtils._vrnds,
            ast.CpuThreadStateExpr));

        public AstNodeStm vrndi() => VEC_VD_i.SetVector(
            index => ast.CallStatic((Func<CpuThreadState, int>) CpuEmitterUtils._vrndi, ast.CpuThreadStateExpr),
            PC);

        public AstNodeStm vrndf1() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<CpuThreadState, float>) CpuEmitterUtils._vrndf1, ast.CpuThreadStateExpr),
            PC);

        public AstNodeStm vrndf2() => VEC_VD.SetVector(
            index => ast.CallStatic((Func<CpuThreadState, float>) CpuEmitterUtils._vrndf2, ast.CpuThreadStateExpr),
            PC);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Matrix Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////

        // Vfpu Matrix MULtiplication
        // @FIX!!!
        public AstNodeStm vmmul()
        {
            var vectorSize = Instruction.OneTwo;
            //var Dest = MAT(VD_NoPrefix);
            //var Src = MAT(VS_NoPrefix);
            //var Target = MAT(VT_NoPrefix);
            var dest = MAT_VD;
            var src = MAT_VS;
            var target = MAT_VT;

            return dest.SetMatrix((column, row) =>
            {
                var adder = (AstNodeExpr) ast.Immediate(0f);
                for (var n = 0; n < vectorSize; n++)
                {
                    adder += target[column, n] * src[row, n];
                    //Adder += Target[Column, n] * Src[Row, n];
                    //Adder += Target[Row, n] * Src[Column, n];
                    //Adder += Target[n, Column] * Src[n, Row];
                }
                return adder;
            }, PC);
        }

        // -

        private AstNodeStm _vtfm_x(int vectorSize)
        {
            var vecVd = VEC(VD, VType.VFloat, vectorSize);
            var vecVt = VEC(VT, VType.VFloat, vectorSize);
            var matVs = MAT(VS, VType.VFloat, vectorSize);

            return vecVd.SetVector(index =>
                    _Aggregate(0f, vectorSize,
                        (aggregatedValue, index2) => aggregatedValue + (matVs[index, index2] * vecVt[index2]))
                , PC);
        }

        private AstNodeStm _vhtfm_x(int vectorSize)
        {
            var vecVd = VEC(VD, VType.VFloat, vectorSize);
            var vecVt = VEC(VT, VType.VFloat, vectorSize);
            var matVs = MAT(VS, VType.VFloat, vectorSize);

            return vecVd.SetVector(index =>
                    _Aggregate(0f, vectorSize,
                        (aggregated, index2) =>
                            aggregated + matVs[index, index2] * ((index2 == vectorSize - 1) ? 1f : vecVt[index2]))
                , PC);
        }

        public AstNodeStm vtfm2() => _vtfm_x(2);
        public AstNodeStm vtfm3() => _vtfm_x(3);
        public AstNodeStm vtfm4() => _vtfm_x(4);
        public AstNodeStm vhtfm2() => _vhtfm_x(2);
        public AstNodeStm vhtfm3() => _vhtfm_x(3);
        public AstNodeStm vhtfm4() => _vhtfm_x(4);
        public AstNodeStm vmidt() => MAT_VD.SetMatrix((column, row) => (column == row) ? 1f : 0f, PC);
        public AstNodeStm vmzero() => MAT_VD.SetMatrix((column, row) => 0f, PC);
        public AstNodeStm vmone() => MAT_VD.SetMatrix((column, row) => 1f, PC);
        public AstNodeStm vmscl() => MAT_VD.SetMatrix((column, row) => MAT_VS[column, row] * CEL_VT.Get(), PC);

        public AstNodeStm vqmul()
        {
            var v1 = VEC(VS, VType.VFloat, 4);
            var v2 = VEC(VT, VType.VFloat, 4);

            return VEC(VD, VType.VFloat, 4).SetVector(index =>
            {
                switch (index)
                {
                    case 0: return +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
                    case 1: return -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
                    case 2: return +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
                    case 3: return -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);
                    default: throw (new InvalidOperationException("vqmul.Assert"));
                }
            }, PC);
        }

        public AstNodeStm vmmov() => MAT_VD.SetMatrix((Column, Row) => MAT_VS[Column, Row], PC);

        public AstNodeStm vuc2i() => VEC_VD_u.SetVector(
            index => ast.Binary((ast.Binary(CEL_VS_u.Get(), ">>", (index * 8)) & 0xFF) * 0x01010101, ">>", 1), PC);

        public AstNodeStm vc2i() => VEC_VD_u.SetVector(Index => ast.Binary(CEL_VS_u.Get(), "<<", ((3 - Index) * 8)) & 0xFF000000, PC);

        // Vfpu Integer to(2) Color?
        public AstNodeStm vi2c()
        {
            var vecVs = VEC(VS, VType.VUInt, 4);
            return CEL_VD_u.Set(
                ast.CallStatic((Func<uint, uint, uint, uint, uint>) CpuEmitterUtils._vi2c_impl, vecVs[0], vecVs[1],
                    vecVs[2], vecVs[3]), PC);
        }

        public AstNodeStm vi2uc()
        {
            var vecVs = VEC(VS, VType.VInt, 4);
            return CEL_VD_u.Set(
                ast.CallStatic((Func<int, int, int, int, uint>) CpuEmitterUtils._vi2uc_impl, vecVs[0], vecVs[1],
                    vecVs[2], vecVs[3]), PC);
        }

        public AstNodeStm vs2i()
        {
            var vectorSize = Instruction.OneTwo;
            if (vectorSize > 2) throw (new NotImplementedException("vs2i.VectorSize"));
            var dest = _Vector(VD, VUInt, vectorSize * 2);
            var src = _Vector(VS, VUInt, vectorSize);
            return dest.SetVector(index =>
            {
                var value = src[index / 2];
                if ((index % 2) == 0) value = ast.Binary(value, "<<", 16);
                return value & 0xFFFF0000;
            }, PC);
        }

        public AstNodeStm vi2f()
        {
            return VEC_VD.SetVector(
                index => ast.CallStatic((Func<float, int, float>) MathFloat.Scalb, ast.Cast<float>(VEC_VS_i[index]),
                    -(int) Instruction.Imm5), PC);
        }

        private AstNodeStm _vf2i_dnu(Func<float, int> roundingFunc)
        {
            return VEC_VD_i.SetVector(index =>
                    ast.CallStatic(
                        roundingFunc,
                        ast.CallStatic(
                            (Func<float, int, float>) MathFloat.Scalb,
                            VEC_VS[index],
                            (int) Instruction.Imm5
                        )
                    )
                , PC);
        }

        public AstNodeStm vf2id() => _vf2i_dnu(MathFloat.Floor);

        public AstNodeStm vf2in() => _vf2i_dnu(MathFloat.Round);

        public AstNodeStm vf2iu() => _vf2i_dnu(MathFloat.Ceil);

        public AstNodeStm vf2iz() => VEC_VD_i.SetVector(Index =>
                ast.CallStatic(
                    (Func<float, int, int>) CpuEmitterUtils._vf2iz,
                    VEC_VS[Index],
                    (int) Instruction.Imm5
                )
            , PC);

        public AstNodeStm vi2s()
        {
            var vectorSize = ONE_TWO;
            return _Vector(VD, VType.VUInt, vectorSize / 2)
                    .SetVector(index => ast.CallStatic(
                        (Func<uint, uint, uint>) CpuEmitterUtils._vi2s_impl,
                        VEC_VS_u[index * 2 + 0],
                        VEC_VS_u[index * 2 + 1]
                    ), PC)
                ;
        }

        public AstNodeStm vf2h()
        {
            var vecVd = VEC(VD, VType.VUInt, ONE_TWO / 2);
            var vecVs = VEC(VS, VType.VFloat, ONE_TWO);
            return vecVd.SetVector(index =>
                    ast.CallStatic(
                        (Func<float, float, uint>) CpuEmitterUtils._vf2h_impl,
                        vecVs[index * 2 + 0],
                        vecVs[index * 2 + 1]
                    )
                , PC);
        }

        public AstNodeStm vh2f()
        {
            var vecVd = VEC(VD, VType.VFloat, ONE_TWO * 2);
            var vecVs = VEC(VS, VType.VUInt, ONE_TWO);
            return vecVd.SetVector(index => ast.CallStatic(
                index % 2 == 0
                    ? (Func<uint, float>) CpuEmitterUtils._vh2f_0
                    : (Func<uint, float>) CpuEmitterUtils._vh2f_1,
                vecVs[index / 2]
            ), PC);
        }

        public AstNodeStm vi2us()
        {
            var vectorSize = ONE_TWO;
            return _Vector(VD, VType.VInt, vectorSize / 2)
                    .SetVector(index => ast.CallStatic(
                        (Func<int, int, int>) CpuEmitterUtils._vi2us_impl,
                        VEC_VS_i[index * 2 + 0],
                        VEC_VS_i[index * 2 + 1]
                    ), PC)
                ;
        }

        public AstNodeStm vmfvc() => ast.NotImplemented();
        public AstNodeStm vmtvc() => ast.NotImplemented();
        public AstNodeStm mtv() => CEL_VD.Set(ast.GPR_f(RT), PC);

        public AstNodeStm mtvc()
        {
            return ast.Statement(ast.CallStatic(
                (Action<CpuThreadState, VfpuControlRegistersEnum, uint>) CpuEmitterUtils._mtvc_impl,
                ast.CpuThreadStateExpr,
                ast.Cast<VfpuControlRegistersEnum>((int) (Instruction.Imm7 + 128), false),
                CEL_VD_u.Get()
            ));
            //_mtvc_impl
        }

        /// <summary>
        /// Copies a vfpu control register into a general purpose register.
        /// </summary>
        public AstNodeStm mfvc() => ast.AssignGpr(RT, ast.CallStatic(
            (Func<CpuThreadState, VfpuControlRegistersEnum, uint>) CpuEmitterUtils._mfvc_impl,
            ast.CpuThreadStateExpr,
            ast.Cast<VfpuControlRegistersEnum>((int) (Instruction.Imm7 + 128), false)
        ));

        // Move From/to Vfpu (C?)_
        public AstNodeStm mfv() => ast.AssignGPR_F(RT, CEL_VD.Get());

        // Load/Store Vfpu (Left/Right)_
        // ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        public AstNodeStm lv_q()
        {
            const int vectorSize = 4;
            var dest = _Vector(VT5_1, VFloat, size: vectorSize);
            var memoryVector = _MemoryVectorIMM14<float>(vectorSize);
            return dest.SetVector(index => memoryVector[index], PC);
        }

        /// <summary>
        /// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        /// </summary>
        public AstNodeStm sv_q()
        {
            var VectorSize = 4;
            var dest = _Vector(VT5_1, VFloat, VectorSize);
            var memoryVector = _MemoryVectorIMM14<float>(VectorSize);
            return memoryVector.SetVector(index => dest[index]);
        }

        private delegate void LvlSvlQDelegate(CpuThreadState cpuThreadState, bool save, float* r0, float* r1,
            float* r2, float* r3, uint address);

        private AstNodeStm _lv_sv_l_r_q(bool left, bool save)
        {
            var register = Instruction.Vt51;
            var methodInfo = left
                    ? (LvlSvlQDelegate) CpuEmitterUtils._lvl_svl_q
                    : CpuEmitterUtils._lvr_svr_q
                ;

            var vt5 = _Vector(VT5_1, VFloat, 4);

            return ast.Statement(ast.CallStatic(
                methodInfo,
                ast.CpuThreadStateExpr,
                save,
                ast.GetAddress(vt5.GetIndexRef(0)),
                ast.GetAddress(vt5.GetIndexRef(1)),
                ast.GetAddress(vt5.GetIndexRef(2)),
                ast.GetAddress(vt5.GetIndexRef(3)),
                Address_RS_IMM14(0)
            ));
        }

        public AstNodeStm lvl_q() => _lv_sv_l_r_q(left: true, save: false);
        public AstNodeStm svl_q() => _lv_sv_l_r_q(left: true, save: true);
        public AstNodeStm lvr_q() => _lv_sv_l_r_q(left: false, save: false);
        public AstNodeStm svr_q() => _lv_sv_l_r_q(left: false, save: true);
        public AstNodeStm lv_s() => _Cell(VT5_2).Set(ast.MemoryGetValue<float>(Memory, Address_RS_IMM14()), PC);
        public AstNodeStm sv_s() => ast.MemorySetValue<float>(Memory, Address_RS_IMM14(), _Cell(VT5_2).Get());
    }
}