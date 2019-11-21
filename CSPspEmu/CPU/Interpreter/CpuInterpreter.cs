using System;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.CPU.Interpreter
{
    // ReSharper disable UnusedMember.Global
    // http://www.mrc.uidaho.edu/mrc/people/jff/digital/MIPSir.html
    public class CpuInterpreter
    {
        public Instruction i;
        public CpuThreadState state;
        public Action<uint, CpuInterpreter> Switch = CpuInterpreterSwitchGenerator.Switch;

        public CpuInterpreter(CpuThreadState state)
        {
            this.state = state;
        }

        public void ExecuteStep()
        {
            Interpret(state.Memory.Read4(state.Pc));
        }

        public void Interpret(Instruction i)
        {
            this.i = i;
            Switch(i, this);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////

        [InstructionName(InstructionNames.Add)]
        public void Add() => AdvancePC().SetRD(RS + RT);

        [InstructionName(InstructionNames.Addu)]
        public void Addu() => Add();

        [InstructionName(InstructionNames.Unknown)]
        public void Default() => throw new Exception("Not implemented");

        [InstructionName(InstructionNames.Sub)]
        public void Sub() => AdvancePC().SetRD(RS - RT);

        [InstructionName(InstructionNames.Subu)]
        public void Subu() => Sub();

        [InstructionName(InstructionNames.Addi)]
        public void Addi() => AdvancePC().SetRT(RS + IMM_s);

        [InstructionName(InstructionNames.Addiu)]
        public void Addiu() => Addi();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Logical Operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.And)]
        public void And() => AdvancePC().SetRD(RS & RT);

        [InstructionName(InstructionNames.Or)]
        public void Or() => AdvancePC().SetRD(RS | RT);

        [InstructionName(InstructionNames.Xor)]
        public void Xor() => AdvancePC().SetRD(RS ^ RT);

        [InstructionName(InstructionNames.Nor)]
        public void Nor() => AdvancePC().SetRD(~(RS | RT));

        [InstructionName(InstructionNames.Andi)]
        public void Andi() => AdvancePC().SetRT(RS_u & IMM_u);

        [InstructionName(InstructionNames.Ori)]
        public void Ori() => AdvancePC().SetRT(RS_u | IMM_u);

        [InstructionName(InstructionNames.Xori)]
        public void Xori() => AdvancePC().SetRT(RS_u ^ IMM_u);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Shift Left/Right Logical/Arithmethic (Variable).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Sll)]
        public void Sll() => AdvancePC().SetRD(RT_u << Pos);

        [InstructionName(InstructionNames.Sra)]
        public void Sra() => AdvancePC().SetRD(RT >> Pos);

        [InstructionName(InstructionNames.Srl)]
        public void Srl() => AdvancePC().SetRD(RT_u >> Pos);

        [InstructionName(InstructionNames.Rotr)]
        public void Rotr() => AdvancePC().SetRD(CpuEmitterUtils._rotr_impl(RT_u, Pos));

        [InstructionName(InstructionNames.Sllv)]
        public void Sllv() => AdvancePC().SetRD(RT_u << (int) (RS_u & 31));

        [InstructionName(InstructionNames.Srav)]
        public void Srav() => AdvancePC().SetRD(RT >> (int) (RS_u & 31));

        [InstructionName(InstructionNames.Srlv)]
        public void Srlv() => AdvancePC().SetRD(RT_u >> (int) (RS_u & 31));

        [InstructionName(InstructionNames.Rotrv)]
        public void Rotrv() => AdvancePC().SetRD(CpuEmitterUtils._rotr_impl(RT_u, RS));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Set Less Than (Immediate) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Slt)]
        public void Slt() => AdvancePC().SetRD(RS < RT);

        [InstructionName(InstructionNames.Sltu)]
        public void Sltu() => AdvancePC().SetRD(RS_u < RT_u);

        [InstructionName(InstructionNames.Slti)]
        public void Slti() => AdvancePC().SetRT(RS_u < IMM_s);

        [InstructionName(InstructionNames.Sltiu)]
        public void Sltiu() => AdvancePC().SetRT(RS_u < IMM_uex);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Upper Immediate.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Lui)]
        public void Lui() => AdvancePC().SetRT(IMM_u << 16);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Sign Extend Byte/Half word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Seb)]
        public void Seb() => AdvancePC().SetRD((sbyte) RT);

        [InstructionName(InstructionNames.Seh)]
        public void Seh() => AdvancePC().SetRD((short) RT);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // BIT REVerse.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Bitrev)]
        public void Bitrev() => AdvancePC().SetRD(CpuEmitterUtils._bitrev_impl(RT_u));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MAXimum/MINimum.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Max)]
        public void Max() => AdvancePC().SetRD(Math.Max(RS, RT));

        [InstructionName(InstructionNames.Min)]
        public void Min() => AdvancePC().SetRD(Math.Min(RS, RT));


        /////////////////////////////////////////////////////////////////////////////////////////////////
        // DIVide (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Div)]
        public void Div()
        {
            AdvancePC();
            CpuEmitterUtils._div_impl(state, RS, RT);
        }

        [InstructionName(InstructionNames.Divu)]
        public void Divu()
        {
            AdvancePC();
            CpuEmitterUtils._divu_impl(state, RS_u, RT_u);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MULTiply (ADD/SUBstract) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mult)]
        public void Mult() => AdvancePC().SetHiLo(RS_l * RT_l);

        [InstructionName(InstructionNames.Multu)]
        public void Multu() => AdvancePC().SetHiLo(RS_ul * RT_ul);

        [InstructionName(InstructionNames.Madd)]
        public void Madd() => AdvancePC().SetHiLo(HI_LO_s + (RS_l * RT_l));

        [InstructionName(InstructionNames.Maddu)]
        public void Maddu() => AdvancePC().SetHiLo(HI_LO_u + (RS_ul * RT_ul));

        [InstructionName(InstructionNames.Msub)]
        public void Msub() => AdvancePC().SetHiLo(HI_LO_s - (RS_l * RT_l));

        [InstructionName(InstructionNames.Msubu)]
        public void Msubu() => AdvancePC().SetHiLo(HI_LO_u - (RS_ul * RT_ul));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move To/From HI/LO.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfhi)]
        public void Mfhi() => AdvancePC().SetRD(state.Hi);

        [InstructionName(InstructionNames.Mflo)]
        public void Mflo() => AdvancePC().SetRD(state.Lo);

        [InstructionName(InstructionNames.Mthi)]
        public void Mthi() => AdvancePC().SetHI(RS);

        [InstructionName(InstructionNames.Mtlo)]
        public void Mtlo() => AdvancePC().SetLO(RS);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move if Zero/Non zero.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Movz)]
        public void Movz()
        {
            AdvancePC();
            if (RT == 0) SetRD(RS);
        }

        [InstructionName(InstructionNames.Movn)]
        public void Movn()
        {
            AdvancePC();
            if (RT != 0) SetRD(RS);
        }

        /// <summary>
        /// EXTract/INSert
        /// </summary>
        [InstructionName(InstructionNames.Ext)]
        public void Ext()
        {
            AdvancePC();
            SetRT(CpuEmitterUtils._ext_impl(RS_u, (int) i.Pos, (int) i.SizeE));
        }

        [InstructionName(InstructionNames.Ins)]
        public void Ins()
        {
            AdvancePC();
            SetRT(CpuEmitterUtils._ins_impl(RT_u, RS_u, (int) i.Pos, (int) i.SizeI));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Count Leading Ones/Zeros in word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Clz)]
        public void Clz() => AdvancePC().SetRD(CpuEmitterUtils._clz_impl(RS_u));

        [InstructionName(InstructionNames.Clo)]
        public void Clo() => AdvancePC().SetRD(CpuEmitterUtils._clo_impl(RS_u));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Word Swap Bytes Within Halfwords/Words.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Wsbh)]
        public void Wsbh() => AdvancePC().SetRD(CpuEmitterUtils._wsbh_impl(RT_u));

        [InstructionName(InstructionNames.Wsbw)]
        public void Wsbw() => AdvancePC().SetRD(CpuEmitterUtils._wsbw_impl(RT_u));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move Control (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Cfc0)]
        public void Cfc0() => throw new Exception($"Unimplemented cfc0 : {Rt}, {Rd}");

        [InstructionName(InstructionNames.Ctc0)]
        public void Ctc0() => throw new Exception($"Unimplemented ctc0 : {Rt}, {Rd}");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfc0)]
        public void Mfc0() => AdvancePC().SetRT(state.C0R[Rd]);

        [InstructionName(InstructionNames.Mtc0)]
        public void Mtc0()
        {
            AdvancePC();
            state.C0R[Rd] = RT_u;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Byte/Half word/Word (Left/Right/Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Lb)]
        public void Lb() => AdvancePC().SetRT((sbyte) state.Memory.Read1(RS_Imm));

        [InstructionName(InstructionNames.Lbu)]
        public void Lbu() => AdvancePC().SetRT((byte) state.Memory.Read1(RS_Imm));

        [InstructionName(InstructionNames.Lh)]
        public void Lh() => AdvancePC().SetRT((short) state.Memory.Read2(RS_Imm));

        [InstructionName(InstructionNames.Lhu)]
        public void Lhu() => AdvancePC().SetRT((ushort) state.Memory.Read2(RS_Imm));

        [InstructionName(InstructionNames.Lw)]
        public void Lw() => AdvancePC().SetRT(state.Memory.Read4(RS_Imm));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Store Byte/Half word/Word (Left/Right).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Sb)]
        public void Sb() => AdvancePC().state.Memory.Write1(RS_Imm, (byte) RT_u);

        [InstructionName(InstructionNames.Sh)]
        public void Sh() => AdvancePC().state.Memory.Write2(RS_Imm, (ushort) RT_u);

        [InstructionName(InstructionNames.Sw)]
        public void Sw() => AdvancePC().state.Memory.Write4(RS_Imm, (ushort) RT_u);

        [InstructionName(InstructionNames.Lwl)]
        public void Lwl() => AdvancePC().SetRT(CpuEmitterUtils._lwl_exec(state, RS_u, IMM_s, RT_u));

        [InstructionName(InstructionNames.Lwr)]
        public void Lwr() => AdvancePC().SetRT(CpuEmitterUtils._lwr_exec(state, RS_u, IMM_s, RT_u));

        [InstructionName(InstructionNames.Swl)]
        public void Swl()
        {
            AdvancePC();
            CpuEmitterUtils._swl_exec(state, RS_u, IMM_s, RT_u);
        }

        [InstructionName(InstructionNames.Swr)]
        public void Swr()
        {
            AdvancePC();
            CpuEmitterUtils._swr_exec(state, RS_u, IMM_s, RT_u);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Linked word.
        // Store Conditional word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Ll)]
        public void Ll() => Lw();

        [InstructionName(InstructionNames.Sc)]
        public void Sc()
        {
            Sw();
            SetRT(1);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Syscall
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Syscall)]
        public void Syscall()
        {
            if (i.Code == SyscallInfo.NativeCallSyscallCode)
            {
                var delegateId = state.Memory.Read4(state.Pc + 4);
                AdvancePC();
                state.SyscallNative(delegateId);
            }
            else
            {
                AdvancePC();
                state.Syscall((int) i.Code);
            }
        }

        [InstructionName(InstructionNames.Cache)]
        public void Cache()
        {
            CpuEmitterUtils._cache_impl(state, state.Pc, i.Value);
            AdvancePC();
        }

        [InstructionName(InstructionNames.Sync)]
        public void Sync()
        {
            CpuEmitterUtils._sync_impl(state, state.Pc, i.Value);
            AdvancePC();
        }

        [InstructionName(InstructionNames.Break)]
        public void Break()
        {
            CpuEmitterUtils._break_impl(state, state.Pc, i.Value);
            AdvancePC();
        }

        [InstructionName(InstructionNames.Dbreak)]
        public void Dbreak() => throw new NotImplementedException("dbreak");

        [InstructionName(InstructionNames.Halt)]
        public void Halt() => throw new NotImplementedException("halt");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // (D?/Exception) RETurn
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Dret)]
        public void Dret() => throw new NotImplementedException("dret");

        [InstructionName(InstructionNames.Eret)]
        public void Eret() => throw new NotImplementedException("eret");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) IC
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfic)]
        public void Mfic() => AdvancePC().SetRT(state.Ic);

        [InstructionName(InstructionNames.Mtic)]
        public void Mtic()
        {
            AdvancePC();
            state.Ic = RT_u;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) DR
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfdr)]
        public void Mfdr() => throw new NotImplementedException("mfdr");

        [InstructionName(InstructionNames.Mtdr)]
        public void Mtdr() => throw new NotImplementedException("mtdr");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // beq(l)     : Branch on EQuals (Likely).
        // bne(l)     : Branch on Not Equals (Likely).
        // btz(al)(l) : Branch on Less Than Zero (And Link) (Likely).
        // blez(l)    : Branch on Less Or Equals than Zero (Likely).
        // bgtz(l)    : Branch on Great Than Zero (Likely).
        // bgez(al)(l): Branch on Greater Equal Zero (And Link) (Likely).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Beq)]
        public void Beq() => Branch(RS_s == RT_s);

        [InstructionName(InstructionNames.Beql)]
        public void Beql() => Branch(RS_s == RT_s, likely: true);

        [InstructionName(InstructionNames.Bne)]
        public void Bne() => Branch(RS_s != RT_s);

        [InstructionName(InstructionNames.Bnel)]
        public void Bnel() => Branch(RS_s != RT_s, likely: true);

        [InstructionName(InstructionNames.Bltz)]
        public void Bltz() => Branch(RS_s < 0);

        [InstructionName(InstructionNames.Bltzl)]
        public void Bltzl() => Branch(RS_s < 0, likely: true);

        [InstructionName(InstructionNames.Bltzal)]
        public void Bltzal() => Branch(RS_s < 0, link: true);

        [InstructionName(InstructionNames.Bltzall)]
        public void Bltzall() => Branch(RS_s < 0, likely: true, link: true);

        [InstructionName(InstructionNames.Blez)]
        public void Blez() => Branch(RS_s <= 0);

        [InstructionName(InstructionNames.Blezl)]
        public void Blezl() => Branch(RS_s <= 0, likely: true);

        [InstructionName(InstructionNames.Bgtz)]
        public void Bgtz() => Branch(RS_s > 0);

        [InstructionName(InstructionNames.Bgtzl)]
        public void Bgtzl() => Branch(RS_s > 0, likely: true);

        [InstructionName(InstructionNames.Bgez)]
        public void Bgez() => Branch(RS_s >= 0);

        [InstructionName(InstructionNames.Bgezl)]
        public void Bgezl() => Branch(RS_s >= 0, likely: true);

        [InstructionName(InstructionNames.Bgezal)]
        public void Bgezal() => Branch(RS_s >= 0, link: true);

        [InstructionName(InstructionNames.Bgezall)]
        public void Bgezall() => Branch(RS_s >= 0, likely: true, link: true);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // j(al)(r): Jump (And Link) (Register)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.J)]
        public void J() => Jump(JumpAddr);

        [InstructionName(InstructionNames.Jal)]
        public void Jal() => Jump(JumpAddr, link: true);

        [InstructionName(InstructionNames.Jr)]
        public void Jr() => Jump(RS_u);

        [InstructionName(InstructionNames.Jalr)]
        public void Jalr() => Jump(RS_u, link: true);

                /////////////////////////////////////////////////////////////////////////////////////////////////
        // Binary Floating Point Unit Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.AddS)]
        public void add_s() => AdvancePC().SetFD(FS + FT);

        [InstructionName(InstructionNames.SubS)]
        public void sub_s() => AdvancePC().SetFD(FS - FT);

        [InstructionName(InstructionNames.MulS)]
        public void mul_s() => AdvancePC().SetFD(FS * FT);

        [InstructionName(InstructionNames.DivS)]
        public void div_s() => AdvancePC().SetFD(FS / FT);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Unary Floating Point Unit Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.SqrtS)]
        public void sqrt_s() => AdvancePC().SetFD(MathFloat.Sqrt(FS));

        [InstructionName(InstructionNames.AbsS)]
        public void abs_s() => AdvancePC().SetFD(MathFloat.Abs(FS));

        [InstructionName(InstructionNames.MovS)]
        public void mov_s() => AdvancePC().SetFD(FS);

        [InstructionName(InstructionNames.NegS)]
        public void neg_s() => AdvancePC().SetFD(-FS);

        [InstructionName(InstructionNames.TruncWS)]
        public void trunc_w_s() => AdvancePC().SetFD_I(MathFloat.Cast(FS));

        [InstructionName(InstructionNames.RoundWS)]
        public void round_w_s() => AdvancePC().SetFD_I(MathFloat.Round(FS));

        [InstructionName(InstructionNames.CeilWS)]
        public void ceil_w_s() => AdvancePC().SetFD_I(MathFloat.Ceil(FS));

        [InstructionName(InstructionNames.FloorWS)]
        public void floor_w_s() => AdvancePC().SetFD_I(MathFloat.Floor(FS));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Convert FS register (stored as an int) to float and stores the result on FD.
        // Floating-Point Convert to Word Fixed-Point
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.CvtSW)]
        public void cvt_s_w() => AdvancePC().SetFD(FS_I);

        [InstructionName(InstructionNames.CvtWS)]
        public void cvt_w_s() => AdvancePC().SetFD_I(CpuEmitterUtils._cvt_w_s_impl(state, FS));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (from/to) float point registers (reinterpreted)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfc1)]
        public void Mfc1() => AdvancePC().SetRT(FS_I);

        [InstructionName(InstructionNames.Mtc1)]
        public void Mtc1() => AdvancePC().SetFpr_I(Fs, RT);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Word to Cop1 floating point.
        // Store Word from Cop1 floating point.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Lwc1)]
        public void Lwc1() => AdvancePC().SetFpr_I(Ft, (int)state.Memory.Read4(RS_Imm));

        [InstructionName(InstructionNames.Swc1)]
        public void Swc1() => AdvancePC().state.Memory.Write4(RS_Imm, (uint)FT_I);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // CFC1 -- move Control word from/to floating point (C1)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Cfc1)]
        public void Cfc1()
        {
            CpuEmitterUtils._cfc1_impl(state, Rd, Rt);
            AdvancePC();
        }

        [InstructionName(InstructionNames.Ctc1)]
        public void Ctc1()
        {
            CpuEmitterUtils._ctc1_impl(state, Rd, Rt);
            AdvancePC();
        }

        /// <summary>
        /// Compare (condition) Single_
        /// </summary>
        /// <param name="fc02"></param>
        /// <param name="fc3"></param>
        private void _comp(int fc02, int fc3)
        {
            var fcUnordererd = (fc02 & 1) != 0;
            var fcEqual = (fc02 & 2) != 0;
            var fcLess = (fc02 & 4) != 0;
            var fcInvQnan = fc3 != 0; // TODO -- Only used for detecting invalid operations?

            var s = FS;
            var t = FT;

            if (float.IsNaN(s) || float.IsNaN(t))
            {
            	state.Fcr31.Cc = fcUnordererd;
            }
            else
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                var equal = (fcEqual) && (s == t);
            	var less = (fcLess) && (s < t);
            	state.Fcr31.Cc = (less || equal);
            }

            AdvancePC();
        }

        //private AstNodeStm _comp_assign(AstNodeExpr Value)
        //{
        //	return ast.Assign();
        //}

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // c.f.s: Compare False Single : This predicate is always False it never has a True Result
        // c.un.s: Compare UNordered Single
        // c.eq.s: Compare EQual Single
        // c.ueq.s: Compare Unordered or EQual Single
        // c.olt.s: Compare Ordered or Less Than Single
        // c.ult.s: Compare Unordered or Less Than Single
        // c.ole.s: Compare Ordered or Less than or Equal Single
        // c.ule.s: Compare Unordered or Less than or Equal Single
        // c.sf.s: Compare Signaling False Single : This predicate always False
        // c.ngle.s: Compare Non Greater Than or Less than or Equal Single
        // c.seq.s: Compare Signaling Equal Single
        // c.ngl.s: Compare Not Greater than or Less than Single
        // c.lt.s: Compare Less Than Single
        // c.nge.s: Compare Not Greater than or Equal Single
        // c.le.s: Compare Less than or Equal Single
        // c.ngt.s: Compare Not Greater Than Single
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.CFS)]
        public void c_f_s() => _comp(0, 0);

        [InstructionName(InstructionNames.CUnS)]
        public void c_un_s() => _comp(1, 0);

        [InstructionName(InstructionNames.CEqS)]
        public void c_eq_s() => _comp(2, 0);

        [InstructionName(InstructionNames.CUeqS)]
        public void c_ueq_s() => _comp(3, 0);

        [InstructionName(InstructionNames.COltS)]
        public void c_olt_s() => _comp(4, 0);

        [InstructionName(InstructionNames.CUltS)]
        public void c_ult_s() => _comp(5, 0);

        [InstructionName(InstructionNames.COleS)]
        public void c_ole_s() => _comp(6, 0);

        [InstructionName(InstructionNames.CUleS)]
        public void c_ule_s() => _comp(7, 0);

        [InstructionName(InstructionNames.CSfS)]
        public void c_sf_s() => _comp(0, 1);

        [InstructionName(InstructionNames.CNgleS)]
        public void c_ngle_s() => _comp(1, 1);

        [InstructionName(InstructionNames.CSeqS)]
        public void c_seq_s() => _comp(2, 1);

        [InstructionName(InstructionNames.CNglS)]
        public void c_ngl_s() => _comp(3, 1);

        [InstructionName(InstructionNames.CLtS)]
        public void c_lt_s() => _comp(4, 1);

        [InstructionName(InstructionNames.CNgeS)]
        public void c_nge_s() => _comp(5, 1);

        [InstructionName(InstructionNames.CLeS)]
        public void c_le_s() => _comp(6, 1);

        [InstructionName(InstructionNames.CNgtS)]
        public void c_ngt_s() => _comp(7, 1);
        
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // bc1(f/t)(l): Branch on C1 (False/True) (Likely)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Bc1F)]
        public void Bc1F() => Branch(!state.Fcr31.Cc);

        [InstructionName(InstructionNames.Bc1Fl)]
        public void Bc1Fl() => Branch(!state.Fcr31.Cc, likely: true);

        [InstructionName(InstructionNames.Bc1T)]
        public void Bc1T() => Branch(state.Fcr31.Cc);

        [InstructionName(InstructionNames.Bc1Tl)]
        public void Bc1Tl() => Branch(state.Fcr31.Cc, likely: true);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Tools
        /////////////////////////////////////////////////////////////////////////////////////////////////

        #region Tools

        private void Jump(uint addr, bool link = false)
        {
            var old = state.nPC;
            state.Pc = state.nPC;
            state.nPC = addr;
            if (link)
            {
                state.Ra = old;
            }
        }

        private void Branch(bool condition, bool likely = false, bool link = false)
        {
            if (condition)
            {
                if (link)
                {
                    state.Ra = state.nPC + 4;
                }

                var next = BranchAddr;
                state.Pc = state.nPC;
                state.nPC = next;
            }
            else
            {
                if (likely)
                {
                    state.Pc = state.nPC + 4;
                    state.nPC = state.Pc + 4;
                }
                else
                {
                    AdvancePC(4);
                }
            }
        }

        private CpuThreadState.GprList Gpr => state.Gpr;
        private CpuThreadState.FprList Fpr => state.Fpr;
        private CpuThreadState.FprListInteger Fpr_I => state.FprI;
        private int Rd => i.Rd;
        private int Rs => i.Rs;
        private int Rt => i.Rt;

        private int Fd => i.Fd;
        private int Fs => i.Fs;
        private int Ft => i.Ft;

        private int RD => Gpr[Rd];
        private int RS => Gpr[Rs];
        private int RT => Gpr[Rt];

        private float FD => Fpr[Fd];
        private float FS => Fpr[Fs];
        private float FT => Fpr[Ft];

        private int FD_I => Fpr_I[Fd];
        private int FS_I => Fpr_I[Fs];
        private int FT_I => Fpr_I[Ft];

        private uint JumpAddr => i.GetJumpAddress(null, state.Pc);
        private uint BranchAddr => i.GetBranchAddress(state.Pc);

        private int RD_s => Gpr[Rd];
        private int RS_s => Gpr[Rs];
        private int RT_s => Gpr[Rt];

        private uint RS_Imm => (uint) (RS_u + i.Imm);

        private long RD_l => (long) Gpr[Rd];
        private long RS_l => (long) Gpr[Rs];
        private long RT_l => (long) Gpr[Rt];

        private ulong RD_ul => (ulong) Gpr[Rd];
        private ulong RS_ul => (ulong) Gpr[Rs];
        private ulong RT_ul => (ulong) Gpr[Rt];

        private uint RD_u => (uint) Gpr[Rd];
        private uint RS_u => (uint) Gpr[Rs];
        private uint RT_u => (uint) Gpr[Rt];

        private long HI_LO_s => state.HiLo;
        private ulong HI_LO_u => (ulong) state.HiLo;

        private uint Pos_u => (uint) i.Pos;
        private int Pos => (int) i.Pos;

        private int IMM_s => i.Imm;
        private uint IMM_u => i.Immu;
        private uint IMM_uex => (uint) i.Imm;

        private CpuInterpreter AdvancePC(int incr = +4)
        {
            state.Pc = state.nPC;
            state.nPC += (uint) incr;
            return this;
        }

        private CpuInterpreter SetGpr(int idx, int value)
        {
            Gpr[idx] = value;
            return this;
        }

        private CpuInterpreter SetFpr(int idx, float value)
        {
            Fpr[idx] = value;
            return this;
        }

        private CpuInterpreter SetFpr_I(int idx, int value)
        {
            Fpr_I[idx] = value;
            return this;
        }


        private CpuInterpreter SetHiLo(long value)
        {
            state.HiLo = value;
            return this;
        }

        private CpuInterpreter SetHiLo(ulong value) => SetHiLo((long) value);

        private CpuInterpreter SetRD(bool value) => SetRD(value ? 1 : 0);
        private CpuInterpreter SetRT(bool value) => SetRT(value ? 1 : 0);

        private CpuInterpreter SetRD(int value) => SetGpr(Rd, value);
        private CpuInterpreter SetRT(int value) => SetGpr(Rt, value);

        private CpuInterpreter SetRD(uint value) => SetGpr(Rd, (int) value);
        private CpuInterpreter SetRT(uint value) => SetGpr(Rt, (int) value);
        
        private CpuInterpreter SetFD(float value) => SetFpr(Fd, value);
        private CpuInterpreter SetFD_I(int value) => SetFpr_I(Fd, value);

        private CpuInterpreter SetHI(int value)
        {
            state.Hi = value;
            return this;
        }

        private CpuInterpreter SetLO(int value)
        {
            state.Lo = value;
            return this;
        }

        #endregion
    }
}