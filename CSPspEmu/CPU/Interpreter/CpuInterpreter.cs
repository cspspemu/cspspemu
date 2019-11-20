using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.CPU.Interpreter
{
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

        private CpuThreadState.GprList Gpr => state.Gpr;
        private int Rd => i.Rd;
        private int Rs => i.Rs;
        private int Rt => i.Rt;
        
        [InstructionName(InstructionNames.Add)]
        public void Add() => Gpr[Rd] = Gpr[Rs] + Gpr[Rt];

        [InstructionName(InstructionNames.Addu)]
        public void Addu() => Add();
        
        [InstructionName(InstructionNames.Unknown)]
        public void Default() => throw new Exception("Not implemented");
    }
}