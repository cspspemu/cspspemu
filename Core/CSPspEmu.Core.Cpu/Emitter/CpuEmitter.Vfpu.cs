using System;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed unsafe partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Vfpu DOT product
        // Vfpu SCaLe/ROTate
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Vdot)]
        public AstNodeStm Vdot() => CelVd.Set(
            _Aggregate(0f, OneTwo, (aggregated, index) => aggregated + (VecVs[index] * VecVt[index])), _pc);

        [InstructionName(InstructionNames.Vscl)]
        public AstNodeStm Vscl() => VecVd.SetVector(index => VecVs[index] * CelVt.Get(), _pc);

        /// <summary>
        /// Vector ROTate
        /// </summary>
        [InstructionName(InstructionNames.Vrot)]
        public AstNodeStm Vrot()
        {
            var imm5 = _instruction.Imm5;
            var cosIndex = BitUtils.Extract(imm5, 0, 2);
            var sinIndex = BitUtils.Extract(imm5, 2, 2);
            var negateSin = BitUtils.ExtractBool(imm5, 4);

            var dest = VecVd;
            var src = CelVs;

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
        [InstructionName(InstructionNames.Vzero)]
        public AstNodeStm Vzero() => VecVd.SetVector(index => 0f, _pc);

        [InstructionName(InstructionNames.Vone)]
        public AstNodeStm Vone() => VecVd.SetVector(index => 1f, _pc);

        // vmov  : Vector MOVe
        // vsgn  : Vector SiGN
        // *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
        // @CHECK
        [InstructionName(InstructionNames.Vmov)]
        public AstNodeStm Vmov()
        {
            PrefixTarget.Consume();
            return VecVd.SetVector(index => VecVs[index], _pc);
        }

        [InstructionName(InstructionNames.Vabs)]
        public AstNodeStm Vabs() =>
            VecVd.SetVector(index => _ast.CallStatic((Func<float, float>) MathFloat.Abs, VecVs[index]), _pc);

        [InstructionName(InstructionNames.Vneg)]
        public AstNodeStm Vneg() => VecVd.SetVector(index => -VecVs[index], _pc);

        [InstructionName(InstructionNames.Vocp)]
        public AstNodeStm Vocp() => VecVd.SetVector(index => 1f - VecVs[index], _pc);

        [InstructionName(InstructionNames.Vsgn)]
        public AstNodeStm Vsgn() =>
            VecVd.SetVector(index => _ast.CallStatic((Func<float, float>) MathFloat.Sign, VecVs[index]), _pc);

        [InstructionName(InstructionNames.Vrcp)]
        public AstNodeStm Vrcp() => VecVd.SetVector(index => 1f / VecVs[index], _pc);

        private AstNodeStm _vfpu_call_ff(Delegate Delegate) =>
            VecVd.SetVector(index => _ast.CallStatic(Delegate, VecVs[index]), _pc);

        // OP_V_INTERNAL_IN_N!(1, "1.0f / sqrt(v)");
        // vcst: Vfpu ConSTant
        [InstructionName(InstructionNames.Vsqrt)]
        public AstNodeStm Vsqrt() => _vfpu_call_ff((Func<float, float>) MathFloat.Sqrt);

        [InstructionName(InstructionNames.Vrsq)]
        public AstNodeStm Vrsq() => _vfpu_call_ff((Func<float, float>) MathFloat.RSqrt);

        [InstructionName(InstructionNames.Vsin)]
        public AstNodeStm Vsin() => _vfpu_call_ff((Func<float, float>) MathFloat.SinV1);

        [InstructionName(InstructionNames.Vcos)]
        public AstNodeStm Vcos() => _vfpu_call_ff((Func<float, float>) MathFloat.CosV1);

        [InstructionName(InstructionNames.Vexp2)]
        public AstNodeStm Vexp2() => _vfpu_call_ff((Func<float, float>) MathFloat.Exp2);

        [InstructionName(InstructionNames.Vlog2)]
        public AstNodeStm Vlog2() => _vfpu_call_ff((Func<float, float>) MathFloat.Log2);

        [InstructionName(InstructionNames.Vasin)]
        public AstNodeStm Vasin() => _vfpu_call_ff((Func<float, float>) MathFloat.AsinV1);

        [InstructionName(InstructionNames.Vnrcp)]
        public AstNodeStm Vnrcp() => _vfpu_call_ff((Func<float, float>) MathFloat.NRcp);

        [InstructionName(InstructionNames.Vnsin)]
        public AstNodeStm Vnsin() => _vfpu_call_ff((Func<float, float>) MathFloat.NSinV1);

        [InstructionName(InstructionNames.Vrexp2)]
        public AstNodeStm Vrexp2() => _vfpu_call_ff((Func<float, float>) MathFloat.RExp2);

        [InstructionName(InstructionNames.Vsat0)]
        public AstNodeStm Vsat0() => _vfpu_call_ff((Func<float, float>) MathFloat.Vsat0);

        [InstructionName(InstructionNames.Vsat1)]
        public AstNodeStm Vsat1() => _vfpu_call_ff((Func<float, float>) MathFloat.Vsat1);

        // Vector -> Cell operations
        [InstructionName(InstructionNames.Vcst)]
        public AstNodeStm Vcst() =>
            CelVd.Set(VfpuConstants.GetConstantValueByIndex((int) _instruction.Imm5).Value, _pc);

        [InstructionName(InstructionNames.Vhdp)]
        public AstNodeStm Vhdp()
        {
            var vectorSize = (uint) OneTwo;
            return CelVd.Set(_Aggregate(0f, (aggregate, index) =>
                aggregate + VecVt[index] * ((index == vectorSize - 1) ? 1f : VecVs[index])
            ), _pc);
        }

        [InstructionName(InstructionNames.VcrsT)]
        public AstNodeStm vcrs_t()
        {
            var vVd = Vec(Vd, VType.VFloat, 3);
            var vVs = Vec(Vs, VType.VFloat, 3);
            var vVt = Vec(Vt, VType.VFloat, 3);
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
        [InstructionName(InstructionNames.VcrspT)]
        public AstNodeStm vcrsp_t()
        {
            var d = Vec(Vd, VType.VFloat, 3);
            var s = Vec(Vs, VType.VFloat, 3);
            var t = Vec(Vt, VType.VFloat, 3);

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
        [InstructionName(InstructionNames.Vmin)]
        public AstNodeStm Vmin() => VecVd.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) MathFloat.Min, VecVs[index], VecVt[index]), _pc);

        [InstructionName(InstructionNames.Vmax)]
        public AstNodeStm Vmax() => VecVd.SetVector(
            index => _ast.CallStatic((Func<float, float, float>) MathFloat.Max, VecVs[index], VecVt[index]), _pc);

        [InstructionName(InstructionNames.Vadd)]
        public AstNodeStm Vadd() => VecVd.SetVector(index => VecVs[index] + VecVt[index], _pc);
        
        [InstructionName(InstructionNames.Vsub)]
        public AstNodeStm Vsub() => VecVd.SetVector(index => VecVs[index] - VecVt[index], _pc);
        
        [InstructionName(InstructionNames.Vdiv)]
        public AstNodeStm Vdiv() => VecVd.SetVector(index => VecVs[index] / VecVt[index], _pc);
        
        [InstructionName(InstructionNames.Vmul)]
        public AstNodeStm Vmul() => VecVd.SetVector(index => VecVs[index] * VecVt[index], _pc);

        // Vfpu (Matrix) IDenTity
        [InstructionName(InstructionNames.Vidt)]
        public AstNodeStm Vidt() => VecVd.SetVector(index => (index == (_instruction.Imm7 % OneTwo)) ? 1f : 0f, _pc);

        // Vfpu load Integer IMmediate
        [InstructionName(InstructionNames.Viim)]
        public AstNodeStm Viim() => CelVtNoPrefix.Set((float) _instruction.Imm, _pc);

        [InstructionName(InstructionNames.Vdet)]
        public AstNodeStm Vdet()
        {
            var v1 = Vec(Vs, VType.VFloat, 2);
            var v2 = Vec(Vt, VType.VFloat, 2);
            return CelVd.Set(v1[0] * v2[1] - v1[1] * v2[0], _pc);
        }

        [InstructionName(InstructionNames.Mfvme)]
        public AstNodeStm Mfvme() => _ast.NotImplemented();
        
        [InstructionName(InstructionNames.Mtvme)]
        public AstNodeStm Mtvme() => _ast.NotImplemented();
        
        [InstructionName(InstructionNames.Vfim)]
        public AstNodeStm Vfim() => CelVtNoPrefix.Set(_instruction.ImmHf, _pc);
        
        [InstructionName(InstructionNames.Vlgb)]
        public AstNodeStm Vlgb() => _ast.NotImplemented();
        
        [InstructionName(InstructionNames.Vsbn)]
        public AstNodeStm Vsbn() => _ast.NotImplemented();
        
        [InstructionName(InstructionNames.Vsbz)]
        public AstNodeStm Vsbz() => _ast.NotImplemented();

        [InstructionName(InstructionNames.Vsocp)]
        public AstNodeStm Vsocp()
        {
            var vectorSize = OneTwo;
            //Console.WriteLine("VECTOR_SIZE: {0}", VectorSize);
            var vvd = Vec(Vd, VType.VFloat, vectorSize * 2);
            var vvs = Vec(Vs, VType.VFloat, vectorSize);
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

        [InstructionName(InstructionNames.Vus2I)]
        public AstNodeStm Vus2I() => _ast.NotImplemented();

        public static float _vwbn_impl(float source, int imm8)
        {
            return 0f;
            //#if true
            //            var exp = new BigInteger((int) Math.Pow(2, 127 - imm8));
            //            var bn = new BigInteger((int) source);
            //            if ((int) bn > 0)
            //            {
            //                bn = BigInteger.ModPow(bn, exp, bn);
            //            }
            //            return (float) (bn + ((source < 0.0f) ? -exp : exp));
            //
            //#else
            //			double exp = Math.Pow(2.0, 127 - Imm8);
            //			double bn = (double)Source;
            //			if (bn > 0.0) bn = Math.Pow(bn, exp) % bn;
            //			return (float)((Source < 0.0f) ? (bn - exp) : (bn + exp));
            //#endif
        }

        [InstructionName(InstructionNames.Vwbn)]
        public AstNodeStm Vwbn() => VecVd.SetVector(
            index => _ast.CallStatic((Func<float, int, float>) _vwbn_impl, VecVs[index], (int) _instruction.Imm8),
            _pc);

        [InstructionName(InstructionNames.Vnop)]
        public AstNodeStm Vnop() => _ast.Statement();

        [InstructionName(InstructionNames.Vsync)]
        public AstNodeStm Vsync() => _ast.Statement();

        [InstructionName(InstructionNames.Vflush)]
        public AstNodeStm Vflush() => _ast.Statement();

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

        private AstNodeStm _vtXXXX_q(Func<uint, uint, uint> vtXxxxStepCallback)
        {
            var vectorSize = _instruction.OneTwo;
            if (vectorSize != 4) throw new Exception("Not implemented _vtXXXX_q for VectorSize=" + vectorSize);
            var dest = Vec(VdNoPrefix, VuInt, 2);
            var src = Vec(VsNoPrefix, VuInt, 4);
            //AstLoadVfpuReg

            var node = dest.SetVector(index => _ast.CallStatic(
                vtXxxxStepCallback,
                src[index * 2 + 0],
                src[index * 2 + 1]
            ), _pc);

            //throw(new Exception(GeneratorCSharp.GenerateString<GeneratorCSharp>(Node)));

            return node;
        }

        [InstructionName(InstructionNames.Vt4444Q)]
        public AstNodeStm vt4444_q() => _vtXXXX_q(_vt4444_step);
        
        [InstructionName(InstructionNames.Vt5551Q)]
        public AstNodeStm vt5551_q() => _vtXXXX_q(_vt5551_step);
        
        [InstructionName(InstructionNames.Vt5650Q)]
        public AstNodeStm vt5650_q() => _vtXXXX_q(_vt5650_step);

        [InstructionName(InstructionNames.Vbfy1)]
        public AstNodeStm Vbfy1() => VecVd.SetVector(index =>
        {
            switch (index)
            {
                case 0: return VecVs[0] + VecVs[1];
                case 1: return VecVs[0] - VecVs[1];
                case 2: return VecVs[2] + VecVs[3];
                case 3: return VecVs[2] - VecVs[3];
                default: throw (new InvalidOperationException("vbfy1.Assert!"));
            }
        }, _pc);

        [InstructionName(InstructionNames.Vbfy2)]
        public AstNodeStm Vbfy2() => VecVd.SetVector(index =>
        {
            switch (index)
            {
                case 0: return VecVs[0] + VecVs[2];
                case 1: return VecVs[1] + VecVs[3];
                case 2: return VecVs[0] - VecVs[2];
                case 3: return VecVs[1] - VecVs[3];
                default: throw (new InvalidOperationException("vbfy2.Assert!"));
            }
        }, _pc);

        [InstructionName(InstructionNames.Vsrt1)]
        public AstNodeStm Vsrt1()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = Vec(Vd, VType.VFloat, vectorSize);
            var vvs = Vec(Vs, VType.VFloat, vectorSize);

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

        [InstructionName(InstructionNames.Vsrt2)]
        public AstNodeStm Vsrt2()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = Vec(Vd, VType.VFloat, vectorSize);
            var vvs = Vec(Vs, VType.VFloat, vectorSize);

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

        [InstructionName(InstructionNames.Vsrt3)]
        public AstNodeStm Vsrt3()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = Vec(Vd, VType.VFloat, vectorSize);
            var vvs = Vec(Vs, VType.VFloat, vectorSize);

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

        [InstructionName(InstructionNames.Vsrt4)]
        public AstNodeStm Vsrt4()
        {
            var vectorSize = OneTwo;
            if (vectorSize != 4) return _ast.Statement();

            var vvd = Vec(Vd, VType.VFloat, vectorSize);
            var vvs = Vec(Vs, VType.VFloat, vectorSize);

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

        [InstructionName(InstructionNames.Vfad)]
        public AstNodeStm Vfad() => CelVd.Set(_Aggregate(0f, (value, index) => value + VecVs[index]), _pc);

        [InstructionName(InstructionNames.Vavg)]
        public AstNodeStm Vavg() =>
            CelVd.Set(_Aggregate(0f, (value, index) => value + VecVs[index]) / (float) OneTwo, _pc);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Prefixes
        /////////////////////////////////////////////////////////////////////////////////////////////////

        private AstNodeStm _vpfx_dst(IVfpuPrefixCommon prefix, Action<CpuThreadState, uint> vpfxDstImpl)
        {
            prefix.EnableAndSetValueAndPc(_instruction.Value, _pc);
            return _ast.Statement(_ast.CallStatic(vpfxDstImpl, _ast.CpuThreadStateExpr, _instruction.Value));
        }

        [InstructionName(InstructionNames.Vpfxd)]
        public AstNodeStm Vpfxd() => _vpfx_dst(PrefixDestination, CpuEmitterUtils._vpfxd_impl);
        
        [InstructionName(InstructionNames.Vpfxs)]
        public AstNodeStm Vpfxs() => _vpfx_dst(PrefixSource, CpuEmitterUtils._vpfxs_impl);
        
        [InstructionName(InstructionNames.Vpfxt)]
        public AstNodeStm Vpfxt() => _vpfx_dst(PrefixTarget, CpuEmitterUtils._vpfxt_impl);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Random:
        //   Seed
        //   Integer: -2^31 <= value < 2^31 
        //   Float  : 0.0 <= value < 2.0.
        /////////////////////////////////////////////////////////////////////////////////////////////////

        [InstructionName(InstructionNames.Vrnds)]
        public AstNodeStm Vrnds() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, int>) CpuEmitterUtils._vrnds,
            _ast.CpuThreadStateExpr));

        [InstructionName(InstructionNames.Vrndi)]
        public AstNodeStm Vrndi() => VecVdI.SetVector(
            index => _ast.CallStatic((Func<CpuThreadState, int>) CpuEmitterUtils._vrndi, _ast.CpuThreadStateExpr),
            _pc);

        [InstructionName(InstructionNames.Vrndf1)]
        public AstNodeStm Vrndf1() => VecVd.SetVector(
            index => _ast.CallStatic((Func<CpuThreadState, float>) CpuEmitterUtils._vrndf1, _ast.CpuThreadStateExpr),
            _pc);

        [InstructionName(InstructionNames.Vrndf2)]
        public AstNodeStm Vrndf2() => VecVd.SetVector(
            index => _ast.CallStatic((Func<CpuThreadState, float>) CpuEmitterUtils._vrndf2, _ast.CpuThreadStateExpr),
            _pc);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Matrix Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////

        // Vfpu Matrix MULtiplication
        // @FIX!!!
        [InstructionName(InstructionNames.Vmmul)]
        public AstNodeStm Vmmul()
        {
            var vectorSize = _instruction.OneTwo;
            //var Dest = MAT(VD_NoPrefix);
            //var Src = MAT(VS_NoPrefix);
            //var Target = MAT(VT_NoPrefix);
            var dest = MatVd;
            var src = MatVs;
            var target = MatVt;

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
            var vecVd = Vec(Vd, VType.VFloat, vectorSize);
            var vecVt = Vec(Vt, VType.VFloat, vectorSize);
            var matVs = Mat(Vs, VType.VFloat, vectorSize);

            return vecVd.SetVector(index =>
                    _Aggregate(0f, vectorSize,
                        (aggregatedValue, index2) => aggregatedValue + (matVs[index, index2] * vecVt[index2]))
                , _pc);
        }

        private AstNodeStm _vhtfm_x(int vectorSize)
        {
            var vecVd = Vec(Vd, VType.VFloat, vectorSize);
            var vecVt = Vec(Vt, VType.VFloat, vectorSize);
            var matVs = Mat(Vs, VType.VFloat, vectorSize);

            return vecVd.SetVector(index =>
                    _Aggregate(0f, vectorSize,
                        (aggregated, index2) =>
                            aggregated + matVs[index, index2] * ((index2 == vectorSize - 1) ? 1f : vecVt[index2]))
                , _pc);
        }

        [InstructionName(InstructionNames.Vtfm2)]
        public AstNodeStm Vtfm2() => _vtfm_x(2);
        
        [InstructionName(InstructionNames.Vtfm3)]
        public AstNodeStm Vtfm3() => _vtfm_x(3);
        
        [InstructionName(InstructionNames.Vtfm4)]
        public AstNodeStm Vtfm4() => _vtfm_x(4);
        
        [InstructionName(InstructionNames.Vhtfm2)]
        public AstNodeStm Vhtfm2() => _vhtfm_x(2);
        
        [InstructionName(InstructionNames.Vhtfm3)]
        public AstNodeStm Vhtfm3() => _vhtfm_x(3);
        
        [InstructionName(InstructionNames.Vhtfm4)]
        public AstNodeStm Vhtfm4() => _vhtfm_x(4);
        
        [InstructionName(InstructionNames.Vmidt)]
        public AstNodeStm Vmidt() => MatVd.SetMatrix((column, row) => (column == row) ? 1f : 0f, _pc);
        
        [InstructionName(InstructionNames.Vmzero)]
        public AstNodeStm Vmzero() => MatVd.SetMatrix((column, row) => 0f, _pc);
        
        [InstructionName(InstructionNames.Vmone)]
        public AstNodeStm Vmone() => MatVd.SetMatrix((column, row) => 1f, _pc);
        
        [InstructionName(InstructionNames.Vmscl)]
        public AstNodeStm Vmscl() => MatVd.SetMatrix((column, row) => MatVs[column, row] * CelVt.Get(), _pc);

        [InstructionName(InstructionNames.Vqmul)]
        public AstNodeStm Vqmul()
        {
            var v1 = Vec(Vs, VType.VFloat, 4);
            var v2 = Vec(Vt, VType.VFloat, 4);

            return Vec(Vd, VType.VFloat, 4).SetVector(index =>
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

        [InstructionName(InstructionNames.Vmmov)]
        public AstNodeStm Vmmov() => MatVd.SetMatrix((column, row) => MatVs[column, row], _pc);

        [InstructionName(InstructionNames.Vuc2I)]
        public AstNodeStm Vuc2I() => VecVdU.SetVector(
            index => _ast.Binary((_ast.Binary(CelVsU.Get(), ">>", (index * 8)) & 0xFF) * 0x01010101, ">>", 1), _pc);

        [InstructionName(InstructionNames.Vc2I)]
        public AstNodeStm Vc2I() =>
            VecVdU.SetVector(index => _ast.Binary(CelVsU.Get(), "<<", ((3 - index) * 8)) & 0xFF000000, _pc);

        // Vfpu Integer to(2) Color?
        [InstructionName(InstructionNames.Vi2C)]
        public AstNodeStm Vi2C()
        {
            var vecVs = Vec(Vs, VType.VuInt, 4);
            return CelVdU.Set(
                _ast.CallStatic((Func<uint, uint, uint, uint, uint>) CpuEmitterUtils._vi2c_impl, vecVs[0], vecVs[1],
                    vecVs[2], vecVs[3]), _pc);
        }

        [InstructionName(InstructionNames.Vi2Uc)]
        public AstNodeStm Vi2Uc()
        {
            var vecVs = Vec(Vs, VType.VInt, 4);
            return CelVdU.Set(
                _ast.CallStatic((Func<int, int, int, int, uint>) CpuEmitterUtils._vi2uc_impl, vecVs[0], vecVs[1],
                    vecVs[2], vecVs[3]), _pc);
        }

        [InstructionName(InstructionNames.Vs2I)]
        public AstNodeStm Vs2I()
        {
            var vectorSize = _instruction.OneTwo;
            if (vectorSize > 2) throw (new NotImplementedException("vs2i.VectorSize"));
            var dest = _Vector(Vd, VuInt, vectorSize * 2);
            var src = _Vector(Vs, VuInt, vectorSize);
            return dest.SetVector(index =>
            {
                var value = src[index / 2];
                if ((index % 2) == 0) value = _ast.Binary(value, "<<", 16);
                return value & 0xFFFF0000;
            }, _pc);
        }

        [InstructionName(InstructionNames.Vi2F)]
        public AstNodeStm Vi2F()
        {
            return VecVd.SetVector(
                index => _ast.CallStatic((Func<float, int, float>) MathFloat.Scalb, _ast.Cast<float>(VecVsI[index]),
                    -(int) _instruction.Imm5), _pc);
        }

        private AstNodeStm _vf2i_dnu(Func<float, int> roundingFunc)
        {
            return VecVdI.SetVector(index =>
                    _ast.CallStatic(
                        roundingFunc,
                        _ast.CallStatic(
                            (Func<float, int, float>) MathFloat.Scalb,
                            VecVs[index],
                            (int) _instruction.Imm5
                        )
                    )
                , _pc);
        }

        [InstructionName(InstructionNames.Vf2Id)]
        public AstNodeStm Vf2Id() => _vf2i_dnu(MathFloat.Floor);

        [InstructionName(InstructionNames.Vf2In)]
        public AstNodeStm Vf2In() => _vf2i_dnu(MathFloat.Round);

        [InstructionName(InstructionNames.Vf2Iu)]
        public AstNodeStm Vf2Iu() => _vf2i_dnu(MathFloat.Ceil);

        [InstructionName(InstructionNames.Vf2Iz)]
        public AstNodeStm Vf2Iz() => VecVdI.SetVector(index =>
                _ast.CallStatic(
                    (Func<float, int, int>) CpuEmitterUtils._vf2iz,
                    VecVs[index],
                    (int) _instruction.Imm5
                )
            , _pc);

        [InstructionName(InstructionNames.Vi2S)]
        public AstNodeStm Vi2S()
        {
            var vectorSize = OneTwo;
            return _Vector(Vd, VType.VuInt, vectorSize / 2)
                    .SetVector(index => _ast.CallStatic(
                        (Func<uint, uint, uint>) CpuEmitterUtils._vi2s_impl,
                        VecVsU[index * 2 + 0],
                        VecVsU[index * 2 + 1]
                    ), _pc)
                ;
        }

        [InstructionName(InstructionNames.Vf2H)]
        public AstNodeStm Vf2H()
        {
            var vecVd = Vec(Vd, VType.VuInt, OneTwo / 2);
            var vecVs = Vec(Vs, VType.VFloat, OneTwo);
            return vecVd.SetVector(index =>
                    _ast.CallStatic(
                        (Func<float, float, uint>) CpuEmitterUtils._vf2h_impl,
                        vecVs[index * 2 + 0],
                        vecVs[index * 2 + 1]
                    )
                , _pc);
        }

        [InstructionName(InstructionNames.Vh2F)]
        public AstNodeStm Vh2F()
        {
            var vecVd = Vec(Vd, VType.VFloat, OneTwo * 2);
            var vecVs = Vec(Vs, VType.VuInt, OneTwo);
            return vecVd.SetVector(index => _ast.CallStatic(
                index % 2 == 0
                    ? (Func<uint, float>) CpuEmitterUtils._vh2f_0
                    : (Func<uint, float>) CpuEmitterUtils._vh2f_1,
                vecVs[index / 2]
            ), _pc);
        }

        [InstructionName(InstructionNames.Vi2Us)]
        public AstNodeStm Vi2Us()
        {
            var vectorSize = OneTwo;
            return _Vector(Vd, VType.VInt, vectorSize / 2)
                    .SetVector(index => _ast.CallStatic(
                        (Func<int, int, int>) CpuEmitterUtils._vi2us_impl,
                        VecVsI[index * 2 + 0],
                        VecVsI[index * 2 + 1]
                    ), _pc)
                ;
        }

        [InstructionName(InstructionNames.Vmfvc)]
        public AstNodeStm Vmfvc() => _ast.NotImplemented();
        
        [InstructionName(InstructionNames.Vmtvc)]
        public AstNodeStm Vmtvc() => _ast.NotImplemented();
        
        [InstructionName(InstructionNames.Mtv)]
        public AstNodeStm Mtv() => CelVd.Set(_ast.GPR_f(Rt), _pc);

        [InstructionName(InstructionNames.Mtvc)]
        public AstNodeStm Mtvc()
        {
            return _ast.Statement(_ast.CallStatic(
                (Action<CpuThreadState, VfpuControlRegistersEnum, uint>) CpuEmitterUtils._mtvc_impl,
                _ast.CpuThreadStateExpr,
                _ast.Cast<VfpuControlRegistersEnum>((int) (_instruction.Imm7 + 128), false),
                CelVdU.Get()
            ));
            //_mtvc_impl
        }

        /// <summary>
        /// Copies a vfpu control register into a general purpose register.
        /// </summary>
        [InstructionName(InstructionNames.Mfvc)]
        public AstNodeStm Mfvc() => _ast.AssignGpr(Rt, _ast.CallStatic(
            (Func<CpuThreadState, VfpuControlRegistersEnum, uint>) CpuEmitterUtils._mfvc_impl,
            _ast.CpuThreadStateExpr,
            _ast.Cast<VfpuControlRegistersEnum>((int) (_instruction.Imm7 + 128), false)
        ));

        // Move From/to Vfpu (C?)_
        [InstructionName(InstructionNames.Mfv)]
        public AstNodeStm Mfv() => _ast.AssignGPR_F(Rt, CelVd.Get());

        // Load/Store Vfpu (Left/Right)_
        // ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        [InstructionName(InstructionNames.LvQ)]
        public AstNodeStm lv_q()
        {
            const int vectorSize = 4;
            var dest = _Vector(Vt51, VFloat, size: vectorSize);
            var memoryVector = _MemoryVectorIMM14<float>(vectorSize);
            return dest.SetVector(index => memoryVector[index], _pc);
        }

        /// <summary>
        /// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        /// </summary>
        [InstructionName(InstructionNames.SvQ)]
        public AstNodeStm sv_q()
        {
            var vectorSize = 4;
            var dest = _Vector(Vt51, VFloat, vectorSize);
            var memoryVector = _MemoryVectorIMM14<float>(vectorSize);
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

            var vt5 = _Vector(Vt51, VFloat, 4);

            return _ast.Statement(_ast.CallStatic(
                methodInfo,
                _ast.CpuThreadStateExpr,
                save,
                _ast.GetAddress(vt5.GetIndexRef(0)),
                _ast.GetAddress(vt5.GetIndexRef(1)),
                _ast.GetAddress(vt5.GetIndexRef(2)),
                _ast.GetAddress(vt5.GetIndexRef(3)),
                Address_RS_IMM14()
            ));
        }

        [InstructionName(InstructionNames.LvlQ)]
        public AstNodeStm lvl_q() => _lv_sv_l_r_q(left: true, save: false);
        
        [InstructionName(InstructionNames.SvlQ)]
        public AstNodeStm svl_q() => _lv_sv_l_r_q(left: true, save: true);
        
        [InstructionName(InstructionNames.LvrQ)]
        public AstNodeStm lvr_q() => _lv_sv_l_r_q(left: false, save: false);
        
        [InstructionName(InstructionNames.SvrQ)]
        public AstNodeStm svr_q() => _lv_sv_l_r_q(left: false, save: true);
        
        [InstructionName(InstructionNames.LvS)]
        public AstNodeStm lv_s() => _Cell(Vt52).Set(_ast.MemoryGetValue<float>(_memory, Address_RS_IMM14()), _pc);
        
        [InstructionName(InstructionNames.SvS)]
        public AstNodeStm sv_s() => _ast.MemorySetValue<float>(_memory, Address_RS_IMM14(), _Cell(Vt52).Get());
    }
}