using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// Load Byte/Half word/Word (Left/Right/Unsigned).
		public void lb() { throw (new NotImplementedException()); }
		public void lh() { throw (new NotImplementedException()); }
		public void lw() { throw (new NotImplementedException()); }
		public void lwl() { throw (new NotImplementedException()); }
		public void lwr() { throw (new NotImplementedException()); }
		public void lbu() { throw (new NotImplementedException()); }
		public void lhu() { throw (new NotImplementedException()); }

		// Store Byte/Half word/Word (Left/Right).
		public void sb() { throw (new NotImplementedException()); }
		public void sh() { throw (new NotImplementedException()); }
		public void sw() { throw (new NotImplementedException()); }
		public void swl() { throw (new NotImplementedException()); }
		public void swr() { throw (new NotImplementedException()); }

		// Load Linked word.
		// Store Conditional word.
		public void ll() { throw (new NotImplementedException()); }
		public void sc() { throw (new NotImplementedException()); }

		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		public void lwc1() { throw (new NotImplementedException()); }
		public void swc1() { throw (new NotImplementedException()); }
	}
}
