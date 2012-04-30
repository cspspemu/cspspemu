using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Rtc
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
						this._DateTime = value;
						if (!OnList)
						{
							PspRtc.Timers.AddLast(this);
							OnList = true;
						}
					}
				}
			}
			get
			{
				return _DateTime;
			}
		}
		public bool OnList;

		internal Action Callback;
		public bool Enabled;

		internal PspVirtualTimer(PspRtc PspRtc)
		{
			this.PspRtc = PspRtc;
		}

		public void SetIn(TimeSpan TimeSpan)
		{
			this.DateTime = DateTime.UtcNow + TimeSpan;
		}

		public void SetAt(DateTime DateTime)
		{
			this.DateTime = DateTime;
		}

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}
}
