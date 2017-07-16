using System;
using System.IO;
using CSharpUtils.Fastcgi;
using CSharpUtils.Streams;
using NUnit.Framework;

namespace CSharpUtilsTests
{
	[TestFixture]
	public class FastcgiHandlerTest
	{
		private static void WriteBeginRequest(Stream Stream)
		{
			Stream.WriteBytes(new byte[]
			{
				1, // Version
				(byte)Fastcgi.PacketType.FCGI_BEGIN_REQUEST, // Type
				0, 1, // RequestId
				0, 8, // Content Length
				0,    // Padding Length
				0,    // Reserved
				1, 2, 3, 4, 5, 6, 7, 8, // Data
			});
		}

		[Test]
		public void FastcgiHandlerConstructorTest()
		{
			var ConsumerMemoryStream = new ConsumerMemoryStream();
			WriteBeginRequest(ConsumerMemoryStream);

			var FastcgiHandler = new FastcgiHandler(new FastcgiPipeStream(ConsumerMemoryStream));
			FastcgiHandler.Reader.HandlePacket += delegate(Fastcgi.PacketType Type, ushort RequestId, byte[] Content)
			{
				Assert.AreEqual(Type, Fastcgi.PacketType.FCGI_BEGIN_REQUEST);
				Assert.AreEqual(RequestId, 1);
				Console.WriteLine(Content.Implode(","));
				CollectionAssert.AreEqual(Content, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

				return true;
			};
			FastcgiHandler.Reader.ReadPacket();
		}
	}
}
