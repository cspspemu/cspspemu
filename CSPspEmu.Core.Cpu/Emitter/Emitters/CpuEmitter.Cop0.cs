using System;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		// C? (From/To) Cop0
		public void cfc0() {
			//throw (new NotImplementedException());
			Console.WriteLine("Unimplemented cfc0 : {0}, {1}", RT, RD);
		}

		/// <summary>
		/// ctc0    $t0, $17         # Move Control to Coprocessor 0
		/// </summary>
		public void ctc0() {
			Console.WriteLine("Unimplemented ctc0 : {0}, {1}", RT, RD);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) Cop0
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfc0() { this.GenerateAssignGPR(RT, this.REG("C0R" + RD)); }
		public void mtc0() { this.GenerateAssignREG(this.REG("C0R" + RD), GPR(RT)); }
	}
}
