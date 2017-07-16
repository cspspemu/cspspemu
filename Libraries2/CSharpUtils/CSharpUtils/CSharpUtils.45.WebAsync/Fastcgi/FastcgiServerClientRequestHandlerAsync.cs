using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpUtils;

namespace CSharpUtils.Web._45.Fastcgi
{
	public class FastcgiServerClientRequestHandlerAsync
	{
		protected Stream ClientStream;
		protected ushort RequestId;
		public MemoryStream ParamsStream = new MemoryStream();
		public FastcgiRequestAsync FastcgiRequestAsync;
		public FastcgiResponseAsync FastcgiResponseAsync;
		internal FastcgiServerClientHandlerAsync FastcgiServerClientHandlerAsync;

		public FastcgiServerClientRequestHandlerAsync(FastcgiServerClientHandlerAsync FastcgiServerClientHandlerAsync, Stream ClientStream, ushort RequestId)
		{
			this.FastcgiServerClientHandlerAsync = FastcgiServerClientHandlerAsync;
			this.ClientStream = ClientStream;
			this.RequestId = RequestId;
			this.FastcgiRequestAsync = new FastcgiRequestAsync()
			{
				StdinStream = new FastcgiInputStream(),
			};
			this.FastcgiResponseAsync = new FastcgiResponseAsync()
			{
				StdoutStream = new FastcgiOutputStream(),
				StderrStream = new FastcgiOutputStream(),
				Headers = new FastcgiHeaders(),
			};

			this.FastcgiResponseAsync.StdoutWriter = new StreamWriter(this.FastcgiResponseAsync.StdoutStream);
			this.FastcgiResponseAsync.StdoutWriter.AutoFlush = true;

			this.FastcgiResponseAsync.StderrWriter = new StreamWriter(this.FastcgiResponseAsync.StderrStream);
			this.FastcgiResponseAsync.StderrWriter.AutoFlush = true;
		}

		protected static int ReadVariable(Stream Stream)
		{
			int Value = 0;
			byte Data;
			do
			{
				Data = (byte)Stream.ReadByte();
				Value <<= 7;
				Value |= Data & 0x7F;
			} while ((Data & 0x80) != 0);
			return Value;
		}

		public async Task HandlePacket(TcpClient Client, FastcgiPacket Packet)
		{
			if (FastcgiServerClientHandlerAsync.FastcgiServerAsync.Debug)
			{
				await Console.Out.WriteLineAsync(String.Format("HandlePacket"));
			}
			var Content = Packet.Content.Array;
			var ContentLength = Content.Length;

			switch (Packet.Type)
			{
				case Fastcgi.PacketType.FCGI_BEGIN_REQUEST:
					var Role = (Fastcgi.Role)(Content[0] | (Content[1] << 8));
					var Flags = (Fastcgi.Flags)(Content[2]);
					break;
				case Fastcgi.PacketType.FCGI_PARAMS:
					if (Content.Length == 0)
					{
						ParamsStream.Position = 0;
						FastcgiRequestAsync.Params = new Dictionary<string, string>();
						while (ParamsStream.Position < ParamsStream.Length)
						{
							int KeyLength = ReadVariable(ParamsStream);
							int ValueLength = ReadVariable(ParamsStream);
							var Key = ParamsStream.ReadString(KeyLength, Encoding.UTF8);
							var Value = ParamsStream.ReadString(ValueLength, Encoding.UTF8);
							FastcgiRequestAsync.Params[Key] = Value;
						}
						//Request.ParamsStream.Finalized = true;
					}
					else
					{
						ParamsStream.Write(Content, 0, ContentLength);
					}
					break;
				case Fastcgi.PacketType.FCGI_STDIN:
					if (Content.Length == 0)
					{
						FastcgiRequestAsync.StdinStream.Position = 0;
						Exception Exception = null;
						var Stopwatch = new Stopwatch();

						Stopwatch.Start();
						try
						{
							await FastcgiServerClientHandlerAsync.FastcgiServerAsync.HandleRequestAsync(this.FastcgiRequestAsync, this.FastcgiResponseAsync);
						}
						catch (Exception _Exception)
						{
							Exception = _Exception;
						}
						Stopwatch.Stop();

						if (Exception != null)
						{
							var StreamWriter = new StreamWriter(FastcgiResponseAsync.StderrStream);
							StreamWriter.WriteLine(String.Format("{0}", Exception));
							StreamWriter.Flush();
						}
						var HeaderPlusOutputStream = new MemoryStream();

						var HeaderStream = new MemoryStream();
						var HeaderStreamWriter = new StreamWriter(HeaderStream);
						HeaderStreamWriter.AutoFlush = true;

						FastcgiResponseAsync.Headers.Add("Content-Type", "text/html");
						FastcgiResponseAsync.Headers.Add("X-Time", Stopwatch.Elapsed.ToString());

						foreach (var Header in FastcgiResponseAsync.Headers.Headers)
						{
							HeaderStreamWriter.Write("{0}: {1}\r\n", Header.Key, Header.Value);
						}
						HeaderStreamWriter.Write("\r\n");

						HeaderStream.Position = 0;
						HeaderStream.CopyToFast(HeaderPlusOutputStream);
						FastcgiResponseAsync.StdoutStream.Position = 0;
						FastcgiResponseAsync.StdoutStream.CopyToFast(HeaderPlusOutputStream);
						HeaderPlusOutputStream.Position = 0;

						await FastcgiPacket.WriteMemoryStreamToAsync(RequestId: RequestId, PacketType: Fastcgi.PacketType.FCGI_STDOUT, From: HeaderPlusOutputStream, ClientStream: ClientStream);
						await FastcgiPacket.WriteMemoryStreamToAsync(RequestId: RequestId, PacketType: Fastcgi.PacketType.FCGI_STDERR, From: FastcgiResponseAsync.StderrStream, ClientStream: ClientStream);

						await new FastcgiPacket() { Type = Fastcgi.PacketType.FCGI_STDOUT, RequestId = RequestId, Content = new ArraySegment<byte>() }.WriteToAsync(ClientStream);
						await new FastcgiPacket() { Type = Fastcgi.PacketType.FCGI_STDERR, RequestId = RequestId, Content = new ArraySegment<byte>() }.WriteToAsync(ClientStream);
						await new FastcgiPacket() { Type = Fastcgi.PacketType.FCGI_END_REQUEST, RequestId = RequestId, Content = new ArraySegment<byte>(new byte[] { 0, 0, 0, 0, (byte)Fastcgi.ProtocolStatus.FCGI_REQUEST_COMPLETE }) }.WriteToAsync(ClientStream);
						//await ClientStream.FlushAsync();
						ClientStream.Close();
					}
					else
					{
						await FastcgiRequestAsync.StdinStream.WriteAsync(Content, 0, ContentLength);
					}
					break;
				default:
					Console.Error.WriteLine("Unhandled packet type: '" + Packet.Type + "'");
					Client.Close();
					//throw (new Exception("Unhandled packet type: '" + Type + "'"));
					break;
			}
		}
	}
}
