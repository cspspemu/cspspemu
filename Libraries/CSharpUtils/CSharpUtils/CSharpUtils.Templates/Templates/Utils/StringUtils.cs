using System;

namespace CSharpUtils.Templates.Utils
{
	public static class StringUtils
	{
		public static String EscapeString(String Value)
		{
			String EscapedString = "";

			if (Value == null)
			{
				Console.Error.WriteLine("EscapeString.Value=null");
			}

			for (int n = 0; n < Value.Length; n++)
			{
				switch (Value[n])
				{
					case '\n': EscapedString += @"\n"; break;
					case '\r': EscapedString += @"\r"; break;
					case '\t': EscapedString += @"\t"; break;
					case '"': EscapedString += "\\\""; break;
					default: EscapedString += Value[n]; break;
				}
			}

			return '"' + EscapedString + '"';
		}

		public static String UnescapeString(String Value)
		{
			if (Value.Length < 2) throw(new Exception("Invalid String [1]"));
			if (Value[0] != '\'' && Value[0] != '"') throw (new Exception("Invalid String [2]"));
			if (Value.Substr(0, 1) != Value.Substr(-1, 1)) throw (new Exception("Invalid String [3]"));
			String RetString = "";
			Value = Value.Substr(1, -1);
			for (int n = 0; n < Value.Length; n++)
			{
				if (Value[n] == '\\')
				{
					switch (Value[++n])
					{
						case 'n': RetString += '\n'; break;
						case 'r': RetString += '\r'; break;
						case 't': RetString += '\t'; break;
						default: throw(new Exception("Unknown Escape Sequence"));
					}
				}
				else
				{
					RetString += Value[n];
				}
			}

			return RetString;
		}
	}
}
