using CSharpUtils.VirtualFileSystem;
using CSharpUtils.VirtualFileSystem.Local;
using CSharpUtils.VirtualFileSystem.Utils;
using NUnit.Framework;

namespace CSharpUtilsTests
{
	[TestFixture]
	public class SynchronizerTest
	{
		[SetUp]
		public void TestInitialize()
		{
			RestoreFileSystem();
		}

		[TearDown]
		public void TestCleanup()
		{
			RestoreFileSystem();
		}

		void RestoreFileSystem()
		{

		}

		[Test]
		public void SynchronizeTest()
		{
			FileSystem SourceFileSystem = new LocalFileSystem(Config.ProjectTestInputPath);
			FileSystem DestinationFileSystem = new LocalFileSystem(Config.ProjectTestOutputPath);
			Synchronizer.Synchronize(
				SourceFileSystem, ".",
				DestinationFileSystem, ".",
				Synchronizer.SynchronizationMode.CopyNewAndUpdateOldFiles,
				Synchronizer.ReferenceMode.SizeAndLastWriteTime
			);
			DestinationFileSystem.GetFileTime("ExistentFolder/2/AnotherFile.txt");
		}
	}
}
