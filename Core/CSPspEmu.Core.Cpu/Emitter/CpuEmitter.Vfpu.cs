﻿using System;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Linq;
using System.Numerics;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed unsafe partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Vfpu DOT product
        // Vfpu SCaLe/ROTate
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("vdot")]
        public AstNodeStm vdot() => CEL_VD.Set(
            _Aggregate(0f, OneTwo, (aggregated, index) => aggregated + (VEC_VS[index] * VEC_VT[index])), _pc);

        [InstructionName("vscl")]
        public AstNodeStm vscl() => VEC_VD.SetVector(index => VEC_VS[index] * CEL_VT.Get(), _pc);

        /// <summary>
        /// Vector ROTate
        /// </summary>
        [InstructionName("vrot")]
        public AstNodeStm vrot()
        {
            var imm5 = _instruction.Imm5;
            var cosIndex = BitUtils.Extract(imm5, 0, 2);
            var sinIndex = BitUtils.Extract(imm5, 2, 2);
            var negateSin = BitUtils.ExtractBool(imm5, 4);

            var dest = VEC_VD;
            var src = CEL_VS;

            AstNodeExpr sine = _ast.CallStatic((Func<float, float>) MathFloat.SinV1, src.Get());
            AstNodeExpr cosine = _ast.CallStatic((Func<float, float>) MathFloat.CosV1, src.Get());
            if (negateSin) sine = -sine;

            //Console.WriteLine("{0},{1},{2}", CosIndex, SinIndex, NegateSin);

            return dest.SetVector(index =>
            {
                if (index == cosIndex) return cosine;
                if (index == sinIndex) return sine;
                return (sinIndex == cosIndex) ? sine : 0f;
            }, _pc);
        }

        // vzero: Vector ZERO
        // vone : Vector ONE
        [InstructionName("vzero")]
        public AstNodeStm vzero() => VEC_VD.SetVector(index => 0f, _pc);

        [InstructionName("vone")]
        public AstNodeStm vone() => VEC_VD.SetVector(index => 1f, _pc);

        // vmov  : Vector MOVe
        // vsgn  : Vector SiGN
        // *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
        // @CHECK
        [InstructionName("vmov")]
        public AstNodeStm vmov()
        {
            PrefixTarget.Consume();
            return VEC_VD.SetVector(index => VEC_VS[index], _pc);
        }

        [InstructionName("vabs")]
        public AstNodeStm vabs() =>
            VEC_VD.SetVector(index => _ast.CallStatic((Func<float, float>) MathFloat.Abs, VEC_VS[index]), _pc);

        [InstructionName("vneg")]
        public AstNodeStm vneg() => VEC_VD.SetVector(index => -VEC_VS[index], _pc);

        [InstructionName("vocp")]
        public AstNodeStm vocp() => VEC_VD.SetVector(index => 1f - VEC_VS[index], _pc);

        [InstructionName("vsgn")]
        public AstNodeStm vsgn() =>
            VEC_VD.SetVector(index => _ast.CallStatic((Func<float, float>) MathFloat.Sign, VEC_VS[index]), _pc);

        [InstructionName("vrcp")]
        public AstNodeStm vrcp() => VEC_VD.SetVector(index => 1f / VEC_VS[index], _pc);

        private AstNodeStm _vfpu_call_ff(Delegate Delegate) =>
            VEC_VD.SetVector(index => _ast.CallStatic(Delegate, VEC_VS[index]), _pc);

        // OP_V_INTERNAL_IN_N!(1, "1.0f / sqrt(v)");
        // vcst: Vfpu ConSTant
        [InstructionName("vsqrt")]
        public AstNodeStm vsqrt() => _vfpu_call_ff((Func<float, float>) MathFloat.Sqrt);

        [InstructionName("vrsq")]
        public AstNodeStm vrsq() => _vfpu_call_ff((Func<float, float>) MathFloat.RSqrt);

        [InstructionName("vsin")]
        public AstNodeStm vsin() => _vfpu_call_ff((Func<float, float>) MathFloat.SinV1);

        [InstructionName("vcos")]
        public AstNodeStm vcos() => _vfpu_call_ff((Func<float, float>) MathFloat.CosV1);

        [InstructionName("vexp2")]
        public AstNodeStm vexp2() => _vfpu_call_ff((Func<float, float>) MathFloat.Exp2);

        [InstructionName("vlog2")]
        public AstNodeStm vlog2() => _vfpu_call_ff((Func<float, float>) MathFloat.Log2);

        [InstructionName("vasin")]
        public AstNodeStm vasin() => _vfpu_call_ff((Func<float, float>) MathFloat.AsinV1);

        [InstructionName("vnrcp")]
        public AstNodeStm vnrcp() => _vfpu_call_ff((Func<float, float>) MathFloat.NRcp);

        [InstructionName("vnsin")]
        public AstNodeStm vnsin() => _vfpu_call_ff((Func<float, float>) MathFloat.NSinV1);

        [InstructionName("vrexp2")]
        public AstNodeStm vrexp2() => _vfpu_call_ff((Func<float, float>) MathFloat.RExp2);

        [InstructionName("vsat0")]
        public AstNodeStm vsat0() => _vfpu_call_ff((Func<float, float>) MathFloat.Vsat0);

        [InstructionName("vsat1")]
        public AstNodeStm vsat1() => _vfpu_call_ff((Func<float, float>) MathFloat.Vsat1);

        // Vector -> Cell operations
        [InstructionName("vcst")]
        public AstNodeStm vcst() =>
            CEL_VD.Set(VfpuConstants.GetConstantValueByIndex((int) _instruction.Imm5).Value, _pc);

        [InstructionName("vhdp")]
        public AstNodeStm vhdp()
        {
            var vectorSize = (uint) OneTwo;
            return CEL_VD.Set(_Aggregate(0f, (aggregate, index) =>
                aggregate + VEC_VT[index] * ((index == vectorSize - 1) ? 1f : VEC_VS[index])
            ), _pc);
        }

        [InstructionName("vcrs.t")]
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
            }, _pc);
        }

        /// <summary>
        /// Cross product
        /// </summary>
        [InstructionName("vcrsp.t")]
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
            }, _pc);
        }

        // Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
        [InstructionName("vmin")]
        public AstNodeStm vmin() => VEC_VD.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) MathFloat.Min, VEC_VS[index], VEC_VT[index]), _pc);

        [InstructionName("vmax")]
        public AstNodeStm vmax() => VEC_VD.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) MathFloat.Max, VEC_VS[index], VEC_VT[index]), _pc);

        [InstructionName("vadd")]
        public AstNodeStm vadd() => VEC_VD.SetVector(index => VEC_VS[index] + VEC_VT[index], _pc);
        
        [InstructionName("vsub")]
        public AstNodeStm vsub() => VEC_VD.SetVector(index => VEC_VS[index] - VEC_VT[index], _pc);
        
        [InstructionName("vdiv")]
        public AstNodeStm vdiv() => VEC_VD.SetVector(index => VEC_VS[index] / VEC_VT[index], _pc);
        
        [InstructionName("vmul")]
        public AstNodeStm vmul() => VEC_VD.SetVector(index => VEC_VS[index] * VEC_VT[index], _pc);

        // Vfpu (Matrix) IDenTity
        [InstructionName("vidt")]
        public AstNodeStm vidt() => VEC_VD.SetVector(index => (index == (_instruction.Imm7 % OneTwo)) ? 1f : 0f, _pc);

        // Vfpu load Integer IMmediate
        [InstructionName("viim")]
        public AstNodeStm viim() => CEL_VT_NoPrefix.Set((float) _instruction.Imm, _pc);

        [InstructionName("vdet")]
        public AstNodeStm vdet()
        {
            var v1 = VEC(VS, VType.VFloat, 2);
            var v2 = VEC(VT, VType.VFloat, 2);
            return CEL_VD.Set(v1[0] * v2[1] - v1[1] * v2[0], _pc);
        }

        [InstructionName("mfvme")]
        public AstNodeStm mfvme() => _ast.NotImplemented();
        
        [InstructionName("mtvme")]
        public AstNodeStm mtvme() => _ast.NotImplemented();
        
        [InstructionName("vfim")]
        public AstNodeStm vfim() => CEL_VT_NoPrefix.Set(_instruction.ImmHf, _pc);
        
        [InstructionName("vlgb")]
        public AstNodeStm vlgb() => _ast.NotImplemented();
        
        [InstructionName("vsbn")]
        public AstNodeStm vsbn() => _ast.NotImplemented();
        
        [InstructionName("vsbz")]
        public AstNodeStm vsbz() => _ast.NotImplemented();

        [InstructionName("vsocp")]
        public AstNodeStm vsocp()
        {
            var vectorSize = OneTwo;
            //Console.WriteLine("VECTOR_SIZE: {0}", VectorSize);
            var vvd = VEC(VD, VType.VFloat, vectorSize * 2);
            var vvs = VEC(VS, VType.VFloat, vectorSize);
            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0:
                        return _ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, 1f - vvs[0], 0f, 1f);
                    case 1: return _ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, vvs[0], 0f, 1f);
                    case 2:
                        return _ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, 1f - vvs[1], 0f, 1f);
                    case 3: return _ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp, vvs[1], 0f, 1f);
                    default: throw (new NotImplementedException("vsocp: " + index));
                }
            }, _pc);
        }

        [InstructionName("vus2i")]
        public AstNodeStm vus2i() => _ast.NotImplemented();

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

        [InstructionName("vwbn")]
        public AstNodeStm vwbn() => VEC_VD.SetVector(
            index => _ast.CallStatic((Func<float, int, float>) _vwbn_impl, VEC_VS[index], (int) _instruction.Imm8),
            _pc);

        [InstructionName("vnop")]
        public AstNodeStm vnop() => _ast.Statement();

        [InstructionName("vsync")]
        public AstNodeStm vsync() => _ast.Statement();

        [InstructionName("vflush")]
        public AstNodeStm vflush() => _ast.Statement();

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
            var vectorSize = _instruction.OneTwo;
            if (vectorSize != 4) throw new Exception("Not implemented _vtXXXX_q for VectorSize=" + vectorSize);
            var dest = VEC(VD_NoPrefix, VUInt, 2);
            var src = VEC(VS_NoPrefix, VUInt, 4);
            //AstLoadVfpuReg

            var node = dest.SetVector(index => _ast.CallStatic(
                _vtXXXX_stepCallback,
                src[index * 2 + 0],
                src[index * 2 + 1]
            ), _pc);

            //throw(new Exception(GeneratorCSharp.GenerateString<GeneratorCSharp>(Node)));

            return node;
        }

        [InstructionName("vt4444.q")]
        public AstNodeStm vt4444_q() => _vtXXXX_q(_vt4444_step);
        
        [InstructionName("vt5551.q")]
        public AstNodeStm vt5551_q() => _vtXXXX_q(_vt5551_step);
        
        [InstructionName("vt5650.q")]
        public AstNodeStm vt5650_q() => _vtXXXX_q(_vt5650_step);

        [InstructionName("vbfy1")]
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
        }, _pc);

        [InstructionName("vbfy2")]
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
        }, _pc);

        [InstructionName("vsrt1")]
        public AstNodeStm vsrt1()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[1]);
                    case 1: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[1]);
                    case 2: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[2], vvs[3]);
                    case 3: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[2], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt1.Assert!"));
                }
            }, _pc);
        }

        [InstructionName("vsrt2")]
        public AstNodeStm vsrt2()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[3]);
                    case 1: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[1], vvs[2]);
                    case 2: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[1], vvs[2]);
                    case 3: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt2.Assert!"));
                }
            }, _pc);
        }

        [InstructionName("vsrt3")]
        public AstNodeStm vsrt3()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[1]);
                    case 1: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[1]);
                    case 2: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[2], vvs[3]);
                    case 3: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[2], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt3.Assert!"));
                }
            }, _pc);
        }

        [InstructionName("vsrt4")]
        public AstNodeStm vsrt4()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = VEC(VD, VType.VFloat, vectorSize);
            var vvs = VEC(VS, VType.VFloat, vectorSize);

            return vvd.SetVector(index =>
            {
                switch (index)
                {
                    case 0: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[0], vvs[3]);
                    case 1: return _ast.CallStatic((Func<float, float, float>) MathFloat.Max, vvs[1], vvs[2]);
                    case 2: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[1], vvs[2]);
                    case 3: return _ast.CallStatic((Func<float, float, float>) MathFloat.Min, vvs[0], vvs[3]);
                    default: throw (new InvalidOperationException("vsrt4.Assert!"));
                }
            }, _pc);
        }

        [InstructionName("vfad")]
        public AstNodeStm vfad() => CEL_VD.Set(_Aggregate(0f, (value, index) => value + VEC_VS[index]), _pc);

        [InstructionName("vavg")]
        public AstNodeStm vavg() =>
            CEL_VD.Set(_Aggregate(0f, (value, index) => value + VEC_VS[index]) / (float) OneTwo, _pc);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Prefixes
        /////////////////////////////////////////////////////////////////////////////////////////////////

        private AstNodeStm _vpfx_dst(IVfpuPrefixCommon prefix, Action<CpuThreadState, uint> vpfxDstImpl)
        {
            prefix.EnableAndSetValueAndPc(_instruction.Value, _pc);
            return _ast.Statement(_ast.CallStatic(vpfxDstImpl, _ast.CpuThreadStateExpr, _instruction.Value));
        }

        [InstructionName("vpfxd")]
        public AstNodeStm vpfxd() => _vpfx_dst(PrefixDestination, CpuEmitterUtils._vpfxd_impl);
        
        [InstructionName("vpfxs")]
        public AstNodeStm vpfxs() => _vpfx_dst(PrefixSource, CpuEmitterUtils._vpfxs_impl);
        
        [InstructionName("vpfxt")]
        public AstNodeStm vpfxt() => _vpfx_dst(PrefixTarget, CpuEmitterUtils._vpfxt_impl);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Random:
        //   Seed
        //   Integer: -2^31 <= value < 2^31 
        //   Float  : 0.0 <= value < 2.0.
        /////////////////////////////////////////////////////////////////////////////////////////////////

        [InstructionName("vrnds")]
        public AstNodeStm vrnds() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, int>) CpuEmitterUtils._vrnds,
            _ast.CpuThreadStateExpr));

        [InstructionName("vrndi")]
        public AstNodeStm vrndi() => VEC_VD_i.SetVector(
            index => _ast.CallStatic((Func<CpuThreadState, int>) CpuEmitterUtils._vrndi, _ast.CpuThreadStateExpr),
            _pc);

        [InstructionName("vrndf1")]
        public AstNodeStm vrndf1() => VEC_VD.SetVector(
            index => _ast.CallStatic((Func<CpuThreadState, float>) CpuEmitterUtils._vrndf1, _ast.CpuThreadStateExpr),
            _pc);

        [InstructionName("vrndf2")]
        public AstNodeStm vrndf2() => VEC_VD.SetVector(
            index => _ast.CallStatic((Func<CpuThreadState, float>) CpuEmitterUtils._vrndf2, _ast.CpuThreadStateExpr),
            _pc);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Matrix Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////

        // Vfpu Matrix MULtiplication
        // @FIX!!!
        [InstructionName("vmmul")]
        public AstNodeStm vmmul()
        {
            var vectorSize = _instruction.OneTwo;
            //var Dest = MAT(VD_NoPrefix);
            //var Src = MAT(VS_NoPrefix);
            //var Target = MAT(VT_NoPrefix);
            var dest = MAT_VD;
            var src = MAT_VS;
            var target = MAT_VT;

            return dest.SetMatrix((column, row) =>
            {
                var adder = (AstNodeExpr) _ast.Immediate(0f);
                for (var n = 0; n < vectorSize; n++)
                {
                    adder += target[column, n] * src[row, n];
                    //Adder += Target[Column, n] * Src[Row, n];
                    //Adder += Target[Row, n] * Src[Column, n];
                    //Adder += Target[n, Column] * Src[n, Row];
                }
                return adder;
            }, _pc);
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
                , _pc);
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
                , _pc);
        }

        [InstructionName("vtfm2")]
        public AstNodeStm vtfm2() => _vtfm_x(2);
        
        [InstructionName("vtfm3")]
        public AstNodeStm vtfm3() => _vtfm_x(3);
        
        [InstructionName("vtfm4")]
        public AstNodeStm vtfm4() => _vtfm_x(4);
        
        [InstructionName("vhtfm2")]
        public AstNodeStm vhtfm2() => _vhtfm_x(2);
        
        [InstructionName("vhtfm3")]
        public AstNodeStm vhtfm3() => _vhtfm_x(3);
        
        [InstructionName("vhtfm4")]
        public AstNodeStm vhtfm4() => _vhtfm_x(4);
        
        [InstructionName("vmidt")]
        public AstNodeStm vmidt() => MAT_VD.SetMatrix((column, row) => (column == row) ? 1f : 0f, _pc);
        
        [InstructionName("vmzero")]
        public AstNodeStm vmzero() => MAT_VD.SetMatrix((column, row) => 0f, _pc);
        
        [InstructionName("vmone")]
        public AstNodeStm vmone() => MAT_VD.SetMatrix((column, row) => 1f, _pc);
        
        [InstructionName("vmscl")]
        public AstNodeStm vmscl() => MAT_VD.SetMatrix((column, row) => MAT_VS[column, row] * CEL_VT.Get(), _pc);

        [InstructionName("vqmul")]
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
            }, _pc);
        }

        [InstructionName("vmmov")]
        public AstNodeStm vmmov() => MAT_VD.SetMatrix((Column, Row) => MAT_VS[Column, Row], _pc);

        [InstructionName("vuc2i")]
        public AstNodeStm vuc2i() => VEC_VD_u.SetVector(
            index => _ast.Binary((_ast.Binary(CEL_VS_u.Get(), ">>", (index * 8)) & 0xFF) * 0x01010101, ">>", 1), _pc);

        [InstructionName("vc2i")]
        public AstNodeStm vc2i() =>
            VEC_VD_u.SetVector(Index => _ast.Binary(CEL_VS_u.Get(), "<<", ((3 - Index) * 8)) & 0xFF000000, _pc);

        // Vfpu Integer to(2) Color?
        [InstructionName("vi2c")]
        public AstNodeStm vi2c()
        {
            var vecVs = VEC(VS, VType.VUInt, 4);
            return CEL_VD_u.Set(
                _ast.CallStatic((Func<uint, uint, uint, uint, uint>) CpuEmitterUtils._vi2c_impl, vecVs[0], vecVs[1],
                    vecVs[2], vecVs[3]), _pc);
        }

        [InstructionName("vi2uc")]
        public AstNodeStm vi2uc()
        {
            var vecVs = VEC(VS, VType.VInt, 4);
            return CEL_VD_u.Set(
                _ast.CallStatic((Func<int, int, int, int, uint>) CpuEmitterUtils._vi2uc_impl, vecVs[0], vecVs[1],
                    vecVs[2], vecVs[3]), _pc);
        }

        [InstructionName("vs2i")]
        public AstNodeStm vs2i()
        {
            var vectorSize = _instruction.OneTwo;
            if (vectorSize > 2) throw (new NotImplementedException("vs2i.VectorSize"));
            var dest = _Vector(VD, VUInt, vectorSize * 2);
            var src = _Vector(VS, VUInt, vectorSize);
            return dest.SetVector(index =>
            {
                var value = src[index / 2];
                if ((index % 2) == 0) value = _ast.Binary(value, "<<", 16);
                return value & 0xFFFF0000;
            }, _pc);
        }

        [InstructionName("vi2f")]
        public AstNodeStm vi2f()
        {
            return VEC_VD.SetVector(
                index => _ast.CallStatic((Func<float, int, float>) MathFloat.Scalb, _ast.Cast<float>(VEC_VS_i[index]),
                    -(int) _instruction.Imm5), _pc);
        }

        private AstNodeStm _vf2i_dnu(Func<float, int> roundingFunc)
        {
            return VEC_VD_i.SetVector(index =>
                    _ast.CallStatic(
                        roundingFunc,
                        _ast.CallStatic(
                            (Func<float, int, float>) MathFloat.Scalb,
                            VEC_VS[index],
                            (int) _instruction.Imm5
                        )
                    )
                , _pc);
        }

        [InstructionName("vf2id")]
        public AstNodeStm vf2id() => _vf2i_dnu(MathFloat.Floor);

        [InstructionName("vf2in")]
        public AstNodeStm vf2in() => _vf2i_dnu(MathFloat.Round);

        [InstructionName("vf2iu")]
        public AstNodeStm vf2iu() => _vf2i_dnu(MathFloat.Ceil);

        [InstructionName("vf2iz")]
        public AstNodeStm vf2iz() => VEC_VD_i.SetVector(Index =>
                _ast.CallStatic(
                    (Func<float, int, int>) CpuEmitterUtils._vf2iz,
                    VEC_VS[Index],
                    (int) _instruction.Imm5
                )
            , _pc);

        [InstructionName("vi2s")]
        public AstNodeStm vi2s()
        {
            var vectorSize = OneTwo;
            return _Vector(VD, VType.VUInt, vectorSize / 2)
                    .SetVector(index => _ast.CallStatic(
                        (Func<uint, uint, uint>) CpuEmitterUtils._vi2s_impl,
                        VEC_VS_u[index * 2 + 0],
                        VEC_VS_u[index * 2 + 1]
                    ), _pc)
                ;
        }

        [InstructionName("vf2h")]
        public AstNodeStm vf2h()
        {
            var vecVd = VEC(VD, VType.VUInt, OneTwo / 2);
            var vecVs = VEC(VS, VType.VFloat, OneTwo);
            return vecVd.SetVector(index =>
                    _ast.CallStatic(
                        (Func<float, float, uint>) CpuEmitterUtils._vf2h_impl,
                        vecVs[index * 2 + 0],
                        vecVs[index * 2 + 1]
                    )
                , _pc);
        }

        [InstructionName("vh2f")]
        public AstNodeStm vh2f()
        {
            var vecVd = VEC(VD, VType.VFloat, OneTwo * 2);
            var vecVs = VEC(VS, VType.VUInt, OneTwo);
            return vecVd.SetVector(index => _ast.CallStatic(
                index % 2 == 0
                    ? (Func<uint, float>) CpuEmitterUtils._vh2f_0
                    : (Func<uint, float>) CpuEmitterUtils._vh2f_1,
                vecVs[index / 2]
            ), _pc);
        }

        [InstructionName("vi2us")]
        public AstNodeStm vi2us()
        {
            var vectorSize = OneTwo;
            return _Vector(VD, VType.VInt, vectorSize / 2)
                    .SetVector(index => _ast.CallStatic(
                        (Func<int, int, int>) CpuEmitterUtils._vi2us_impl,
                        VEC_VS_i[index * 2 + 0],
                        VEC_VS_i[index * 2 + 1]
                    ), _pc)
                ;
        }

        [InstructionName("vmfvc")]
        public AstNodeStm vmfvc() => _ast.NotImplemented();
        
        [InstructionName("vmtvc")]
        public AstNodeStm vmtvc() => _ast.NotImplemented();
        
        [InstructionName("mtv")]
        public AstNodeStm mtv() => CEL_VD.Set(_ast.GPR_f(Rt), _pc);

        [InstructionName("mtvc")]
        public AstNodeStm mtvc()
        {
            return _ast.Statement(_ast.CallStatic(
                (Action<CpuThreadState, VfpuControlRegistersEnum, uint>) CpuEmitterUtils._mtvc_impl,
                _ast.CpuThreadStateExpr,
                _ast.Cast<VfpuControlRegistersEnum>((int) (_instruction.Imm7 + 128), false),
                CEL_VD_u.Get()
            ));
            //_mtvc_impl
        }

        /// <summary>
        /// Copies a vfpu control register into a general purpose register.
        /// </summary>
        [InstructionName("mfvc")]
        public AstNodeStm mfvc() => _ast.AssignGpr(Rt, _ast.CallStatic(
            (Func<CpuThreadState, VfpuControlRegistersEnum, uint>) CpuEmitterUtils._mfvc_impl,
            _ast.CpuThreadStateExpr,
            _ast.Cast<VfpuControlRegistersEnum>((int) (_instruction.Imm7 + 128), false)
        ));

        // Move From/to Vfpu (C?)_
        [InstructionName("mfv")]
        public AstNodeStm mfv() => _ast.AssignGPR_F(Rt, CEL_VD.Get());

        // Load/Store Vfpu (Left/Right)_
        // ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        [InstructionName("lv.q")]
        public AstNodeStm lv_q()
        {
            const int vectorSize = 4;
            var dest = _Vector(VT5_1, VFloat, size: vectorSize);
            var memoryVector = _MemoryVectorIMM14<float>(vectorSize);
            return dest.SetVector(index => memoryVector[index], _pc);
        }

        /// <summary>
        /// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        /// </summary>
        [InstructionName("sv.q")]
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
            var register = _instruction.Vt51;
            var methodInfo = left
                    ? (LvlSvlQDelegate) CpuEmitterUtils._lvl_svl_q
                    : CpuEmitterUtils._lvr_svr_q
                ;

            var vt5 = _Vector(VT5_1, VFloat, 4);

            return _ast.Statement(_ast.CallStatic(
                methodInfo,
                _ast.CpuThreadStateExpr,
                save,
                _ast.GetAddress(vt5.GetIndexRef(0)),
                _ast.GetAddress(vt5.GetIndexRef(1)),
                _ast.GetAddress(vt5.GetIndexRef(2)),
                _ast.GetAddress(vt5.GetIndexRef(3)),
                Address_RS_IMM14(0)
            ));
        }

        [InstructionName("lvl.q")]
        public AstNodeStm lvl_q() => _lv_sv_l_r_q(left: true, save: false);
        
        [InstructionName("svl.q")]
        public AstNodeStm svl_q() => _lv_sv_l_r_q(left: true, save: true);
        
        [InstructionName("lvr.q")]
        public AstNodeStm lvr_q() => _lv_sv_l_r_q(left: false, save: false);
        
        [InstructionName("svr.q")]
        public AstNodeStm svr_q() => _lv_sv_l_r_q(left: false, save: true);
        
        [InstructionName("lv.s")]
        public AstNodeStm lv_s() => _Cell(VT5_2).Set(_ast.MemoryGetValue<float>(_memory, Address_RS_IMM14()), _pc);
        
        [InstructionName("sv.s")]
        public AstNodeStm sv_s() => _ast.MemorySetValue<float>(_memory, Address_RS_IMM14(), _Cell(VT5_2).Get());
    }
}