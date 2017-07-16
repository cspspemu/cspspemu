using CSharpUtilsTests;
using CSharpUtils.VirtualFileSystem.Zip;
using NUnit.Framework;

namespace Tests.CSharpUtils.Vfs.VirtualFileSystem.Zip
{
	[TestFixture]
	public class ZipFileSystemTest
	{
		[Test]
		public void TestMethod1()
		{
			var Vfs = new ZipFileSystem(Config.ProjectTestInputPath + @"\TestInputMounted.zip", ZipFileSystem.ZipArchiveMode.Read);

			Assert.AreEqual(
				"text.2",
				Vfs.OpenFile("DirectoryOnMountedFileSystem/2.txt", System.IO.FileMode.Open).ReadAllContentsAsString()
			);

			Assert.AreEqual(
				"FileSystemEntry(FullName=DirectoryOnMountedFileSystem/1.txt, Name=1.txt, Size=6, Type=File)" +
				"FileSystemEntry(FullName=DirectoryOnMountedFileSystem/2.txt, Name=2.txt, Size=6, Type=File)" +
				"FileSystemEntry(FullName=DirectoryOnMountedFileSystem/3.txt, Name=3.txt, Size=6, Type=File)"
				,
				Vfs.FindFiles("/DirectoryOnMountedFileSystem", "*.txt").ToStringArray("")
			);

			Assert.AreEqual(
				"FileSystemEntry(FullName=CompressedFile.txt, Name=CompressedFile.txt, Size=130, Type=File)" +
				"FileSystemEntry(FullName=DirectoryOnMountedFileSystem, Name=DirectoryOnMountedFileSystem, Size=0, Type=Directory)" +
				"FileSystemEntry(FullName=FileInMountedFileSystem.txt, Name=FileInMountedFileSystem.txt, Size=11, Type=File)"
				,
				Vfs.FindFiles("/").ToStringArray("")
			);
		}
	}
}
