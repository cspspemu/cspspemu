using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System.Runtime.CompilerServices;
using System.IO;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        [Inject] private CpuProcessor _cpuProcessor;

        [Inject] private PspMemory _memory;

        private MipsMethodEmitter _mipsMethodEmitter;
        private IInstructionReader _instructionReader;
        private Instruction _instruction;
        private uint _pc;

        public int BranchCount = 0;

        private static AstMipsGenerator _ast = AstMipsGenerator.Instance;

        public CpuEmitter(InjectContext injectContext, MipsMethodEmitter mipsMethodEmitter,
            IInstructionReader instructionReader)
        {
            injectContext.InjectDependencesTo(this);
            _mipsMethodEmitter = mipsMethodEmitter;
            _instructionReader = instructionReader;
        }

        public Instruction LoadAt(uint pc) => _instruction = _instructionReader[_pc = pc];

        private int OneTwo => _instruction.OneTwo;
        private int Rt => _instruction.Rt;
        private int Rd => _instruction.Rd;
        private int Rs => _instruction.Rs;
        private int Imm => _instruction.Imm;
        private uint Immu => _instruction.Immu;
        private int Ft => _instruction.Ft;
        private int Fd => _instruction.Fd;
        private int Fs => _instruction.Fs;
        private AstNodeExpr IMM_s() => _ast.Immediate(Imm);
        private AstNodeExpr IMM_u() => _ast.Immediate((uint) (ushort) Imm);
        private AstNodeExpr IMM_uex() => _ast.Immediate((uint) Imm);

        private AstNodeExpr Address_RS_IMM14(int offset = 0) =>
            _ast.Cast<uint>(_ast.Binary(_ast.GPR_s(Rs), "+", _instruction.Imm14 * 4 + offset), false);

        private AstNodeExpr Address_RS_IMM() => _ast.Cast<uint>(_ast.Binary(_ast.GPR_s(Rs), "+", IMM_s()), false);
    }
}