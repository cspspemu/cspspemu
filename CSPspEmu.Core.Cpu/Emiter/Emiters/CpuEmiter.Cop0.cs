using System;

namespace CSPspEmu.Core.Cpu.Emiter
{
	public sealed partial class CpuEmiter
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
			MipsMethodEmiter.SaveGPR(RT, () => { MipsMethodEmiter.LoadC0R(RD); });
		}
		public void mtc0() {
			//Console.WriteLine("mtc0 {0}, {1}", RD, RT);
			MipsMethodEmiter.SaveC0R(RD, () => { MipsMethodEmiter.LoadGPR_Unsigned(RT); });
		}
	}
}
