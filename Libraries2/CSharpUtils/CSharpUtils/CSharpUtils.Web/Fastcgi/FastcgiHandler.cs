using System;
using System.Collections.Generic;
using System.Threading;

namespace CSharpUtils.Fastcgi
{
	public class FastcgiHandler
	{
		public bool Debug = false;
		public FastcgiPacketReader Reader;
		public FastcgiPacketWriter Writer;

		static byte[] Dummy = new byte[0];

		public delegate void HandleFastcgiRequestDelegate(FastcgiRequest FastcgiRequest);

		public event HandleFastcgiRequestDelegate HandleFastcgiRequest;

		protected Dictionary<ushort, FastcgiRequest> Requests = new Dictionary<ushort, FastcgiRequest>();

		public FastcgiHandler(IFastcgiPipe FastcgiPipe, bool Debug = false)
		{
			this.Writer = new FastcgiPacketWriter(FastcgiPipe, Debug);
			this.Reader = new FastcgiPacketReader(FastcgiPipe, Debug);
			this.Reader.HandlePacket += Reader_HandlePacket;
		}

		bool Reader_HandlePacket(Fastcgi.PacketType Type, ushort RequestId, byte[] Content)
		{
			var Request = GetOrCreateFastcgiRequest(RequestId);

			if (Debug)
			{
				Console.WriteLine("Handling Packet (" + Type + ")");
			}

			switch (Type)
			{
				case Fastcgi.PacketType.FCGI_BEGIN_REQUEST:
					var Role = (Fastcgi.Role)(Content[0] | (Content[1] << 8));
					var Flags = (Fastcgi.Flags)(Content[2]);
					break;
				case Fastcgi.PacketType.FCGI_PARAMS:
					if (Content.Length == 0)
					{
						Request.ParamsStream.Finalized = true;
					}
					else
					{
						Request.ParamsStream.Write(Content, 0, Content.Length);
					}
					break;
				case Fastcgi.PacketType.FCGI_STDIN:
					if (Content.Length == 0)
					{
						Request.StdinStream.Finalized = true;
						Request.FinalizedRequest = true;
					}
					else
					{
						Request.StdinStream.Write(Content, 0, Content.Length);
					}
					break;

				default:
					Console.Error.WriteLine("Unhandled packet type: '" + Type + "'");
					//throw (new Exception("Unhandled packet type: '" + Type + "'"));
					break;
			}

			if (Debug)
			{
				Console.WriteLine("     : FinalizedRequest(" + Request.FinalizedRequest + ")");
			}

			if (Request.ParamsStream.Finalized && Request.Processing == false)
			{
				Request.Processing = true;
				ThreadPool.QueueUserWorkItem(HandleRequest, Request);
			}
            

			if (Request.FinalizedRequest)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public void HandleRequest(Object _Request)
		{
			var Request = (FastcgiRequest)_Request;

			int Result = 0;
			try
			{
				Request.ParseParamsStream();
				HandleFastcgiRequest(Request);
				Result = 0;
			}
			catch (Exception Exception)
			{
				Result = -1;
				Console.Error.WriteLine(Exception);
			}
			Request.StdoutStream.Flush();
			Request.StderrStream.Flush();
			Writer.WritePacket(Request.RequestId, Fastcgi.PacketType.FCGI_STDOUT, Dummy, 0, 0);
			Writer.WritePacketEndRequest(Request.RequestId, Result, Fastcgi.ProtocolStatus.FCGI_REQUEST_COMPLETE);
			if (Debug)
			{
				Console.WriteLine("Completed Request(RequestId={0}, Result={1})", Request.RequestId, Result);
			}

			lock (Requests)
			{
				Requests.Remove(Request.RequestId);
				if (Requests.Count == 0)
				{
					Writer.FastcgiPipe.Close();
				}
			}
		}

		public FastcgiRequest GetOrCreateFastcgiRequest(ushort RequestId)
		{
			if (!Requests.ContainsKey(RequestId))
			{
				return Requests[RequestId] = new FastcgiRequest(this, RequestId);
			}
			return Requests[RequestId];
		}
	}
}
