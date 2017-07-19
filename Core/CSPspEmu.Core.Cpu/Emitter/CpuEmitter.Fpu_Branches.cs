using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // bc1(f/t)(l): Branch on C1 (False/True) (Likely)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Bc1F)]
        public AstNodeStm Bc1F() => AssignBranchFlag(_ast.Unary("!", _ast.FCR31_CC()));

        [InstructionName(InstructionNames.Bc1Fl)]
        public AstNodeStm Bc1Fl() => Bc1F();

        [InstructionName(InstructionNames.Bc1T)]
        public AstNodeStm Bc1T() => AssignBranchFlag(_ast.FCR31_CC());

        [InstructionName(InstructionNames.Bc1Tl)]
        public AstNodeStm Bc1Tl() => Bc1T();
    }
}