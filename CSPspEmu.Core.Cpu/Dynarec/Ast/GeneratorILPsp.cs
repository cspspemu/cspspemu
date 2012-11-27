using SafeILGenerator.Ast.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
