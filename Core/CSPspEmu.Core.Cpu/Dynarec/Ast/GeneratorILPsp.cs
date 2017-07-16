using SafeILGenerator.Ast.Generators;
using System;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
	public class GeneratorILPsp : GeneratorIL
	{
		public GeneratorILPsp() : base()
		{
		}

		protected void _Generate(AstNodeStmPspInstruction pspInstruction)
		{
			EmitComment(String.Format("0x{0:X8}: {1}", pspInstruction.DisassembledResult.InstructionPc, pspInstruction.DisassembledResult.ToString()));
			Generate(pspInstruction.Statement);
		}
	}
}
