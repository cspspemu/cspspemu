using CSPspEmu.Hle.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class EncryptedPrxTest
	{
		[TestMethod]
		public void LoadTest()
		{
			var EncryptedPrx = new EncryptedPrx();
			EncryptedPrx.Load(File.ReadAllBytes("../../../TestInput/EBOOT.BIN"));
		}
	}
}
