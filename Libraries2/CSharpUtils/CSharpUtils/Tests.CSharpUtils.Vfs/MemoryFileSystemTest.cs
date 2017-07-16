using System.IO;
using System.Text;
using System.Linq;
using CSharpUtils.VirtualFileSystem.Memory;
using NUnit.Framework;

namespace CSharpUtilsTests
{
	[TestFixture]
	public class MemoryFileSystemTest
	{
		[Test]
		public void AddFileTest()
		{
			var MemoryFileSystem = new MemoryFileSystem();
			MemoryFileSystem.AddFile("/./test.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello World")));
			MemoryFileSystem.AddFile("/./Directory/2.txt", new MemoryStream(Encoding.UTF8.GetBytes("2")));
			MemoryFileSystem.AddFile("/./Directory/Folder/4.txt", new MemoryStream(Encoding.UTF8.GetBytes("4")));
			MemoryFileSystem.AddFile("/./Directory/Folder/5.txt", new MemoryStream(Encoding.UTF8.GetBytes("5")));
			MemoryFileSystem.AddFile("/./1.txt", new MemoryStream(Encoding.UTF8.GetBytes("1")));
			MemoryFileSystem.AddFile("/./Directory/3.bin", new MemoryStream(Encoding.UTF8.GetBytes("3-bin")));

			CollectionAssert.AreEqual(
				new string[] { "2.txt", "Folder", "3.bin" },
				MemoryFileSystem.FindFiles("/Directory").Select(Item => Item.Name).ToArray()
			);

			Assert.AreEqual("3-bin", MemoryFileSystem.OpenFile("/Directory/3.bin", FileMode.Open).ReadAllContentsAsString(Encoding.UTF8));
		}
	}
}
