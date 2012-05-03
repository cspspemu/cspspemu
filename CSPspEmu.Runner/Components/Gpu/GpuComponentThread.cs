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

		[Inject]
		private GpuProcessor GpuProcessor;

		[Inject]
		private GpuImpl GpuImpl;

		public override void InitializeComponent()
		{
		}

		protected override void Main()
		{
			GpuImpl.InitSynchronizedOnce();

			GpuProcessor.ProcessInit();
			GpuProcessor.SetCurrent();

			while (true)
			{
				WaitHandle.WaitAny(new[] { GpuProcessor.DisplayListQueueUpdated, ThreadTaskQueue.EnqueuedEvent, RunningUpdatedEvent }, 10);

				ThreadTaskQueue.HandleEnqueued();
				if (!Running) break;
				GpuProcessor.ProcessStep();
			}

			GpuProcessor.UnsetCurrent();
		}
	}
}
