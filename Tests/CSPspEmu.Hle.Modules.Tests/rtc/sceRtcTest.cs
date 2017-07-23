using System;

using CSPspEmu.Hle.Modules.rtc;
using CSPspEmu.Hle.Vfs;
using CSharpUtils;
using CSPspEmu.Core.Components.Rtc;
using Xunit;

namespace CSPspEmu.Hle.Modules.Tests.rtc
{
    
    [InjectMap(typeof(PspRtc), typeof(PspRtcMock))]
    public class SceRtcTest : BaseModuleTest
    {
        [Inject] protected sceRtc SceRtc = null;

        int _fakedYear;
        int _fakedMonth;
        int _fakedDay;
        int _fakedHour;
        int _fakedMinute;
        int _fakedSecond;
        int _fakedMillisecond;

        private void ResetTimes()
        {
            _fakedYear = 2012;
            _fakedMonth = 4;
            _fakedDay = 30;
            _fakedHour = 12;
            _fakedMinute = 13;
            _fakedSecond = 14;
            _fakedMillisecond = 973;
        }

        public class PspRtcMock : PspRtc
        {
            public SceRtcTest SceRtcTest;

            protected override void UpdateInternal()
            {
                Console.WriteLine("PspRtcMock.UpdateInternal()");
                var dateTime = new DateTime(
                    SceRtcTest._fakedYear,
                    SceRtcTest._fakedMonth,
                    SceRtcTest._fakedDay,
                    SceRtcTest._fakedHour,
                    SceRtcTest._fakedMinute,
                    SceRtcTest._fakedSecond,
                    SceRtcTest._fakedMillisecond
                );
                CurrentTime.SetToDateTime(dateTime);
                CurrentDateTime = dateTime;
            }
        }

        public SceRtcTest()
        {
            ResetTimes();
        }

        [Fact]
        public void Test_sceRtcGetDayOfWeek()
        {
            Assert.Equal((int) PspDaysOfWeek.Monday, 1);
            Assert.Equal(PspDaysOfWeek.Monday, SceRtc.sceRtcGetDayOfWeek(2012, 4, 30));
            Assert.Equal(2, (int) SceRtc.sceRtcGetDayOfWeek(2012, 5, 1));
            Assert.Equal(PspDaysOfWeek.Tuesday, SceRtc.sceRtcGetDayOfWeek(2012, 5, 1));
        }

        [Fact(Skip = "check. Time not mocked")]
        public void Test_sceRtcGetCurrentClock()
        {
            ScePspDateTime scePspDateTime;
            var result = SceRtc.sceRtcGetCurrentClock(out scePspDateTime, 0);
            Assert.Equal(0, result);
            Assert.Equal(_fakedYear, scePspDateTime.Year);
            Assert.Equal(_fakedMonth, scePspDateTime.Month);
            Assert.Equal(_fakedDay, scePspDateTime.Day);
            Assert.Equal(_fakedHour, scePspDateTime.Hour);
            Assert.Equal(_fakedMinute, scePspDateTime.Minute);
            Assert.Equal(_fakedSecond, scePspDateTime.Second);
            Assert.Equal(_fakedMillisecond, (int) scePspDateTime.Microsecond / 1000);
        }

        [Fact(Skip = "check. Time not mocked")]
        public void Test_timeIsIncreasing()
        {
            var prevDateTime = DateTimeRange.ConvertFromUnixTimestamp(0);
            for (int n = 0; n < 40; n++)
            {
                Console.WriteLine("Iter");
                _fakedMillisecond++;
                if (_fakedMillisecond >= 1000)
                {
                    _fakedSecond++;
                    _fakedMillisecond = _fakedMillisecond % 1000;
                }

                ScePspDateTime scePspDateTime;
                SceRtc.sceRtcGetCurrentClock(out scePspDateTime, 0);

                var currentDateTime = scePspDateTime.ToDateTime();

                if (!(currentDateTime > prevDateTime))
                {
                    Console.WriteLine("N: {0}", n);
                    Console.WriteLine("P: {0}", prevDateTime.Ticks);
                    Console.WriteLine("C: {0}", currentDateTime.Ticks);
                }
                Assert.True(currentDateTime.Ticks > prevDateTime.Ticks);
                prevDateTime = currentDateTime;
            }
        }
    }
}