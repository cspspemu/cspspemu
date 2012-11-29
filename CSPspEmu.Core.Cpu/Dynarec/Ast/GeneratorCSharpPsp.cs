using SafeILGenerator.Ast.Generators;
using System;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
	public class GeneratorCSharpPsp : GeneratorCSharp
	{
		protected void _Generate(AstNodeStmPspInstruction PspInstruction)
		{
			GenerateRoot(PspInstruction.Statement);
			Output.Write(String.Format(" // 0x{0:X8} {1}", PspInstruction.DisassembledResult.InstructionPC, PspInstruction.DisassembledResult.AssemblyLine));
		}
	}
}
