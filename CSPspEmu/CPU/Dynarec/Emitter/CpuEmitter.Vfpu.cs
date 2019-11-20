using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast;

namespace CSPspEmu.Core.Cpu.Emitter
{
    // http://forums.ps2dev.org/viewtopic.php?t=6929 
    // http://wiki.fx-world.org/doku.php?do=index
    // http://mrmrice.fx-world.org/vfpu.html
    // http://hitmen.c02.at/files/yapspd/psp_doc/chap4.html
    // pspgl_codegen.h
    // 
    // *
    //  Before you begin messing with the vfpu, you need to do one thing in your project:
    //  PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_VFPU);
    //  Almost all psp applications define this in the projects main c file. It sets a value that tells the psp how to handle your applications thread
    //  in case the kernel needs to switch to another thread and back to yours. You need to add PSP_THREAD_ATTR_VFPU to this so the psp's kernel will
    //  properly save/restore the vfpu state on thread switch, otherwise bad things might happen if another thread uses the vfpu and stomps on whatever was in there.
    // 
    //  Before diving into the more exciting bits, first you need to know how the VFPU registers are configured.
    //  The vfpu contains 128 32-bit floating point registers (same format as the float type in C).
    //  These registers can be accessed individually or in groups of 2, 3, 4, 9 or 16 in one instruction.
    //  They are organized as 8 blocks of registers, 16 per block.When you write code to access these registers, there is a naming convention you must use.
    //  
    //  Every register name has 4 characters: Xbcr
    //  
    //  X can be one of:
    //    M - this identifies a matrix block of 4, 9 or 16 registers
    //    E - this identifies a transposed matrix block of 4, 9 or 16 registers
    //    C - this identifies a column of 2, 3 or 4 registers
    //    R - this identifies a row of 2, 3, or 4 registers
    //    S - this identifies a single register
    // 
    //  b can be one of:
    //    0 - register block 0
    //    1 - register block 1
    //    2 - register block 2
    //    3 - register block 3
    //    4 - register block 4
    //    5 - register block 5
    //    6 - register block 6
    //    7 - register block 7
    // 
    //  c can be one of:
    //    0 - column 0
    //    1 - column 1
    //    2 - column 2
    //    3 - column 3
    // 
    //  r can be one of:
    //    0 - row 0
    //    1 - row 1
    //    2 - row 2
    //    3 - row 3
    // 
    //  So for example, the register name S132 would be a single register in column 3, row 2 in register block 1.
    //  M500 would be a matrix of registers in register block 5.
    // 
    //  Almost every vfpu instruction will end with one of the following extensions:
    //    .s - instruction works on a single register
    //    .p - instruction works on a 2 register vector or 2x2 matrix
    //    .t - instruction works on a 3 register vector or 3x3 matrix
    //    .q - instruction works on a 4 register vector or 4x4 matrix
    //  
    //  http://wiki.fx-world.org/doku.php?id=general:vfpu_registers
    // 
    //  This is something you need to know about how to transfer data in or out of the vfpu. First lets show the instructions used to load/store data from the vfpu:
    //    lv.s (load 1 vfpu reg from unaligned memory)
    //    lv.q (load 4 vfpu regs from 16 byte aligned memory)
    //    sv.s (write 1 vfpu reg to unaligned memory)
    //    sv.q (write 4 vfpu regs to 16 byte aligned memory)
    // 
    //  There are limitations with these instructions. You can only transfer to or from column or row registers in the vfpu.
    // 
    //  You can also load values into the vfpu from a MIPS register, this will work with all single registers:
    //    mtv (move MIPS register to vfpu register)
    //    mfv (move from vfpu register to MIPS register)
    // 
    //  There are 2 instructions, ulv.q and usv.q, that perform unaligned ran transfers to/from the vfpu. These have been found to be faulty so it is not recommended to use them.
    // 
    //  The vfpu performs a few trig functions, but they dont behave like the normal C functions we are used to.
    //  Normally we would pass in the angle in radians from -pi/2 to +pi/2, but the vfpu wants the input value in the range of -1 to 1.
    // 

    //
    //   The VFPU contains 32 registers (128bits each, 4x32bits).
    //
    //   VFPU Registers can get accessed as Matrices, Vectors or single words.
    //   All registers are overlayed and enumerated in 3 digits (Matrix/Column/Row):
    //
    //	M000 | C000   C010   C020   C030	M100 | C100   C110   C120   C130
    //	-----+--------------------------	-----+--------------------------
    //	R000 | S000   S010   S020   S030	R100 | S100   S110   S120   S130
    //	R001 | S001   S011   S021   S031	R101 | S101   S111   S121   S131
    //	R002 | S002   S012   S022   S032	R102 | S102   S112   S122   S132
    //	R003 | S003   S013   S023   S033	R103 | S103   S113   S123   S133
    //
    //  same for matrices starting at M200 - M700.
    //  Subvectors can get addressed as singles/pairs/triplets/quads.
    //  Submatrices can get addressed 2x2 pairs, 3x3 triplets or 4x4 quads.
    //
    //  So Q_C010 specifies the Quad Column starting at S010, T_C011 the triple Column starting at S011.
    //
    // ReSharper disable UnusedMember.Global
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
        
        
        ////////
        ///
        
         private void _call_debug_vfpu()
        {
            throw (new NotImplementedException("_call_debug_vfpu"));
            //MipsMethodEmitter.CallMethodWithCpuThreadStateAsFirstArgument(this.GetType(), "_debug_vfpu");
        }

        public static void _debug_vfpu(CpuThreadState cpuThreadState)
        {
            Console.Error.WriteLine("");
            Console.Error.WriteLine("VPU DEBUG:");
            fixed (float* fpr = &cpuThreadState.Vfr0)
            {
                var index = 0;
                for (var matrix = 0; matrix < 8; matrix++)
                {
                    Console.Error.WriteLine("Matrix {0}: ", matrix);
                    for (var row = 0; row < 4; row++)
                    {
                        for (var column = 0; column < 4; column++)
                        {
                            Console.Error.Write("{0},", fpr[index]);
                            index++;
                        }
                        Console.Error.WriteLine("");
                    }
                    Console.Error.WriteLine("");
                }
            }
        }

        private void _load_memory_imm14_index(uint index)
        {
            throw (new NotImplementedException("_load_memory_imm14_index"));
            //MipsMethodEmitter._getmemptr(() =>
            //{
            //	MipsMethodEmitter.LoadGPR_Unsigned(RS);
            //	SafeILGenerator.Push((int)(Instruction.IMM14 * 4 + Index * 4));
            //	SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
            //}, Safe: true, CanBeNull: false);
        }

        public VfpuPrefix PrefixNone = new VfpuPrefix();
        public VfpuPrefix PrefixSource = new VfpuPrefix();
        public VfpuPrefix PrefixTarget = new VfpuPrefix();
        public VfpuDestinationPrefix PrefixDestinationNone = new VfpuDestinationPrefix();
        public VfpuDestinationPrefix PrefixDestination = new VfpuDestinationPrefix();

        internal abstract class VfpuRuntimeRegister
        {
            protected uint Pc;
            protected Instruction Instruction;
            protected VReg VReg;
            protected VType VType;
            protected int VectorSize;
            protected Dictionary<int, AstLocal> Locals = new Dictionary<int, AstLocal>();
            private static readonly AstMipsGenerator Ast = AstMipsGenerator.Instance;

            protected AstNodeExprLocal GetLocal(int index)
            {
                if (!Locals.ContainsKey(index)) Locals[index] = AstLocal.Create(GetVTypeType(), "LocalVFPR" + index);
                return Ast.Local(Locals[index]);
            }

            protected VfpuRuntimeRegister(CpuEmitter cpuEmitter, VReg vReg, VType vType, int vectorSize)
            {
                Pc = cpuEmitter._pc;
                Instruction = cpuEmitter._instruction;
                VReg = vReg;
                VType = vType;
                VectorSize = (vectorSize == 0) ? Instruction.OneTwo : vectorSize;
            }

            private Type GetVTypeType()
            {
                switch (VType)
                {
                    case VType.VFloat: return typeof(float);
                    case VType.VInt: return typeof(int);
                    case VType.VuInt: return typeof(uint);
                    default: throw (new InvalidCastException("Invalid VType " + VType));
                }
            }

            protected AstNodeExprLValue _GetVRegRef(int regIndex)
            {
                var toType = GetVTypeType();
                return toType == typeof(float) ? Ast.Vfr(regIndex) : Ast.Reinterpret(toType, Ast.Vfr(regIndex));
            }

            protected AstNodeExpr GetRegApplyPrefix(int[] indices, int regIndex, int prefixIndex)
            {
                var prefix = VReg.VfpuPrefix;
                prefix.CheckPrefixUsage(Pc);

                //Console.WriteLine("[Get] {0:X8}:: {1}", PC, Prefix.Enabled);
                //Console.WriteLine("GetRegApplyPrefix: {0}, {1}", Indices.Length, RegIndex);

                AstNodeExpr astNodeExpr = _GetVRegRef(indices[regIndex]);

                if (prefix.Enabled && prefix.IsValidIndex(prefixIndex))
                {
                    // Constant.
                    if (prefix.SourceConstant(prefixIndex))
                    {
                        float value = 0.0f;
                        var sourceIndex = prefix.SourceIndex(prefixIndex);
                        switch (sourceIndex)
                        {
                            case 0:
                                value = prefix.SourceAbsolute(prefixIndex) ? (3.0f) : (0.0f);
                                break;
                            case 1:
                                value = prefix.SourceAbsolute(prefixIndex) ? (1.0f / 3.0f) : (1.0f);
                                break;
                            case 2:
                                value = prefix.SourceAbsolute(prefixIndex) ? (1.0f / 4.0f) : (2.0f);
                                break;
                            case 3:
                                value = prefix.SourceAbsolute(prefixIndex) ? (1.0f / 6.0f) : (0.5f);
                                break;
                            default: throw (new InvalidOperationException("Invalid SourceIndex : " + sourceIndex));
                        }

                        astNodeExpr = Ast.Cast(GetVTypeType(), value);
                    }
                    // Value.
                    else
                    {
                        astNodeExpr = _GetVRegRef(indices[(int) prefix.SourceIndex(prefixIndex)]);
                        if (prefix.SourceAbsolute(prefixIndex))
                            astNodeExpr = Ast.CallStatic((Func<float, float>) MathFloat.Abs, astNodeExpr);
                    }

                    if (prefix.SourceNegate(prefixIndex)) astNodeExpr = Ast.Unary("-", astNodeExpr);
                }

                return astNodeExpr;
            }

            protected AstNodeStm SetRegApplyPrefix(int regIndex, int prefixIndex, AstNodeExpr astNodeExpr)
            {
                if (astNodeExpr == null) return null;

                var prefixDestination = VReg.VfpuDestinationPrefix;
                prefixDestination.CheckPrefixUsage(Pc);

                //Console.WriteLine("[Set] {0:X8}:: {1}", PC, PrefixDestination.Enabled);

                if (prefixDestination.Enabled && prefixDestination.IsValidIndex(prefixIndex))
                {
                    // It is masked. It won't write the value.
                    float max = 0;
                    float min = 0;
                    if (prefixDestination.DestinationMask(prefixIndex))
                    {
                        //return ast.Statement();
                        astNodeExpr = _GetVRegRef(regIndex);
                    }
                    else
                    {
                        var doClamp = false;
                        switch (prefixDestination.DestinationSaturation(prefixIndex))
                        {
                            case 1:
                                doClamp = true;
                                min = 0.0f;
                                max = 1.0f;
                                break;
                            case 3:
                                doClamp = true;
                                min = -1.0f;
                                max = 1.0f;
                                break;
                        }

                        if (doClamp)
                        {
                            if (VType == CpuEmitter.VType.VFloat)
                            {
                                astNodeExpr = Ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp,
                                    astNodeExpr, (float) min, (float) max);
                            }
                            else
                            {
                                astNodeExpr = Ast.Cast(GetVTypeType(),
                                    Ast.CallStatic((Func<int, int, int, int>) MathFloat.ClampInt, astNodeExpr,
                                        (int) min, (int) max));
                            }
                        }
                    }
                }

                //Console.Error.WriteLine("PrefixIndex:{0}", PrefixIndex);
                return Ast.Assign(GetLocal(regIndex), Ast.Cast(GetVTypeType(), astNodeExpr));
            }

            protected AstNodeStm SetRegApplyPrefix2(int regIndex, int prefixIndex, uint pc, string calledFrom)
            {
                return Ast.Statements(
#if CHECK_VFPU_REGISTER_SET
					ast.Statement(ast.CallStatic((Action<string, int, uint, float>)CheckVfpuRegister, CalledFrom, RegIndex, PC, ast.Cast<float>(GetLocal(RegIndex)))),
#endif
                    Ast.Assign(_GetVRegRef(regIndex), GetLocal(regIndex))
                );
            }

            public static void CheckVfpuRegister(string opcode, int regIndex, uint pc, float value)
            {
                //if (float.IsNaN(Value))
                {
                    Console.WriteLine("VFPU_SET:{0:X4}:{1}:VR{2}:{3:X8}:{4},", pc, opcode, regIndex,
                        MathFloat.ReinterpretFloatAsUInt(value), value);
                }
            }
        }

        internal sealed class VfpuCell : VfpuRuntimeRegister
        {
            private int _index;

            public VfpuCell(CpuEmitter cpuEmitter, VReg vReg, VType vType)
                : base(cpuEmitter, vReg, vType, 1)
            {
                _index = VfpuUtils.GetIndexCell(vReg.Reg);
            }

            public AstNodeExpr Get() => GetRegApplyPrefix(new[] {_index}, 0, 0);

            public AstNodeStm Set(AstNodeExpr value, uint pc, [CallerMemberName] string calledFrom = "") =>
                _ast.Statements(
                    SetRegApplyPrefix(_index, 0, value),
                    SetRegApplyPrefix2(_index, 0, pc, calledFrom)
                );
        }

        internal sealed class VfpuVector : VfpuRuntimeRegister
        {
            private int[] _indices;

            public VfpuVector(CpuEmitter cpuEmitter, VReg vReg, VType vType, int vectorSize)
                : base(cpuEmitter, vReg, vType, vectorSize)
            {
                _indices = VfpuUtils.GetIndicesVector(VectorSize, vReg.Reg);
            }

            public AstNodeExpr this[int index] => Get(index);
            public AstNodeExprLValue GetIndexRef(int index) => _GetVRegRef(_indices[index]);
            public AstNodeExpr Get(int index) => GetRegApplyPrefix(_indices, index, index);
            public AstNodeStm Set(int index, AstNodeExpr value) => SetRegApplyPrefix(_indices[index], index, value);

            public AstNodeStm Set2(int index, uint pc, [CallerMemberName] string calledFrom = "") =>
                SetRegApplyPrefix2(this._indices[index], index, pc, calledFrom);

            public AstNodeStm SetVector(Func<int, AstNodeExpr> generator, uint pc,
                [CallerMemberName] string calledFrom = "")
            {
                return _ast.Statements(
                    _ast.Statements(Enumerable.Range(0, VectorSize).Select(index => Set(index, generator(index)))
                        .Where(statement => statement != null)),
                    _ast.StatementsInline(Enumerable.Range(0, VectorSize)
                        .Select(index => Set2(index, pc, calledFrom)).Where(statement => statement != null))
                );
            }
        }

        internal sealed class VFpuVectorRef
        {
            private AstNodeExprLValue[] _refs;

            public VFpuVectorRef(params AstNodeExprLValue[] refs) => _refs = refs;

            public int VectorSize => _refs.Length;

            public AstNodeStm SetVector(Func<int, AstNodeExpr> generator)
            {
                return _ast.Statements(Enumerable.Range(0, VectorSize)
                    .Select(index => _ast.Assign(this[index], generator(index))));
            }

            public AstNodeExprLValue this[int index] => _refs[index];

            public static VFpuVectorRef Generate(int vectorSize, Func<int, AstNodeExprLValue> callback)
            {
                return new VFpuVectorRef(Enumerable.Range(0, vectorSize).Select(callback).ToArray());
            }
        }

        internal sealed class VfpuMatrix : VfpuRuntimeRegister
        {
            private int[,] _indices;

            public VfpuMatrix(CpuEmitter cpuEmitter, VReg vReg, VType vType, int vectorSize)
                : base(cpuEmitter, vReg, vType, vectorSize)
            {
                _indices = VfpuUtils.GetIndicesMatrix(this.VectorSize, vReg.Reg);
            }

            public AstNodeExpr this[int column, int row] => Get(column, row);

            public AstNodeExprLValue GetIndexRef(int column, int row) => _GetVRegRef(_indices[column, row]);

            private AstNodeExpr Get(int column, int row) => _GetVRegRef(this._indices[column, row]);

            private int GetPrefixIndex(int column, int row)
            {
                //return 0;
                return -1;
                //return Row;
                //return Row * 4 + Column;
                //return Column;
            }

            public AstNodeStm Set(int column, int row, AstNodeExpr value)
            {
                return SetRegApplyPrefix(_indices[column, row], GetPrefixIndex(column, row), value);
            }

            public AstNodeStm Set2(int column, int row, uint pc, [CallerMemberName] string calledFrom = "")
            {
                return SetRegApplyPrefix2(_indices[column, row], GetPrefixIndex(column, row), pc, calledFrom);
            }

            public AstNodeStm SetMatrix(Func<int, int, AstNodeExpr> generator, uint pc,
                [CallerMemberName] string calledFrom = "")
            {
                var statements = new List<AstNodeStm>();

                for (var row = 0; row < VectorSize; row++)
                for (var column = 0; column < VectorSize; column++)
                {
                    statements.Add(Set(column, row, generator(column, row)));
                }

                for (var row = 0; row < VectorSize; row++)
                for (var column = 0; column < VectorSize; column++) statements.Add(Set2(column, row, pc, calledFrom));
                return _ast.Statements(statements);
            }
        }

        public class VReg
        {
            public VfpuRegisterInt Reg;
            public VfpuDestinationPrefix VfpuDestinationPrefix;
            public VfpuPrefix VfpuPrefix;
        }

        public enum VType
        {
            VFloat,
            VInt,
            VuInt,
        }

        private VType VInt => VType.VInt;
        private VType VuInt => VType.VuInt;
        private VType VFloat => VType.VFloat;

        private VReg Vd => new VReg
        {
            Reg = _instruction.Vd,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestination
        };

        private VReg Vs => new VReg
        {
            Reg = _instruction.Vs,
            VfpuPrefix = PrefixSource,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg Vt => new VReg
        {
            Reg = _instruction.Vt,
            VfpuPrefix = PrefixTarget,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg Vt51 => new VReg
        {
            Reg = _instruction.Vt51,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg Vt52 => new VReg
        {
            Reg = _instruction.Vt52,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg VdNoPrefix => new VReg
        {
            Reg = _instruction.Vd,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg VsNoPrefix => new VReg
        {
            Reg = _instruction.Vs,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg VtNoPrefix => new VReg
        {
            Reg = _instruction.Vt,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VFpuVectorRef _MemoryVectorIMM14<TType>(int vectorSize)
        {
            var elementSize = Marshal.SizeOf(typeof(TType));
            return VFpuVectorRef.Generate(vectorSize,
                index => _ast.MemoryGetPointerRef<TType>(_memory, Address_RS_IMM14(index * elementSize)));
        }

        private VfpuCell _Cell(VReg vReg, VType vType = VType.VFloat) => new VfpuCell(this, vReg, vType);

        private VfpuVector _Vector(VReg vReg, VType vType = VType.VFloat, int size = 0) =>
            new VfpuVector(this, vReg, vType, size);

        private VfpuMatrix _Matrix(VReg vReg, VType vType = VType.VFloat, int size = 0) =>
            new VfpuMatrix(this, vReg, vType, size);

        private VfpuMatrix Mat(VReg vReg, VType vType = VType.VFloat, int size = 0) =>
            new VfpuMatrix(this, vReg, vType, size);

        private VfpuVector Vec(VReg vReg, VType vType = VType.VFloat, int size = 0) =>
            new VfpuVector(this, vReg, vType, size);

        private VfpuCell Cel(VReg vReg, VType vType = VType.VFloat) => new VfpuCell(this, vReg, vType);
        private VfpuMatrix MatVs => Mat(Vs);
        private VfpuMatrix MatVd => Mat(Vd);
        private VfpuMatrix MatVt => Mat(Vt);
        private VfpuVector VecVs => Vec(Vs);
        private VfpuVector VecVd => Vec(Vd);
        private VfpuVector VecVt => Vec(Vt);
        private VfpuVector VecVsI => Vec(Vs, VType.VInt);
        private VfpuVector VecVdI => Vec(Vd, VType.VInt);
        private VfpuVector VecVtI => Vec(Vt, VType.VInt);
        private VfpuVector VecVsU => Vec(Vs, VType.VuInt);
        private VfpuVector VecVdU => Vec(Vd, VType.VuInt);
        private VfpuVector VecVtU => Vec(Vt, VType.VuInt);
        private VfpuCell CelVs => Cel(Vs);
        private VfpuCell CelVd => Cel(Vd);
        private VfpuCell CelVt => Cel(Vt);
        private VfpuCell CelVtNoPrefix => Cel(VtNoPrefix);
        private VfpuCell CelVsI => Cel(Vs, VType.VInt);
        private VfpuCell CelVdI => Cel(Vd, VType.VInt);
        private VfpuCell CelVtI => Cel(Vt, VType.VInt);
        private VfpuCell CelVtINoPrefix => Cel(VtNoPrefix, VType.VInt);
        private VfpuCell CelVsU => Cel(Vs, VType.VuInt);
        private VfpuCell CelVdU => Cel(Vd, VType.VuInt);
        private VfpuCell CelVtU => Cel(Vt, VType.VuInt);
        private VfpuCell CelVtUNoPrefix => Cel(VtNoPrefix, VType.VuInt);

        private AstNodeExpr _Aggregate(AstNodeExpr first, Func<AstNodeExpr, int, AstNodeExpr> callback) =>
            _Aggregate(first, OneTwo, callback);

        private AstNodeStmContainer _List(Func<int, AstNodeStm> callback) => _List(OneTwo, callback);

        private AstNodeStmContainer _List(int vectorSize, Func<int, AstNodeStm> callback)
        {
            var statements = _ast.Statements();
            for (var index = 0; index < vectorSize; index++)
                statements.AddStatement(callback(index));
            return statements;
        }

        private AstNodeExpr _Aggregate(AstNodeExpr first, int vectorSize, Func<AstNodeExpr, int, AstNodeExpr> callback)
        {
            var value = first;
            for (var index = 0; index < vectorSize; index++)
                value = callback(value, index);
            return value;
        }
    }
}