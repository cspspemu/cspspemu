using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils;

namespace CSPspEmu.Tests
{
	[TestClass]
	public class PointerUtilsTest
	{
		[TestMethod]
		public void TestMemset()
		{
			var Data = new byte[131];
			PointerUtils.Memset(Data, 0x3E, Data.Length);

			CollectionAssert.AreEqual(
				((byte)0x3E).Repeat(Data.Length),
				Data
			);
		}
	}
}
