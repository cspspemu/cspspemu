using CSPspEmu.Hle.Formats.video;
using NUnit.Framework;
using System;
using System.IO;

namespace CSPspEmu.Core.Tests
{
	[TestFixture]
	public class PmfTest
	{
		[Test]
		public void LoadTest()
		{
			var Pmf = new Pmf();
			Pmf.Load(File.OpenRead("../../../TestInput/test.pmf"));
		}
	}
}
