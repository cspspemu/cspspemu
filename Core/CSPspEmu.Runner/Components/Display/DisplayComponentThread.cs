using System;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Display;
using CSharpUtils;

namespace CSPspEmu.Runner.Components.Display
{
	public sealed class DisplayComponentThread : ComponentThread
	{
		[Inject]
		private HleInterruptManager HleInterruptManager;

		[Inject]
		private PspDisplay PspDisplay;

		protected override string ThreadName { get { return "DisplayThread"; } }

		protected override void Main()
		{
			Console.WriteLine("DisplayComponentThread.Start()");
			try
			{
				var VSyncTimeIncrement = TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double)(PspDisplay.VsyncRow)));
				var EndTimeIncrement = TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double)(PspDisplay.NumberOfRows)));
				var VBlankInterruptHandler = HleInterruptManager.GetInterruptHandler(PspInterrupts.PSP_VBLANK_INT);
				while (true)
				{
					//Console.WriteLine("[1]");
					var StartTime = DateTime.UtcNow;
					var VSyncTime = StartTime + VSyncTimeIncrement;
					var EndTime = StartTime + EndTimeIncrement;

					ThreadTaskQueue.HandleEnqueued();
					if (!Running) return;

					// Draw time
					PspDisplay.TriggerDrawStart();
					ThreadUtils.SleepUntilUtc(VSyncTime);

					// VBlank time
					PspDisplay.TriggerVBlankStart();
					VBlankInterruptHandler.Trigger();
					ThreadUtils.SleepUntilUtc(EndTime);
					PspDisplay.TriggerVBlankEnd();
				}
			}
			finally
			{
				Console.WriteLine("DisplayComponentThread.End()");
			}
		}
	}
}
