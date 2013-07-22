using System;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Display;

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
			while (true)
			{
				// TODO: Count this time!
				ThreadTaskQueue.HandleEnqueued();
				if (!Running) return;

				// Draw time
				PspDisplay.TriggerDrawStart();
				Thread.Sleep(TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double)(PspDisplay.VsyncRow))));

				// VBlank time
				PspDisplay.TriggerVBlankStart();
				HleInterruptManager.GetInterruptHandler(PspInterrupts.PSP_VBLANK_INT).Trigger();
				Thread.Sleep(TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double)(PspDisplay.NumberOfRows - PspDisplay.VsyncRow))));
				PspDisplay.TriggerVBlankEnd();
			}
		}
	}
}
