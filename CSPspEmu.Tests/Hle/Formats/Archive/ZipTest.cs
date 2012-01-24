using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Hle.Formats.Archive;
using CSPspEmu.Resources;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Tests.Hle.Formats.Archive
{
	[TestClass]
	public class ZipTest
	{
		[TestMethod]
		public void TestUncompressedZip()
		{
			var Zip = new ZipArchive();
			Zip.Load("../../../TestInput/UncompressedZip.zip");
			foreach (var Entry in Zip)
			{
				Console.Error.WriteLine(Entry);
			}
		}

		[TestMethod]
		public void TestCompressedZip()
		{
			var Zip = new ZipArchive();
			Zip.Load("../../../TestInput/UncompressedZip.zip");
			var ExpectedString = "ffmpeg -i test.pmf -vcodec copy -an test.h264\nffmpeg -i test.pmf -acodec copy -vn test.ac3p";
			var ResultString = Zip["demux.bat"].OpenUncompressedStream().ReadAllContentsAsString(FromStart:false);
			Assert.AreEqual(ExpectedString, ResultString);
		}

		[TestMethod]
		public void TestZip2()
		{
			var Zip = new ZipArchive();
			Zip.Load(ResourceArchive.GetFlash0ZipFileStream());
			foreach (var Entry in Zip)
			{
				Console.Error.WriteLine(Entry);
			}
		}
	}
}
