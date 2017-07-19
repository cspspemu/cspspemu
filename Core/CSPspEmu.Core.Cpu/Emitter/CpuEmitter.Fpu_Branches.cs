using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // bc1(f/t)(l): Branch on C1 (False/True) (Likely)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("bc1f")]
        public AstNodeStm Bc1F() => AssignBranchFlag(_ast.Unary("!", _ast.FCR31_CC()));

        [InstructionName("bc1fl")]
        public AstNodeStm Bc1Fl() => Bc1F();

        [InstructionName("bc1t")]
        public AstNodeStm Bc1T() => AssignBranchFlag(_ast.FCR31_CC());

        [InstructionName("bc1tl")]
        public AstNodeStm Bc1Tl() => Bc1T();
    }
}