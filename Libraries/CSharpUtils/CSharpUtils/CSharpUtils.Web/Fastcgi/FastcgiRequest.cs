using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpUtils.Fastcgi
{
	public class FastcgiRequest
	{
		public bool Processing;
		public bool FinalizedRequest = false;
		public ushort RequestId;
		public FastcgiHandler FastcgiHandler;
		public FascgiRequestInputStream ParamsStream;
		public FascgiRequestInputStream StdinStream;
		public FascgiResponseOutputStream StdoutStream;
		public FascgiResponseOutputStream StderrStream;
		public Dictionary<String, String> PostParams;

		public FastcgiRequest(FastcgiHandler FastcgiHandler, ushort RequestId)
		{
			this.FastcgiHandler = FastcgiHandler;
			this.RequestId = RequestId;
			this.ParamsStream = new FascgiRequestInputStream();
			this.StdinStream = new FascgiRequestInputStream();
			this.StdoutStream = new FascgiResponseOutputStream(this, Fastcgi.PacketType.FCGI_STDOUT);
			this.StderrStream = new FascgiResponseOutputStream(this, Fastcgi.PacketType.FCGI_STDERR);
		}

		public void ParseParamsStream()
		{
			Params = new Dictionary<string, string>();
			ParamsStream.Position = 0;
			while (!ParamsStream.Eof())
			{
				var Key = new byte[FastcgiPacketReader.ReadVariableInt(ParamsStream)];
				var Value = new byte[FastcgiPacketReader.ReadVariableInt(ParamsStream)];
				ParamsStream.Read(Key, 0, Key.Length);
				ParamsStream.Read(Value, 0, Value.Length);
				Params[Encoding.ASCII.GetString(Key)] = Encoding.ASCII.GetString(Value);
			}
			ParamsStream = new FascgiRequestInputStream();
		}

		public Dictionary<String, String> Params = new Dictionary<string, string>();

		public String GetParam(String Key)
		{
			String Value;
			return Params.TryGetValue(Key, out Value) ? Value : "";
		}
	}
}
