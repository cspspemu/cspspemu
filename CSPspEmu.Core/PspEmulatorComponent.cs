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

		virtual public void ComponentStart()
		{
		}

		virtual public void ComponentReset()
		{
		}

		virtual public void ComponentResume()
		{
		}

		virtual public void ComponentPause()
		{
		}

		virtual public void ComponentStop()
		{
		}

		virtual public void ComponentDestroy()
		{
		}
	}
}
