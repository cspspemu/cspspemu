using System;

using CSPspEmu.Hle.Modules.rtc;
using CSPspEmu.Hle.Vfs;
using CSharpUtils;
using CSPspEmu.Core.Components.Rtc;
using Xunit;

namespace CSPspEmu.Hle.Modules.Tests.rtc
{
    
    [InjectMap(typeof(PspRtc), typeof(PspRtcMock))]
    public class sceRtcTest : BaseModuleTest
    {
        [Inject] sceRtc sceRtc = null;

        int FakedYear;
        int FakedMonth;
        int FakedDay;
        int FakedHour;
        int FakedMinute;
        int FakedSecond;
        int FakedMillisecond;

        private void ResetTimes()
        {
            FakedYear = 2012;
            FakedMonth = 4;
            FakedDay = 30;
            FakedHour = 12;
            FakedMinute = 13;
            FakedSecond = 14;
            FakedMillisecond = 973;
        }

        public class PspRtcMock : PspRtc
        {
            public sceRtcTest sceRtcTest;

            protected override void UpdateInternal()
            {
                Console.WriteLine("PspRtcMock.UpdateInternal()");
                var DateTime = new DateTime(
                    sceRtcTest.FakedYear,
                    sceRtcTest.FakedMonth,
                    sceRtcTest.FakedDay,
                    sceRtcTest.FakedHour,
                    sceRtcTest.FakedMinute,
                    sceRtcTest.FakedSecond,
                    sceRtcTest.FakedMillisecond
                );
                CurrentTime.SetToDateTime(DateTime);
                this.CurrentDateTime = DateTime;
            }
        }

        public sceRtcTest()
        {
            ResetTimes();
        }

        [Fact]
        public void Test_sceRtcGetDayOfWeek()
        {
            Assert.Equal((int) PspDaysOfWeek.Monday, 1);
            Assert.Equal(PspDaysOfWeek.Monday, sceRtc.sceRtcGetDayOfWeek(2012, 4, 30));
            Assert.Equal(2, (int) sceRtc.sceRtcGetDayOfWeek(2012, 5, 1));
            Assert.Equal(PspDaysOfWeek.Tuesday, sceRtc.sceRtcGetDayOfWeek(2012, 5, 1));
        }

        [Fact(Skip = "check. Time not mocked")]
        public void Test_sceRtcGetCurrentClock()
        {
            ScePspDateTime ScePspDateTime;
            var Result = sceRtc.sceRtcGetCurrentClock(out ScePspDateTime, 0);
            Assert.Equal(0, Result);
            Assert.Equal(FakedYear, (int) ScePspDateTime.Year);
            Assert.Equal(FakedMonth, (int) ScePspDateTime.Month);
            Assert.Equal(FakedDay, (int) ScePspDateTime.Day);
            Assert.Equal(FakedHour, (int) ScePspDateTime.Hour);
            Assert.Equal(FakedMinute, (int) ScePspDateTime.Minute);
            Assert.Equal(FakedSecond, (int) ScePspDateTime.Second);
            Assert.Equal(FakedMillisecond, (int) ScePspDateTime.Microsecond / 1000);
        }

        [Fact(Skip = "check. Time not mocked")]
        public void Test_timeIsIncreasing()
        {
            DateTime PrevDateTime = DateTimeRange.ConvertFromUnixTimestamp(0);
            ScePspDateTime ScePspDateTime;
            for (int n = 0; n < 40; n++)
            {
                Console.WriteLine("Iter");
                FakedMillisecond++;
                if (FakedMillisecond >= 1000)
                {
                    FakedSecond++;
                    FakedMillisecond = FakedMillisecond % 1000;
                }

                var Result = sceRtc.sceRtcGetCurrentClock(out ScePspDateTime, 0);

                var CurrentDateTime = ScePspDateTime.ToDateTime();

                if (!(CurrentDateTime > PrevDateTime))
                {
                    Console.WriteLine("N: {0}", n);
                    Console.WriteLine("P: {0}", PrevDateTime.Ticks);
                    Console.WriteLine("C: {0}", CurrentDateTime.Ticks);
                }
                Assert.True(CurrentDateTime.Ticks > PrevDateTime.Ticks);
                PrevDateTime = CurrentDateTime;
            }
        }
    }
}