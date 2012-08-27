using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// C? (From/To) Cop0
		public void cfc0() { throw (new NotImplementedException()); }
		public void ctc0() { throw (new NotImplementedException()); }

		// Move (From/To) Cop0
		public void mfc0() { MipsMethodEmiter.SaveGPR(RT, () => { MipsMethodEmiter.LoadC0R(RD); }); }
		public void mtc0() { MipsMethodEmiter.SaveC0R(RD, () => { MipsMethodEmiter.LoadGPR_Unsigned(RT); }); }
	}
}
