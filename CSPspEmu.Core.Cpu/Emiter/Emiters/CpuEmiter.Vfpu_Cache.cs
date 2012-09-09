using System;

namespace CSPspEmu.Core.Cpu.Emiter
{
    public sealed partial class CpuEmiter
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
