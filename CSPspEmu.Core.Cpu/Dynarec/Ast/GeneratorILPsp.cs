using SafeILGenerator.Ast.Generators;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
	public class GeneratorILPsp : GeneratorIL
	{
		public GeneratorILPsp()
		{
		}

		protected void _Generate(AstNodeStmPspInstruction PspInstruction)
		{
			GenerateRoot(PspInstruction.Statement);
		}
	}
}
