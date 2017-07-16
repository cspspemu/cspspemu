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
        [Inject] private HleInterruptManager _hleInterruptManager;

        [Inject] private PspDisplay _pspDisplay;

        protected override string ThreadName => "DisplayThread";

        protected override void Main()
        {
            Console.WriteLine("DisplayComponentThread.Start()");
            try
            {
                var vSyncTimeIncrement =
                    TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double) (PspDisplay.VsyncRow)));
                //var VSyncTimeIncrement = TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double)(PspDisplay.VsyncRow / 2))); // HACK to give more time to render!
                var endTimeIncrement =
                    TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double) (PspDisplay.NumberOfRows)));
                var vBlankInterruptHandler = _hleInterruptManager.GetInterruptHandler(PspInterrupts.PSP_VBLANK_INT);
                while (true)
                {
                    //Console.WriteLine("[1]");
                    var startTime = DateTime.UtcNow;
                    var vSyncTime = startTime + vSyncTimeIncrement;
                    var endTime = startTime + endTimeIncrement;

                    ThreadTaskQueue.HandleEnqueued();
                    if (!Running) return;

                    // Draw time
                    _pspDisplay.TriggerDrawStart();
                    ThreadUtils.SleepUntilUtc(vSyncTime);

                    // VBlank time
                    _pspDisplay.TriggerVBlankStart();
                    vBlankInterruptHandler.Trigger();
                    ThreadUtils.SleepUntilUtc(endTime);
                    _pspDisplay.TriggerVBlankEnd();
                }
            }
            finally
            {
                Console.WriteLine("DisplayComponentThread.End()");
            }
        }
    }
}