using CSPspEmu.Hle.Formats.video;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class PmfTest
	{
		[TestMethod]
		public void LoadTest()
		{
			var Pmf = new Pmf();
			Pmf.Load(File.OpenRead("../../../TestInput/test.pmf"));
		}
	}
}
