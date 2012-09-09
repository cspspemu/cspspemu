using System;
using CSharpUtils;

namespace CSPspEmu.Core.Rtc
{
	public struct PspTimeStruct
	{
		static public Logger Logger = Logger.GetLogger("Rtc");

		public long TotalMicroseconds;

		public void SetToDateTime(DateTime DateTime)
		{
			//DateTime.GetTotalMicroseconds();
			//var Seconds = (uint)(DateTime - Platform.UnixStart).TotalSeconds;
			TotalMicroseconds = Platform.GetCurrentUnixMicroseconds();
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
	}
}
