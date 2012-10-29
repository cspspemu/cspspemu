using System;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
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
