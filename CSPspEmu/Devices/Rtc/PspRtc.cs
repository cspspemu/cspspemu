using System;
using System.Collections.Generic;
using CSharpUtils;

namespace CSPspEmu.Core.Components.Rtc
{
    public unsafe class PspRtc
    {
        public static Logger Logger = Logger.GetLogger("Rtc");

        internal LinkedList<PspVirtualTimer> Timers = new LinkedList<PspVirtualTimer>();
        public DateTime StartDateTime { get; protected set; }
        public DateTime CurrentDateTime { get; protected set; }

        protected PspTimeStruct StartTime;
        protected PspTimeStruct CurrentTime;

        public PspTimeStruct ElapsedTime;

        public TimeSpan Elapsed => CurrentDateTime - StartDateTime;
        public TimeSpan UnixTimeStampTS => CurrentDateTime - new DateTime(1970, 1, 1);
        public uint UnixTimeStamp => (uint) UnixTimeStampTS.TotalSeconds;
        public PspRtc() => Start();

        public void Start()
        {
            StartDateTime = DateTime.UtcNow;
            StartTime.SetToNow();
        }

        protected virtual void UpdateInternal()
        {
            CurrentTime.SetToNow();
            CurrentDateTime = DateTime.UtcNow;
        }

        public void Update()
        {
            UpdateInternal();
            ElapsedTime.TotalMicroseconds = CurrentTime.TotalMicroseconds - StartTime.TotalMicroseconds;

            lock (Timers)
            {
                RetryLoop:
                foreach (var timer in Timers)
                {
                    lock (timer)
                    {
                        if (timer.Enabled && CurrentDateTime >= timer.DateTime)
                        {
                            Timers.Remove(timer);
                            timer.Callback();
                            timer.OnList = false;
                            goto RetryLoop;
                        }
                    }
                }
            }
        }

        public PspVirtualTimer CreateVirtualTimer(Action callback) => new PspVirtualTimer(this)
        {
            Callback = callback,
        };

        public PspVirtualTimer RegisterTimerInOnce(TimeSpan timeSpan, Action callback)
        {
            Logger.Notice("RegisterTimerInOnce: " + timeSpan);
            Update();
            return RegisterTimerAtOnce(CurrentDateTime + timeSpan, callback);
        }

        public PspVirtualTimer RegisterTimerAtOnce(DateTime dateTime, Action callback)
        {
            lock (Timers)
            {
                Logger.Notice("RegisterTimerAtOnce: " + dateTime);
                var virtualTimer = CreateVirtualTimer(callback);
                virtualTimer.SetAt(dateTime);
                virtualTimer.Enabled = true;
                return virtualTimer;
            }
        }

        public void RegisterTimeout(uint* timeout, Action wakeUpCallback)
        {
            if (timeout != null)
            {
                RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*timeout), wakeUpCallback);
            }
        }
    }
}