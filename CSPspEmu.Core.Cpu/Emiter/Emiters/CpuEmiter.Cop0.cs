using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
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
