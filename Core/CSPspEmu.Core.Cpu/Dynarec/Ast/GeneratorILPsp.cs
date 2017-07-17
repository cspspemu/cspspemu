using SafeILGenerator.Ast.Generators;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
    public class GeneratorIlPsp : GeneratorIL
    {
        protected void _Generate(AstNodeStmPspInstruction pspInstruction)
        {
            EmitComment(
                $"0x{pspInstruction.DisassembledResult.InstructionPc:X8}: {pspInstruction.DisassembledResult}"
            );
            Generate(pspInstruction.Statement);
        }
    }
}