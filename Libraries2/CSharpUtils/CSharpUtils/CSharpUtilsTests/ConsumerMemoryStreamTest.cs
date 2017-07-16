using System;
using CSharpUtils.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
	[TestClass]
	public class ConsumerMemoryStreamTest
	{
		[TestMethod]
		public void WriteTest()
		{
			var Stream = new ConsumerMemoryStream();
			Stream.WriteByte(1);
			Stream.WriteByte(2);
			Stream.WriteBytes(new byte[] { 3, 4, 5 });
			var ReadedBlock1 = Stream.ReadBytes(3);
			CollectionAssert.AreEqual(ReadedBlock1, new byte[] { 1, 2, 3 });
			Stream.WriteBytes(new byte[] { 6, 7 });
			var ReadedBlock2 = Stream.ReadBytes(4);
			CollectionAssert.AreEqual(ReadedBlock2, new byte[] { 4, 5, 6, 7 });
			Console.WriteLine(Stream.Contents.Implode(","));
			CollectionAssert.AreEqual(Stream.Contents, new byte[] { });
		}
	}
}
