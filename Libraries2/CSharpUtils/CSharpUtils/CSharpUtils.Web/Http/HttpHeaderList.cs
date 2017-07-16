using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils.Http
{
	public class HttpHeaderList
	{
		protected Dictionary<string, List<HttpHeader>> Headers = new Dictionary<string, List<HttpHeader>>();

		public HttpHeaderList()
		{
		}

		public HttpHeaderList(String String)
		{
			Parse(String);
		}

		public HttpHeaderList(String[] Lines)
		{
			Parse(Lines);
		}

		public HttpHeader[] Get(String Name)
		{
			String NormalizedName = HttpHeader.GetNormalizedName(Name);
			if (Headers.ContainsKey(NormalizedName))
			{
				return Headers[NormalizedName].ToArray();
			}
			else {
				return new HttpHeader[0];
			}
		}

		public bool TryGetOne(String Name, out HttpHeader HttpHeader)
		{
			String NormalizedName = HttpHeader.GetNormalizedName(Name);

			List<HttpHeader> List;
			if (Headers.TryGetValue(NormalizedName, out List))
			{
				if (List.Count > 0)
				{
					HttpHeader = List[0];
					return true;
				}
				else
				{
					HttpHeader = null;
					return false;
				}
			}
			HttpHeader = null;
			return false;
		}

		public HttpHeader GetOne(String Name)
		{
			HttpHeader HttpHeader;
			if (!TryGetOne(Name, out HttpHeader))
			{
				throw (new KeyNotFoundException(String.Format("Can't find header '{0}'", Name)));
			}
			return HttpHeader;
		}

		public HttpHeaderList Set(String Name, String Value, bool Overwrite = true)
		{
			return _Set(Name, Value, false, Overwrite);
		}

		public HttpHeaderList Append(String Name, String Value)
		{
			return _Set(Name, Value, true);
		}

		public HttpHeaderList Remove(String Name)
		{
			Headers.Remove(Name);
			return this;
		}

		protected HttpHeaderList _Set(String _Name, String _Value, bool Append = false, bool Overwrite = true)
		{
			var HttpHeader = new HttpHeader(_Name, _Value);

			if (Append)
			{
				if (!Headers.ContainsKey(HttpHeader.NormalizedName))
				{
					Headers[HttpHeader.NormalizedName] = new List<HttpHeader>();
				}
				Headers[HttpHeader.NormalizedName].Add(HttpHeader);
			}
			else
			{
				if (Overwrite || !Headers.ContainsKey(HttpHeader.NormalizedName))
				{
					Headers[HttpHeader.NormalizedName] = new List<HttpHeader>() { HttpHeader };
				}
			}

			return this;
		}

		public HttpHeaderList SetCookie(String Name, String Value)
		{
			throw (new NotImplementedException());
			//Add("Set-Cookie", String.Format("{0}={1}", Name, Value));
			//return this;
		}

		public HttpHeaderList Parse(String[] Lines)
		{
			foreach (var Line in Lines)
			{
				String[] LineParts = Line.Split(new char[] { ':' }, 2);
				if (LineParts.Length == 2)
				{
					Append(LineParts[0], LineParts[1]);
				}
			}
			return this;
		}

		public HttpHeaderList Parse(String String)
		{
			return Parse(String.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
		}

		public HttpHeaderList WriteTo(TextWriter Output)
		{
			foreach (var Header in Headers)
			{
				foreach (var HeaderValue in Header.Value)
				{
					Output.WriteLine("{0}: {1}", HeaderValue.Name, HeaderValue.Value);
				}
			}
			Output.WriteLine("");
			return this;
		}
	}
}
