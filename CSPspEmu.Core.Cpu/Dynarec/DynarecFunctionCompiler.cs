using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	/// <summary>
	/// Compiles functions
	/// </summary>
	unsafe public partial class DynarecFunctionCompiler : PspEmulatorComponent
	{
		public DynarecFunction CreateFunction(InstructionReader InstructionReader, uint PC)
		{
			var InternalFunctionCompiler = new InternalFunctionCompiler(this, InstructionReader, PC);
			return InternalFunctionCompiler.CreateFunction();
		}
	}
}
