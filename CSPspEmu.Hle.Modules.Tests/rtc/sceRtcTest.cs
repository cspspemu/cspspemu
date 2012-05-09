using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core;
using CSPspEmu.Hle.Modules.rtc;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.Tests.rtc
{
	[TestClass]
	public class sceRtcTest : BaseModuleTest
	{
		[Inject]
		sceRtc sceRtc = null;

		const int FakedYear = 2012;
		const int FakedMonth = 4;
		const int FakedDay = 30;
		const int FakedHour = 12;
		const int FakedMinute = 13;
		const int FakedSecond = 14;
		const int FakedMillisecond = 973;

		class PspRtcMock : PspRtc
		{
			protected override void UpdateInternal()
			{
				var DateTime = new DateTime(FakedYear, FakedMonth, FakedDay, FakedHour, FakedMinute, FakedSecond, FakedMillisecond);
				CurrentTime.SetToDateTime(DateTime);
				this.CurrentDateTime = DateTime;
			}
		}

		protected override void SetMocks(PspEmulatorContext PspEmulatorContext)
		{
			PspEmulatorContext.SetInstanceType<PspRtc, PspRtcMock>();
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
	}
}
