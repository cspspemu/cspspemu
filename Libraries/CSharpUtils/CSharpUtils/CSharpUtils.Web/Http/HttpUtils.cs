using System;
using System.Collections.Generic;

namespace CSharpUtils.Http
{
	public class HttpUtils
	{
		public static Dictionary<String, String> ParseUrlEncoded(String Url)
		{
			var Params = new Dictionary<String, String>();
			if (Url.Length > 0)
			{
				foreach (var Part in Url.Split('&'))
				{
					var Parts = Part.Split(new char[] { '=' }, 2);
					try
					{
						var Key = DecodeURIComponent(Parts[0]);
						var Value = DecodeURIComponent(Parts[1]);
						Params[Key] = Value;
					}
					catch
					{
					}
				}
			}
			return Params;
		}

		public static String DecodeURIComponent(String Input)
		{
			String Output = "";
			for (int n = 0; n < Input.Length; n++)
			{
				if (Input[n] == '%')
				{
					n++;
					// Unicode
					if (Input[n] == 'u')
					{
						n++;
						Output += (char)Convert.ToInt32(Input.Substring(n, 4), 16);
						n += 4;
					}
					// Normal
					else
					{
						Output += (char)Convert.ToInt32(Input.Substring(n, 2), 16);
						n += 2;
					}

					n--;
				}
				else
				{
					Output += Input[n];
				}
			}
			return Output;
		}
	}
}
