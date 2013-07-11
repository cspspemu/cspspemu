using SafeILGenerator.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public class AstMipsGenerator : AstGenerator
	{
		static public AstMipsGenerator Instance = new AstMipsGenerator();

		protected AstMipsGenerator() : base()
		{
		}
	}
}
