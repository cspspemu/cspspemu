using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CSharpUtils.VirtualFileSystem.Ssh;
using System.Net;
using System.IO;
using CSharpUtils;

namespace CSharpUtilsTests.VirtualFileSystem.Ssh
{
	[TestFixture]
	public class SftpFileSystemTest
	{
        // @TODO: These should be Harness tests.
        /*

		protected static SftpFileSystem SftpFileSystem;

		[ClassInitialize]
		public static void Initialize(TestContext testContext)
		{
			SftpFileSystem = new SftpFileSystem(Config.RemoteIp, 22, "ubuntu", "ubuntu", 1000);
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			SftpFileSystem.Shutdown();
		}

		[Test]
		public void FindFilesTest()
		{
			var FilesQuery =
				from File in SftpFileSystem.FindFiles("/home/ubuntu")
				where File.Name == "Desktop"
				select File
			;

			Assert.AreEqual(
				1,
				FilesQuery.Count()
			);
		}

		[Test]
		public void GetFileTimeTest()
		{
			var time = SftpFileSystem.GetFileTime("/home/ubuntu/this is a test.txt");
			Console.WriteLine(time);
		}

		[Test]
		public void DownloadFileTest()
		{
			Assert.AreEqual("Hello World\n", SftpFileSystem.OpenFile("/home/ubuntu/this is a test.txt", FileMode.Open).ReadAllContentsAsString(Encoding.UTF8));
		}

		[Test]
		public void ModifyFileTest()
		{
			//var fs = new FileStream(@"C:\projects\csharputils\temp.bin", FileMode.Create);
			//fs.Close();

			Stream Stream = SftpFileSystem.OpenFile("/home/ubuntu/myfile.txt", FileMode.Create);
			var StreamWriter = new StreamWriter(Stream);
			StreamWriter.Write("This is a string writed");
			StreamWriter.Close();
			//Stream.Close();

			Assert.AreEqual("This is a string writed", SftpFileSystem.OpenFile("/home/ubuntu/myfile.txt", FileMode.Open).ReadAllContentsAsString(Encoding.UTF8));
		}
        */
	}
}
