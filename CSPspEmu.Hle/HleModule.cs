using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public class HleModule : PspEmulatorComponent, IDisposable
	{
		protected PspConfig PspConfig { get { return PspEmulatorContext.PspConfig; } }

		virtual public void Dispose()
		{
		}

		public override void InitializeComponent()
		{
		}
	}
}
