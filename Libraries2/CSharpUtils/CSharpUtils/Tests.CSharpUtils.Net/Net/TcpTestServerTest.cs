using CSharpUtils.Net;
using NUnit.Framework;

namespace CSharpUtilsTests
{
	[TestFixture]
	public class TcpTestServerTest
	{
		[Test]
		public void CreateTest()
		{
			var TestTcpTestServer = TcpTestServer.Create();
			TestTcpTestServer.RemoteTcpClient.GetStream().WriteStringz("Hello World");
			Assert.AreEqual("Hello World", TestTcpTestServer.LocalTcpClient.GetStream().ReadStringz());

			TestTcpTestServer.LocalTcpClient.GetStream().WriteStringz("Hello World");
			Assert.AreEqual("Hello World", TestTcpTestServer.RemoteTcpClient.GetStream().ReadStringz());
		}
	}
}
