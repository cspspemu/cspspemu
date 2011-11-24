using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	public class PspEmulatorComponent
	{
		protected PspEmulatorContext PspEmulatorContext;

		public PspEmulatorComponent(PspEmulatorContext PspEmulatorContext)
		{
			this.PspEmulatorContext = PspEmulatorContext;
		}
	}
}
