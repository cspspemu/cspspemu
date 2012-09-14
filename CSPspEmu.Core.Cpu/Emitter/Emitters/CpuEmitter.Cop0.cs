using System;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		// C? (From/To) Cop0
		public void cfc0() { throw (new NotImplementedException()); }

		/// <summary>
		/// ctc0    $t0, $17         # Move Control to Coprocessor 0
		/// </summary>
		public void ctc0() {
			Console.WriteLine("Unimplemented ctc0 : {0}, {1}", RT, RD);
		}

		// Move (From/To) Cop0
		public void mfc0() {
			//Console.WriteLine("mfc0 {0}, {1}", RT, RD);
			MipsMethodEmitter.SaveGPR(RT, () => { MipsMethodEmitter.LoadC0R(RD); });
		}
		public void mtc0() {
			//Console.WriteLine("mtc0 {0}, {1}", RD, RT);
			MipsMethodEmitter.SaveC0R(RD, () => { MipsMethodEmitter.LoadGPR_Unsigned(RT); });
		}
	}
}
