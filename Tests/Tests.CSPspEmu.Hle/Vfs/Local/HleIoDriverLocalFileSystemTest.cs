using CSPspEmu.Hle.Vfs.Local;
using Xunit;


namespace CSPspEmu.Core.Tests
{
    
    public class HleIoDriverLocalFileSystemTest
    {
        [Fact]
        public void GetSanitizedPathTest()
        {
            Assert.Equal("/test/1/2", HleIoDriverLocalFileSystem.GetSanitizedPath("/test/1/2"));
            Assert.Equal("/test/1/2", HleIoDriverLocalFileSystem.GetSanitizedPath("/test///1//2"));
            Assert.Equal("/test/1/2", HleIoDriverLocalFileSystem.GetSanitizedPath("/test/\\/1\\\\2"));
            Assert.Equal("1/2/3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/3"));
            Assert.Equal("1/3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/../3"));
            Assert.Equal("1/3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/.././././3"));
            Assert.Equal("3", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/../../../../../3"));
            Assert.Equal("1/2/3/4", HleIoDriverLocalFileSystem.GetSanitizedPath("1/2/3/4/////"));
        }
    }
}