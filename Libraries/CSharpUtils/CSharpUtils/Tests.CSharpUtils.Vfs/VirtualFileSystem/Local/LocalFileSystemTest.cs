using System;
using System.IO;
using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using NUnit.Framework;

namespace CSharpUtilsTests
{
	[TestFixture]
	public class LocalFileSystemTest
	{
		LocalFileSystem LocalFileSystem;

		[SetUp]
		public void InitializeTest()
		{
			LocalFileSystem = new LocalFileSystem(Config.ProjectTestInputPath);
			LocalFileSystem.Mount("/Mounted", new LocalFileSystem(Config.ProjectTestInputMountedPath));
			LocalFileSystem.Mount("/NewMounted", new LocalFileSystem(Config.ProjectTestInputMountedPath), "/DirectoryOnMountedFileSystem");
		}
		[Test]
		public void GetFileTimeExistsTest()
		{
			var FileTime = LocalFileSystem.GetFileTime("ExistentFolder");
			Assert.IsTrue(FileTime.CreationTime   >= DateTime.Parse("01/01/2011"));
			Assert.IsTrue(FileTime.LastAccessTime >= DateTime.Parse("01/01/2011"));
			Assert.IsTrue(FileTime.LastWriteTime  >= DateTime.Parse("01/01/2011"));
		}

		[ExpectedException(typeof(FileNotFoundException))]
		[Test]
		public void GetFileTimeNotExistsTest()
		{
			LocalFileSystem.GetFileTime("ThisFilesDoesNotExists");
		}

		[Test]
		public void MountedTest()
		{
			var FileTime = LocalFileSystem.GetFileTime("/Mounted/FileInMountedFileSystem.txt");
		}

		[Test]
		public void Mounted2Test()
		{
			var FileTime = LocalFileSystem.GetFileTime("/NewMounted/3.txt");
		}

		[Test]
		public void FindFilesTest()
		{
			Assert.IsTrue(LocalFileSystem.FindFiles("/Mounted").ContainsFileName("DirectoryOnMountedFileSystem"));
		}

		[Test]
		public void FindFilesWildcardTest()
		{
			var FoundFiles = LocalFileSystem.FindFiles("/Mounted/DirectoryOnMountedFileSystem", "*.dat");
			Assert.IsTrue(FoundFiles.ContainsFileName("3.dat"));
			Assert.IsTrue(FoundFiles.ContainsFileName("4.dat"));
			Assert.IsFalse(FoundFiles.ContainsFileName("1.txt"));
		}

		[Test]
		public void CopyFileTest()
		{
			var FoundFiles = LocalFileSystem.FindFiles("/Mounted/DirectoryOnMountedFileSystem", "*.dat");

			var SourcePath = "/Mounted/DirectoryOnMountedFileSystem/1.txt";
			var DestinationPath = "/Mounted/DirectoryOnMountedFileSystem/10.bin";

			try
			{
				LocalFileSystem.DeleteFile(DestinationPath);
			}
			catch
			{
			}

			LocalFileSystem.CopyFile(SourcePath, DestinationPath);

			Assert.AreEqual(
				LocalFileSystem.GetFileInfo(SourcePath).Size,
				LocalFileSystem.GetFileInfo(DestinationPath).Size
			);

			LocalFileSystem.DeleteFile(DestinationPath);
		}

		[Test]
		public void MountedOpenFileTest()
		{
			var Stream = LocalFileSystem.OpenFile("/Mounted/FileInMountedFileSystem.txt", FileMode.Open);
			var StreamReader = new StreamReader(Stream);
			Assert.AreEqual("Hello World", StreamReader.ReadToEnd());
			StreamReader.Close();
			Stream.Close();
		}

		[Test]
		public void MountedRecursiveTest()
		{
			var FileSystem1 = new ImplFileSystem();
			var FileSystem2 = new ImplFileSystem();
			FileSystem1.Mount("/Mounted1", FileSystem2);
			FileSystem2.Mount("/Mounted2/Mounted/test", LocalFileSystem);
			FileSystem1.GetFileTime("/Mounted1/Mounted2/Mounted/test/Mounted/../../test/Mounted/FileInMountedFileSystem.txt");
		}
	}
}
