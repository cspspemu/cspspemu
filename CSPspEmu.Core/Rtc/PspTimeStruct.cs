using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Rtc
{
	public struct PspTimeStruct
	{
		static public Logger Logger = Logger.GetLogger("Rtc");

		public long TotalMicroseconds;

		public void SetToDateTime(DateTime DateTime)
		{
			var Seconds = (uint)(DateTime - Platform.UnixStart).TotalSeconds;
			TotalMicroseconds = Seconds + DateTime.Millisecond * 1000;
		}

		public void SetToNow()
		{
			var PrevTotalMicroseconds = TotalMicroseconds;
			var CurrentTotalMicroseconds = Platform.GetCurrentUnixMicroseconds();

			if (CurrentTotalMicroseconds < PrevTotalMicroseconds)
			{
				Logger.Error("Total Microseconds overflow Prev({0}), Now({1})", PrevTotalMicroseconds, CurrentTotalMicroseconds);
			}
			this.TotalMicroseconds = CurrentTotalMicroseconds;
		}

		public long TotalMilliseconds
		{
			get
			{
				return TotalMicroseconds / 1000;
			}
		}
	}
}
