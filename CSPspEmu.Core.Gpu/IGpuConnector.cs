using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	public interface IGpuConnector
	{
		void Signal(uint Signal, GpuDisplayList.GuBehavior Behavior);
		void Finish(uint Arg);
	}
}
