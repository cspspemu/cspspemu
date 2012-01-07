using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	unsafe sealed public partial class CpuEmiter
	{
		public void vnop()
		{
			Console.Error.WriteLine("vnop");
		}
		public void vsync()
		{
			Console.Error.WriteLine("vsync");
		}

		public void vflush()
		{
			Console.Error.WriteLine("vflush");
		}

	}
}
