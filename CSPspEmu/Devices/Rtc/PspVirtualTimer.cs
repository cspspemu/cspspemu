using System;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Components.Rtc
{
    public class PspVirtualTimer
    {
        protected PspRtc PspRtc;

        protected DateTime _DateTime;

        public DateTime DateTime
        {
            set
            {
                lock (PspRtc.Timers)
                {
                    lock (this)
                    {
                        _DateTime = value;
                        if (!OnList)
                        {
                            PspRtc.Timers.AddLast(this);
                            OnList = true;
                        }
                    }
                }
            }
            get => _DateTime;
        }

        public bool OnList;

        internal Action Callback;
        public bool Enabled;

        internal PspVirtualTimer(PspRtc pspRtc) => PspRtc = pspRtc;
        public void SetIn(TimeSpan timeSpan) => DateTime = DateTime.UtcNow + timeSpan;
        public void SetAt(DateTime dateTime) => DateTime = dateTime;
        public override string ToString() => this.ToStringDefault();
    }
}