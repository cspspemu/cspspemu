using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		public void bvf() { throw (new NotImplementedException()); }
		public void bvfl() { bvf(); }
		public void bvt() { throw (new NotImplementedException()); }
		public void bvtl() { bvt(); }
	}
}
