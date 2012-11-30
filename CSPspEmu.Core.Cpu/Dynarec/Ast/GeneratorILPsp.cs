using SafeILGenerator.Ast.Generators;
using System;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
	public class GeneratorILPsp : GeneratorIL
	{
		public GeneratorILPsp()
		{
		}

		protected void _Generate(AstNodeStmPspInstruction PspInstruction)
		{
			EmitComment(String.Format("0x{0:X8}: {1}", PspInstruction.DisassembledResult.InstructionPC, PspInstruction.DisassembledResult.ToString()));
			GenerateRoot(PspInstruction.Statement);
		}
	}
}
