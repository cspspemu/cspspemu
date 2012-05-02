using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpUtils
{
	static public class ConvertEx
	{
		static public int FlexibleToInt(String String)
		{
			var Regex = new Regex(@"^\d*", RegexOptions.Compiled);
			String Selected = Regex.Match(String).Groups[0].Value;
			int Value  =0;
			int.TryParse(Selected, out Value);
			return Value;
		}

		static public String GetString(this byte[] Bytes, Encoding Encoding)
		{
			return Encoding.GetString(Bytes);
		}

		static public String GetString(this byte[] Bytes)
		{
			return Bytes.GetString(Encoding.ASCII);
		}
	}
}
