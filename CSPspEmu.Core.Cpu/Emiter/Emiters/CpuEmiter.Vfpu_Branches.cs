using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		public void vcmp() { throw (new NotImplementedException("")); }
		public void vslt() { throw (new NotImplementedException("")); }
		public void vsge() { throw (new NotImplementedException("")); }
		public void vscmp() { throw (new NotImplementedException("")); }

		public void vcmovf() { throw (new NotImplementedException("")); }
		public void vcmovt() { throw (new NotImplementedException("")); }

		public void bvf() { throw (new NotImplementedException()); }
		public void bvfl() { bvf(); }
		public void bvt() { throw (new NotImplementedException()); }
		public void bvtl() { bvt(); }
	}
}
