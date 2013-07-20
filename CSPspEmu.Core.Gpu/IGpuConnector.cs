using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	public interface IGpuConnector
	{
		void Signal(uint PC, PspGeCallbackData PspGeCallbackData, uint Signal, SignalBehavior Behavior);
		void Finish(uint PC, PspGeCallbackData PspGeCallbackData, uint Arg);
	}
}
