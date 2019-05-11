using System;
using CSharpUtils;

namespace CSPspEmu.Core.Components.Rtc
{
    public struct PspTimeStruct
    {
        public static Logger Logger = Logger.GetLogger("Rtc");

        public long TotalMicroseconds;

        public void SetToDateTime(DateTime dateTime)
        {
            //DateTime.GetTotalMicroseconds();
            //var Seconds = (uint)(DateTime - Platform.UnixStart).TotalSeconds;
            TotalMicroseconds = Platform.CurrentUnixMicroseconds;
        }

        public void SetToNow()
        {
            var prevTotalMicroseconds = TotalMicroseconds;
            var currentTotalMicroseconds = Platform.CurrentUnixMicroseconds;

            if (currentTotalMicroseconds < prevTotalMicroseconds)
            {
                Logger.Error("Total Microseconds overflow Prev({0}), Now({1})", prevTotalMicroseconds,
                    currentTotalMicroseconds);
            }
            TotalMicroseconds = currentTotalMicroseconds;
        }
    }
}