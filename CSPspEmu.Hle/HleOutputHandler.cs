using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public class HleOutputHandler : PspEmulatorComponent
	{
		public override void InitializeComponent()
		{
		}

		virtual public void Output(string Output)
		{
			Console.WriteLine("   OUTPUT:  {0}", Output);
		}
	}
}
