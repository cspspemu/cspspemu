using CSPspEmu.Hle.Vfs.Local;
using NUnit.Framework;

namespace CSPspEmu.Core.Tests
{
	[TestFixture]
	public class HleIoDriverLocalFileSystemTest
	{
		[Test]
		public void GetSanitizedPathTest()
		{
			Assert.AreEqual("/test/1/2", HleIoDriverLocalFileSystem.GetSanitizedPath("/test/1/2"));
			Assert.AreEqual("/test/1/2", HleIoDriverLocalFileSystem.GetSanitizedPath("/test///1//2"));
			Assert.AreEqual("/test/1/2", HleIoDriverLocalFileSystem.GetSanitizedPath("/test/\\/1\\\\2"));
			Assert.AreEqual("1/2/3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/3"));
			Assert.AreEqual("1/3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/../3"));
			Assert.AreEqual("1/3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/.././././3"));
			Assert.AreEqual("3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/../../../../../3"));
			Assert.AreEqual("1/2/3/4", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/3/4/////"));
		}
	}
}
