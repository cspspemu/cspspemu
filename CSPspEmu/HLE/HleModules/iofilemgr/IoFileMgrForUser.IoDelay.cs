using System;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Utils;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
    public partial class IoFileMgrForUser
    {
        [Inject] PspRtc PspRtc;

        [Inject] HleThreadManager ThreadManager;

        private class IoDelayType
        {
            public static readonly IoDelayType Open = new IoDelayType("Open", 5.Milliseconds());
            public static readonly IoDelayType Close = new IoDelayType("Close", 1.Milliseconds());
            public static readonly IoDelayType Seek = new IoDelayType("Seek", 0.Milliseconds());
            public static readonly IoDelayType Ioctl = new IoDelayType("Ioctl", 2.Milliseconds());
            public static readonly IoDelayType Devctl = new IoDelayType("Devctl", 2.Milliseconds());
            public static readonly IoDelayType Remove = new IoDelayType("Remove", 0.Milliseconds());
            public static readonly IoDelayType Rename = new IoDelayType("Rename", 0.Milliseconds());

            public static readonly IoDelayType Read = new IoDelayType("Read", 1.Milliseconds(), 4.Milliseconds(), 0x10000);
            public static readonly IoDelayType Write = new IoDelayType("Write", 1.Milliseconds(), 5.Milliseconds(), 0x10000);

            public static readonly IoDelayType GetStat = new IoDelayType("GetStat", 5.Milliseconds());
            public static readonly IoDelayType ChStat = new IoDelayType("ChStat", 5.Milliseconds());

            public static readonly IoDelayType Dopen = new IoDelayType("Dopen", 0.Milliseconds());
            public static readonly IoDelayType Dread = new IoDelayType("Dread", 0.Milliseconds());
            public static readonly IoDelayType Dclose = new IoDelayType("Dclose", 0.Milliseconds());

            public static readonly IoDelayType Mkdir = new IoDelayType("Mkdir", 0.Milliseconds());
            public static readonly IoDelayType Chdir = new IoDelayType("Chdir", 0.Milliseconds());
            public static readonly IoDelayType Rmdir = new IoDelayType("Rmdir", 0.Milliseconds());

            string Name;
            TimeSpan BaseDelay;
            TimeSpan DelayPerUnit;
            int UnitSize = 1;

            private IoDelayType(string name, TimeSpan baseDelay, TimeSpan delayPerUnit = default(TimeSpan), int unitSize = 1)
            {
                this.Name = name;
                this.BaseDelay = baseDelay;
                this.DelayPerUnit = delayPerUnit;
                this.UnitSize = unitSize;
            }

            public TimeSpan GetTimePerSize(long Size) => this.BaseDelay + (((DelayPerUnit.TotalMilliseconds * Size) / UnitSize)).Milliseconds();
            public override string ToString() => $"IoDelayType(Name={Name}, BaseDelay={BaseDelay}, DelayPerUnit={DelayPerUnit}, UnitSize={UnitSize})";
        }

        private void _DelayIo(IoDelayType ioDelayType, long dataSize = 1)
        {
            //return;

            var TimeSpan = ioDelayType.GetTimePerSize(dataSize);

            //Console.WriteLine("_DelayIo: {0}, {1} : {2}", IoDelayType, DataSize, TimeSpan);

            if (TimeSpan != TimeSpan.Zero)
            {
                var CurrentThread = ThreadManager.Current;
                //ThreadManager
                CurrentThread?.SetWaitAndPrepareWakeUp(HleThread.WaitType.Timer, "_DelayIo", null,
                    WakeUpCallback => { PspRtc.RegisterTimerInOnce(TimeSpan, WakeUpCallback); },
                    HandleCallbacks: false);
            }
            else
            {
                ThreadManager.Yield();
            }
        }
    }
}