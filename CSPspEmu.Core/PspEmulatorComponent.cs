using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	abstract public class PspEmulatorComponent : IDisposable
	{
		protected PspEmulatorContext PspEmulatorContext { get; private set; }

		public PspEmulatorComponent()
		{
		}

		public void _InitializeComponent(PspEmulatorContext PspEmulatorContext)
		{
			if (this.PspEmulatorContext != null) throw(new Exception("Can't call _InitializeComponent twice."));
			this.PspEmulatorContext = PspEmulatorContext;
		}

		abstract public void InitializeComponent();

		virtual public void Dispose()
		{
		}
	}
}
