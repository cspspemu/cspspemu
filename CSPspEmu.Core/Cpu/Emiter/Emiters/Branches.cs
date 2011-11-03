using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// Branch on EQuals (Likely).
		public void beq() { throw (new NotImplementedException()); }
		public void beql() { throw (new NotImplementedException()); }

		// Branch on Greater Equal Zero (And Link) (Likely).
		public void bgez() { throw (new NotImplementedException()); }
		public void bgezl() { throw (new NotImplementedException()); }
		public void bgezal() { throw (new NotImplementedException()); }
		public void bgezall() { throw (new NotImplementedException()); }

		// Branch on Less Than Zero (And Link) (Likely).
		public void bltz() { throw (new NotImplementedException()); }
		public void bltzl() { throw (new NotImplementedException()); }
		public void bltzal() { throw (new NotImplementedException()); }
		public void bltzall() { throw (new NotImplementedException()); }

		// Branch on Less Or Equals than Zero (Likely).
		public void blez() { throw (new NotImplementedException()); }
		public void blezl() { throw (new NotImplementedException()); }

		// Branch on Great Than Zero (Likely).
		public void bgtz() { throw (new NotImplementedException()); }
		public void bgtzl() { throw (new NotImplementedException()); }

		// Branch on Not Equals (Likely).
		public void bne() { throw (new NotImplementedException()); }
		public void bnel() { throw (new NotImplementedException()); }

		// Jump (And Link) (Register).
		public void j() { throw (new NotImplementedException()); }
		public void jr() { throw (new NotImplementedException()); }
		public void jalr() { throw (new NotImplementedException()); }
		public void jal() { throw (new NotImplementedException()); }

		// Branch on C1 False/True (Likely).
		public void bc1f() { throw (new NotImplementedException()); }
		public void bc1t() { throw (new NotImplementedException()); }
		public void bc1fl() { throw (new NotImplementedException()); }
		public void bc1tl() { throw (new NotImplementedException()); }
	}
}
