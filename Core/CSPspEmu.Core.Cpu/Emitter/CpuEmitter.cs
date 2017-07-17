using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System.Runtime.CompilerServices;
using System.IO;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed unsafe partial class CpuEmitter
    {
        [Inject] private CpuProcessor CpuProcessor;

        [Inject] private PspMemory Memory;

        private MipsMethodEmitter MipsMethodEmitter;
        private IInstructionReader InstructionReader;
        private Instruction Instruction;
        private uint PC;

        public int BranchCount = 0;

        private static AstMipsGenerator ast = AstMipsGenerator.Instance;

        public CpuEmitter(InjectContext injectContext, MipsMethodEmitter mipsMethodEmitter,
            IInstructionReader instructionReader)
        {
            injectContext.InjectDependencesTo(this);
            this.MipsMethodEmitter = mipsMethodEmitter;
            this.InstructionReader = instructionReader;
        }

        public Instruction LoadAT(uint pc) => Instruction = InstructionReader[PC = pc];

        private int ONE_TWO => Instruction.ONE_TWO;
        private int RT => Instruction.RT;
        private int RD => Instruction.RD;
        private int RS => Instruction.RS;
        private int IMM => Instruction.IMM;
        private uint IMMU => Instruction.IMMU;
        private int FT => Instruction.FT;
        private int FD => Instruction.FD;
        private int FS => Instruction.FS;
        private AstNodeExpr IMM_s() => ast.Immediate(IMM);
        private AstNodeExpr IMM_u() => ast.Immediate((uint) (ushort) IMM);
        private AstNodeExpr IMM_uex() => ast.Immediate((uint) IMM);

        private AstNodeExpr Address_RS_IMM14(int offset = 0) =>
            ast.Cast<uint>(ast.Binary(ast.GPR_s(RS), "+", Instruction.IMM14 * 4 + offset), false);

        private AstNodeExpr Address_RS_IMM() => ast.Cast<uint>(ast.Binary(ast.GPR_s(RS), "+", IMM_s()), false);
    }
}