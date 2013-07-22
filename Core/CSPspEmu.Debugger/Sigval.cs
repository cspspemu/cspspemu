using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Debugger
{
	public enum Sigval
	{
		Ok = -1,
		InvalidOpcode = 4,
		DebugException = 5,
		DoubleFault = 7,
		DivideByZero = 8,
		MemoryException = 11,
		Overflow = 16,
	}
}
