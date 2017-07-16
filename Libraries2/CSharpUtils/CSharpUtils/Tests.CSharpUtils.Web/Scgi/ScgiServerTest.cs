using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using CSharpUtils.Http;
using CSharpUtils.Net;
using CSharpUtils.Scgi;
using NUnit.Framework;

namespace CSharpUtilsTests
{
	[TestFixture]
	public class ScgiServerTest
	{
		sealed class TestScgiServer : ScgiServer
		{
			public AutoResetEvent HandleRequestEvent = new AutoResetEvent(false);

			public TestScgiServer(string BindIp, int BindPort) : base(BindIp, BindPort)
			{
			}

			protected override void HandleRequest(HttpHeaderList HttpHeaderList, Dictionary<string, string> Parameters, byte[] PostContent)
			{
				HandleRequestEvent.Set();
			}
		}

		[Test]
		public void ScgiServerConstructorTest()
		{
			string BindIp = "127.0.0.1";
			int BindPort = NetworkUtilities.GetAvailableTcpPort();
			var ScgiServer = new TestScgiServer(BindIp, BindPort);
			ScgiServer.Listen();
			new Thread(ScgiServer.AcceptLoop).Start();

			var TcpClient = new TcpClient(BindIp, BindPort);
			var TcpClientStream = TcpClient.GetStream();
			var HeaderStream = new MemoryStream().PreservePositionAndLock((Stream) =>
			{
				Stream.WriteStringzPair("CONTENT_LENGTH", "0");
				Stream.WriteStringzPair("SCGI", "1");
				Stream.WriteStringzPair("REQUEST_METHOD", "GET");
				Stream.WriteStringzPair("REQUEST_URI", "/test");
			});
			TcpClientStream.WriteString(HeaderStream.Length + ":");
			TcpClientStream.WriteBytes(HeaderStream.ToArray());
			TcpClientStream.WriteByte((byte)',');
			Assert.IsTrue(ScgiServer.HandleRequestEvent.WaitOne(1000));
		}
	}
}
