using SafeILGenerator.Ast.Generators;
using System;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
	public class GeneratorCSharpPsp : GeneratorCSharp
	{
		protected void _Generate(AstNodeStmPspInstruction pspInstruction)
		{
			Generate(pspInstruction.Statement);
			Output.Write(String.Format(" // 0x{0:X8}: {1}", pspInstruction.DisassembledResult.InstructionPc, pspInstruction.DisassembledResult.AssemblyLine));
		}
	}
}
