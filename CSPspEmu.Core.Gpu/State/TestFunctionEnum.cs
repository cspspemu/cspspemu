using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	public enum TestFunctionEnum
	{
		Never = 0,
		Always = 1,
		Equal = 2,
		NotEqual = 3,
		Less = 4,
		LessOrEqual = 5,
		Greater = 6,
		GreaterOrEqual = 7,
	}
}
