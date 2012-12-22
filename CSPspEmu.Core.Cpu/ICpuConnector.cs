using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public interface ICpuConnector
	{
		void Yield(CpuThreadState CpuThreadState);
	}
}
