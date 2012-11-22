using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core;
using CSPspEmu.Hle.Modules.rtc;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Vfs;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.Tests.rtc
{
	[TestClass]
	public class sceRtcTest : BaseModuleTest
	{
		[Inject]
		sceRtc sceRtc = null;

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

		[TestInitialize]
		public void TestInitialize()
		{
			ResetTimes();
		}

		protected override void SetMocks(PspEmulatorContext PspEmulatorContext)
		{
			PspEmulatorContext.SetInstanceType<PspRtc, PspRtcMock>();
			(PspEmulatorContext.GetInstance<PspRtc>() as PspRtcMock).sceRtcTest = this;
		}

		[TestMethod]
		public void Test_sceRtcGetDayOfWeek()
		{
            Assert.AreEqual((int)PspDaysOfWeek.Monday, 1);
			Assert.AreEqual(PspDaysOfWeek.Monday, sceRtc.sceRtcGetDayOfWeek(2012, 4, 30));
            Assert.AreEqual(2, (int)sceRtc.sceRtcGetDayOfWeek(2012, 5, 1));
			Assert.AreEqual(PspDaysOfWeek.Tuesday, sceRtc.sceRtcGetDayOfWeek(2012, 5, 1));
		}

		[TestMethod]
		public void Test_sceRtcGetCurrentClock()
		{
			ScePspDateTime ScePspDateTime;
			var Result = sceRtc.sceRtcGetCurrentClock(out ScePspDateTime, 0);
			Assert.AreEqual(0, Result);
			Assert.AreEqual(FakedYear, (int)ScePspDateTime.Year);
			Assert.AreEqual(FakedMonth, (int)ScePspDateTime.Month);
			Assert.AreEqual(FakedDay, (int)ScePspDateTime.Day);
			Assert.AreEqual(FakedHour, (int)ScePspDateTime.Hour);
			Assert.AreEqual(FakedMinute, (int)ScePspDateTime.Minute);
			Assert.AreEqual(FakedSecond, (int)ScePspDateTime.Second);
			Assert.AreEqual(FakedMillisecond, (int)ScePspDateTime.Microsecond / 1000);
		}

		[TestMethod]
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
				Assert.IsTrue(CurrentDateTime.Ticks > PrevDateTime.Ticks);
				PrevDateTime = CurrentDateTime;
			}
		}
	}
}
