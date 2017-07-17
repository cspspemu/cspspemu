using SafeILGenerator.Ast.Generators;
using System;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
    public class GeneratorCSharpPsp : GeneratorCSharp
    {
        protected void _Generate(AstNodeStmPspInstruction pspInstruction)
        {
            Generate(pspInstruction.Statement);
            Output.Write(
                $" // 0x{pspInstruction.DisassembledResult.InstructionPc:X8}: {pspInstruction.DisassembledResult.AssemblyLine}"
            );
        }
    }
}