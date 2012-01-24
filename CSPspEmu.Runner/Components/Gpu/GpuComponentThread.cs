using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;

namespace CSPspEmu.Runner.Components.Gpu
{
	sealed public class GpuComponentThread : ComponentThread
	{
		protected override string ThreadName { get { return "CpuThread"; } }

		private GpuProcessor GpuProcessor;

		public override void InitializeComponent()
		{
			GpuProcessor = PspEmulatorContext.GetInstance<GpuProcessor>();
		}

		protected override void Main()
		{
			PspEmulatorContext.GetInstance<GpuImpl>().InitSynchronizedOnce();

			GpuProcessor.ProcessInit();
			GpuProcessor.SetCurrent();

			while (true)
			{
				ThreadTaskQueue.HandleEnqueued();
				if (!Running) break;
				GpuProcessor.ProcessStep();
			}

			GpuProcessor.UnsetCurrent();
		}
	}
}
