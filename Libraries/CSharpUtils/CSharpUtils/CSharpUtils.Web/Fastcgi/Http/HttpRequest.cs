using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CSharpUtils.Http;

namespace CSharpUtils.Fastcgi.Http
{
	public class HttpRequest
	{
		public bool OutputDebug;
		public Encoding Encoding = Encoding.UTF8;
		public HttpHeaderList Headers;
		public TextWriter Output;

		public Dictionary<String, String> Enviroment;
		public Dictionary<String, String> Post;
		public Dictionary<String, HttpFile> Files;
		public Stream StdinStream;
		public Dictionary<String, String> Get;
		public Dictionary<String, String> Cookies;

		public void SetContentType(string MimeType, Encoding Encoding)
		{
			Headers.Set("Content-Type", MimeType + "; charset=" + Encoding.ToString());
			this.Encoding = Encoding;
		}
	}
}
