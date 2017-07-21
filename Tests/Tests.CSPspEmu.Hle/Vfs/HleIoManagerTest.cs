using CSPspEmu.Hle.Managers;

using CSPspEmu.Hle.Vfs.Local;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class HleIoManagerTest
    {
        [Fact]
        public void ParsePathTest()
        {
            var InjectContext = new InjectContext();
            var HleIoManager = InjectContext.GetInstance<HleIoManager>();
            var DriverName = "ms:";
            var Driver = new HleIoDriverLocalFileSystem("C:/$INVALID$PATH$");
            HleIoManager.SetDriver(DriverName, Driver);

            var Parts = HleIoManager.ParsePath("ms3:/path/to/file.txt");

            Assert.Equal(Driver, Parts.HleIoDrvFileArg.HleIoDriver);
            Assert.Equal(3, Parts.HleIoDrvFileArg.FileSystemNumber);
            Assert.Equal("/path/to/file.txt", Parts.LocalPath);
        }
    }
}