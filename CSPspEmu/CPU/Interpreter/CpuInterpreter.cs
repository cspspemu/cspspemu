using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.CPU.Interpreter
{
    // ReSharper disable UnusedMember.Global
    public class CpuInterpreter
    {
        public Instruction i;
        public CpuThreadState state;
        public Action<uint, CpuInterpreter> Switch = CpuInterpreterSwitchGenerator.Switch;

        public CpuInterpreter(CpuThreadState state)
        {
            this.state = state;
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
        public void Add() => IncrPC().SetRD(RS + RT);

        [InstructionName(InstructionNames.Addu)]
        public void Addu() => Add();

        [InstructionName(InstructionNames.Unknown)]
        public void Default() => throw new Exception("Not implemented");

        [InstructionName(InstructionNames.Sub)]
        public void Sub() => IncrPC().SetRD(RS - RT);

        [InstructionName(InstructionNames.Subu)]
        public void Subu() => Sub();

        [InstructionName(InstructionNames.Addi)]
        public void Addi() => IncrPC().SetRT(RS + IMM_s);

        [InstructionName(InstructionNames.Addiu)]
        public void Addiu() => Addi();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Logical Operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.And)]
        public void And() => IncrPC().SetRD(RS & RT);

        [InstructionName(InstructionNames.Or)]
        public void Or() => IncrPC().SetRD(RS | RT);

        [InstructionName(InstructionNames.Xor)]
        public void Xor() => IncrPC().SetRD(RS ^ RT);

        [InstructionName(InstructionNames.Nor)]
        public void Nor() => IncrPC().SetRD(~(RS | RT));

        [InstructionName(InstructionNames.Andi)]
        public void Andi() => IncrPC().SetRT(RS_u & IMM_u);

        [InstructionName(InstructionNames.Ori)]
        public void Ori() => IncrPC().SetRT(RS_u | IMM_u);

        [InstructionName(InstructionNames.Xori)]
        public void Xori() => IncrPC().SetRT(RS_u ^ IMM_u);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Shift Left/Right Logical/Arithmethic (Variable).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Sll)]
        public void Sll() => IncrPC().SetRD(RT_u << Pos);

        [InstructionName(InstructionNames.Sra)]
        public void Sra() => IncrPC().SetRD(RT >> Pos);

        [InstructionName(InstructionNames.Srl)]
        public void Srl() => IncrPC().SetRD(RT_u >> Pos);

        [InstructionName(InstructionNames.Rotr)]
        public void Rotr() => IncrPC().SetRD(CpuEmitterUtils._rotr_impl(RT_u, Pos));

        [InstructionName(InstructionNames.Sllv)]
        public void Sllv() => IncrPC().SetRD(RT_u << (int) (RS_u & 31));

        [InstructionName(InstructionNames.Srav)]
        public void Srav() => IncrPC().SetRD(RT >> (int) (RS_u & 31));

        [InstructionName(InstructionNames.Srlv)]
        public void Srlv() => IncrPC().SetRD(RT_u >> (int) (RS_u & 31));

        [InstructionName(InstructionNames.Rotrv)]
        public void Rotrv() => IncrPC().SetRD(CpuEmitterUtils._rotr_impl(RT_u, RS));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Set Less Than (Immediate) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Slt)]
        public void Slt() => IncrPC().SetRD(RS < RT);

        [InstructionName(InstructionNames.Sltu)]
        public void Sltu() => IncrPC().SetRD(RS_u < RT_u);

        [InstructionName(InstructionNames.Slti)]
        public void Slti() => IncrPC().SetRT(RS_u < IMM_s);

        [InstructionName(InstructionNames.Sltiu)]
        public void Sltiu() => IncrPC().SetRT(RS_u < IMM_uex);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Upper Immediate.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Lui)]
        public void Lui() => IncrPC().SetRT(IMM_u << 16);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Sign Extend Byte/Half word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Seb)]
        public void Seb() => IncrPC().SetRD((sbyte)RT);

        [InstructionName(InstructionNames.Seh)]
        public void Seh() => IncrPC().SetRD((short)RT);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // BIT REVerse.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Bitrev)]
        public void Bitrev() => IncrPC().SetRD(CpuEmitterUtils._bitrev_impl(RT_u)); 
            
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MAXimum/MINimum.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Max)]
        public void Max() => IncrPC().SetRD(Math.Max(RS, RT));

        [InstructionName(InstructionNames.Min)]
        public void Min() => IncrPC().SetRD(Math.Min(RS, RT));

        
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // DIVide (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Div)]
        public void Div()
        {
            IncrPC();
            CpuEmitterUtils._div_impl(state, RS, RT);
        }

        [InstructionName(InstructionNames.Divu)]
        public void Divu()
        {
            IncrPC();
            CpuEmitterUtils._divu_impl(state, RS_u, RT_u);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MULTiply (ADD/SUBstract) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mult)]
        public void Mult() => IncrPC().SetHiLo(RS_l * RT_l);

        [InstructionName(InstructionNames.Multu)]
        public void Multu() => IncrPC().SetHiLo(RS_ul * RT_ul);

        [InstructionName(InstructionNames.Madd)]
        public void Madd() =>  IncrPC().SetHiLo(HI_LO_s + (RS_l * RT_l));

        [InstructionName(InstructionNames.Maddu)]
        public void Maddu() => IncrPC().SetHiLo(HI_LO_u + (RS_ul * RT_ul));

        [InstructionName(InstructionNames.Msub)]
        public void Msub() => IncrPC().SetHiLo(HI_LO_s - (RS_l * RT_l));

        [InstructionName(InstructionNames.Msubu)]
        public void Msubu() => IncrPC().SetHiLo(HI_LO_u - (RS_ul * RT_ul));
        
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move To/From HI/LO.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfhi)]
        public void Mfhi() => IncrPC().SetRD(state.Hi);

        [InstructionName(InstructionNames.Mflo)]
        public void Mflo() => IncrPC().SetRD(state.Lo);

        [InstructionName(InstructionNames.Mthi)]
        public void Mthi() => IncrPC().SetHI(RS);

        [InstructionName(InstructionNames.Mtlo)]
        public void Mtlo() => IncrPC().SetLO(RS);

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move if Zero/Non zero.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Movz)]
        public void Movz()
        {
            IncrPC();
            if (RT == 0) SetRD(RS);
        }

        [InstructionName(InstructionNames.Movn)]
        public void Movn()
        {
            IncrPC();
            if (RT != 0) SetRD(RS);
        }

        /// <summary>
        /// EXTract/INSert
        /// </summary>
        [InstructionName(InstructionNames.Ext)]
        public void Ext()
        {
            IncrPC();
            SetRT(CpuEmitterUtils._ext_impl(RS_u, (int) i.Pos, (int) i.SizeE));
        }

        [InstructionName(InstructionNames.Ins)]
        public void Ins()
        {
            IncrPC();
            SetRT(CpuEmitterUtils._ins_impl(RT_u, RS_u, (int) i.Pos, (int) i.SizeI));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Count Leading Ones/Zeros in word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Clz)]
        public void Clz() => IncrPC().SetRD(CpuEmitterUtils._clz_impl(RS_u));

        [InstructionName(InstructionNames.Clo)]
        public void Clo() => IncrPC().SetRD(CpuEmitterUtils._clo_impl(RS_u));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Word Swap Bytes Within Halfwords/Words.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Wsbh)]
        public void Wsbh() => IncrPC().SetRD(CpuEmitterUtils._wsbh_impl(RT_u)); 

        [InstructionName(InstructionNames.Wsbw)]
        public void Wsbw() => IncrPC().SetRD(CpuEmitterUtils._wsbw_impl(RT_u));

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
        public void Mfc0() => IncrPC().SetRT(state.C0R[Rd]);

        [InstructionName(InstructionNames.Mtc0)]
        public void Mtc0()
        {
            IncrPC();
            state.C0R[Rd] = RT_u;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Tools
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #region Tools

        private CpuThreadState.GprList Gpr => state.Gpr;
        private int Rd => i.Rd;
        private int Rs => i.Rs;
        private int Rt => i.Rt;

        private int RD => Gpr[Rd];
        private int RS => Gpr[Rs];
        private int RT => Gpr[Rt];

        private long RD_l => (long)Gpr[Rd];
        private long RS_l => (long)Gpr[Rs];
        private long RT_l => (long)Gpr[Rt];

        private ulong RD_ul => (ulong)Gpr[Rd];
        private ulong RS_ul => (ulong)Gpr[Rs];
        private ulong RT_ul => (ulong)Gpr[Rt];

        private uint RD_u => (uint) Gpr[Rd];
        private uint RS_u => (uint) Gpr[Rs];
        private uint RT_u => (uint) Gpr[Rt];

        private long HI_LO_s => state.HiLo;
        private ulong HI_LO_u => (ulong)state.HiLo;

        private uint Pos_u => (uint) i.Pos;
        private int Pos => (int) i.Pos;

        private int IMM_s => i.Imm;
        private uint IMM_u => i.Immu;
        private uint IMM_uex => (uint)i.Imm;

        private CpuInterpreter IncrPC(int incr = +4)
        {
            state.Pc = state.nPC;
            state.Pc += (uint) incr;
            return this;
        }

        private CpuInterpreter SetGpr(int idx, int value)
        {
            Gpr[idx] = value;
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