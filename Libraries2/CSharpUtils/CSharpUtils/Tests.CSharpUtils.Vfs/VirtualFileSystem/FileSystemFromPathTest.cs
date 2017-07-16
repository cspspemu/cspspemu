using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using NUnit.Framework;

namespace CSharpUtilsTests.VirtualFileSystem
{
	[TestFixture]
	public class FileSystemFromPathTest
	{
		[Test]
		public void FileSystemFromPathConstructorTest()
		{
			var LocalFileSystem = new LocalFileSystem(Config.ProjectTestInputMountedPath);
			var LocalFileSystemAccessed = LocalFileSystem.FileSystemFromPath("DirectoryOnMountedFileSystem", false);
			Assert.IsTrue(LocalFileSystemAccessed.ExistsFile("1.txt"));
			Assert.IsFalse(LocalFileSystemAccessed.ExistsFile("../FileInMountedFileSystem.txt"));
		}
	}
}
