using CSPspEmu.Hle.Managers;
using NUnit.Framework;
using System;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Local;

namespace CSPspEmu.Core.Tests
{
    [TestFixture]
    public class HleIoManagerTest
    {
        [Test]
        public void ParsePathTest()
        {
            var InjectContext = new InjectContext();
            var HleIoManager = InjectContext.GetInstance<HleIoManager>();
            var DriverName = "ms:";
            var Driver = new HleIoDriverLocalFileSystem("C:/$INVALID$PATH$");
            HleIoManager.SetDriver(DriverName, Driver);

            var Parts = HleIoManager.ParsePath("ms3:/path/to/file.txt");

            Assert.AreEqual(Driver, Parts.HleIoDrvFileArg.HleIoDriver);
            Assert.AreEqual(3, Parts.HleIoDrvFileArg.FileSystemNumber);
            Assert.AreEqual("/path/to/file.txt", Parts.LocalPath);
        }
    }
}