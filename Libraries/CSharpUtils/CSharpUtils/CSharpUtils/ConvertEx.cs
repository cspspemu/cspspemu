using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpUtils
{
	public static class ConvertEx
	{
		public static int FlexibleToInt(String String)
		{
			var Regex = new Regex(@"^\d*", RegexOptions.Compiled);
			String Selected = Regex.Match(String).Groups[0].Value;
			int Value  =0;
			int.TryParse(Selected, out Value);
			return Value;
		}

		public static String GetString(this byte[] Bytes, Encoding Encoding)
		{
			return Encoding.GetString(Bytes);
		}

		public static String GetString(this byte[] Bytes)
		{
			return Bytes.GetString(Encoding.ASCII);
		}
	}
}
