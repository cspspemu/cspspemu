using SafeILGenerator.Ast.Nodes;
using System;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		// C? (From/To) Cop0
		public AstNodeStm cfc0()
		{
			//throw (new NotImplementedException());
			Console.WriteLine("Unimplemented cfc0 : {0}, {1}", RT, RD);
			return ast.Statement();
		}

		/// <summary>
		/// ctc0    $t0, $17         # Move Control to Coprocessor 0
		/// </summary>
		public AstNodeStm ctc0()
		{
			Console.WriteLine("Unimplemented ctc0 : {0}, {1}", RT, RD);
			return ast.Statement();
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) Cop0
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfc0() { return AssignGPR(RT, REG("C0R" + RD)); }
		public AstNodeStm mtc0() { return AssignREG("C0R" + RD, GPR(RT)); }
	}
}
