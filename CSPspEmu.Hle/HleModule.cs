using System;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public class HleModule : PspEmulatorComponent, IDisposable
	{
		protected PspConfig PspConfig { get { return PspEmulatorContext.PspConfig; } }

		public virtual void Dispose()
		{
		}

		public override void InitializeComponent()
		{
		}
	}
}
