using System;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public class HleOutputHandler : PspEmulatorComponent
	{
		public override void InitializeComponent()
		{
		}

		public virtual void Output(string Output)
		{
			Console.WriteLine("   OUTPUT:  {0}", Output);
		}
	}
}
