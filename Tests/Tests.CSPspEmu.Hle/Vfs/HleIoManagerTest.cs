using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Vfs.Local;
using Xunit;

namespace Tests.CSPspEmu.Hle.Vfs
{
    
    public class HleIoManagerTest
    {
        [Fact]
        public void ParsePathTest()
        {
            var injectContext = new InjectContext();
            var hleIoManager = injectContext.GetInstance<HleIoManager>();
            var driverName = "ms:";
            var driver = new HleIoDriverLocalFileSystem("C:/$INVALID$PATH$");
            hleIoManager.SetDriver(driverName, driver);

            var parts = hleIoManager.ParsePath("ms3:/path/to/file.txt");

            Assert.Equal(driver, parts.HleIoDrvFileArg.HleIoDriver);
            Assert.Equal(3, parts.HleIoDrvFileArg.FileSystemNumber);
            Assert.Equal("/path/to/file.txt", parts.LocalPath);
        }
    }
}