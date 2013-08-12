using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;
using System;

namespace CSPspEmu.Runner.Components.Gpu
{
	public sealed class GpuComponentThread : ComponentThread
	{
		protected override string ThreadName { get { return "CpuThread"; } }

		[Inject]
		private GpuProcessor GpuProcessor;

		[Inject]
		private GpuImpl GpuImpl;

		protected override void Main()
		{
			GpuImpl.InitSynchronizedOnce();

			GpuProcessor.ProcessInit();

			Console.WriteLine("GpuComponentThread.Start()");
			try
			{
				while (true)
				{
					WaitHandle.WaitAny(new WaitHandle[] { GpuProcessor.DisplayListQueueUpdated, ThreadTaskQueue.EnqueuedEvent, RunningUpdatedEvent }, 200);

					// TODO: Should wait until the Form has created its context.

					ThreadTaskQueue.HandleEnqueued();
					if (!Running) break;
					GpuProcessor.SetCurrent();
					GpuProcessor.ProcessStep();
					GpuProcessor.UnsetCurrent();
				}
			}
			finally
			{
				Console.WriteLine("GpuComponentThread.End()");
			}
		}
	}
}
