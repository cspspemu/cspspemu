using System;
using CSPspEmu.Hle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSPspEmu.Core.Tests.Hle
{
	[TestClass]
	public class CStringFormaterTest
	{
		[TestMethod]
		public void TestSprintf()
		{
			var Expected = "Hello 0x00000001, 'World     '!";
			var Actual = CStringFormater.Sprintf("Hello 0x%08X, '%-10s'!", 1, "World");

			Assert.AreEqual(Expected, Actual);
		}
	}
}
