using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	public class PspBasicEmulatorState
	{
		public PspConfig PspConfig;

		public event Action ApplicationExit;

		public PspBasicEmulatorState(PspConfig PspConfig)
		{
			this.PspConfig = PspConfig;
		}
	}
}
