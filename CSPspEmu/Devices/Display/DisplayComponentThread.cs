using System;
using System.Threading;
using CSPspEmu.Hle.Managers;
using CSharpUtils;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Utils;

namespace CSPspEmu.Runner.Components.Display
{
    public sealed class DisplayComponentThread : ComponentThread
    {
        [Inject] private HleInterruptManager _hleInterruptManager;

        [Inject] private PspDisplay _pspDisplay;

        protected override string ThreadName => "DisplayThread";

        TimeSpan vSyncTimeIncrement =
            TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double) (PspDisplay.VsyncRow)));
        //var VSyncTimeIncrement = TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double)(PspDisplay.VsyncRow / 2))); // HACK to give more time to render!
        TimeSpan endTimeIncrement =
            TimeSpan.FromSeconds(1.0 / (PspDisplay.HorizontalSyncHertz / (double) (PspDisplay.NumberOfRows)));
        HleInterruptHandler vBlankInterruptHandler;
        public bool triggerStuff = true;

        public void Step(Action DrawStart, Action VBlankStart, Action VBlankEnd)
        {
            //Console.WriteLine("[1]");
            var startTime = DateTime.UtcNow;
            var vSyncTime = startTime + vSyncTimeIncrement;
            var endTime = startTime + endTimeIncrement;

            ThreadTaskQueue.HandleEnqueued();
            if (!Running) return;

            // Draw time
            DrawStart();
            ThreadUtils.SleepUntilUtc(vSyncTime);

            // VBlank time
            VBlankStart();
            vBlankInterruptHandler.Trigger();
            ThreadUtils.SleepUntilUtc(endTime);
            VBlankEnd();
        }

        protected override void Main()
        {
            vBlankInterruptHandler = _hleInterruptManager.GetInterruptHandler(PspInterrupts.PspVblankInt);
            Console.WriteLine("DisplayComponentThread.Start()");
            try
            {
                while (Running)
                {
                    if (triggerStuff)
                    {
                        Step(_pspDisplay.TriggerDrawStart, _pspDisplay.TriggerVBlankStart, _pspDisplay.TriggerVBlankEnd);
                    }
                    else
                    {
                        Thread.Sleep(16.Milliseconds());
                    }
                }
            }
            finally
            {
                Console.WriteLine("DisplayComponentThread.End()");
            }
        }
    }
}