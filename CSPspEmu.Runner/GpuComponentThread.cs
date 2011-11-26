using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;

namespace CSPspEmu.Runner
{
	sealed public class GpuComponentThread : ComponentThread
	{
		protected override string ThreadName { get { return "CpuThread"; } }

		private GpuProcessor GpuProcessor;

		public GpuComponentThread(PspEmulatorContext PspEmulatorContext)
			: base(PspEmulatorContext)
		{
			GpuProcessor = PspEmulatorContext.GetInstance<GpuProcessor>();
		}

		protected override void Main()
		{
			PspEmulatorContext.GetInstance<GpuImpl>().InitSynchronizedOnce();

			GpuProcessor.ProcessInit();

			while (true)
			{
				ThreadTaskQueue.HandleEnqueued();
				if (!Running) return;
				GpuProcessor.ProcessStep();
			}
		}
	}
}
