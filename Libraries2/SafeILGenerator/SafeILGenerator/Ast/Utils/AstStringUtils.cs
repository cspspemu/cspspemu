using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Utils
{
	public class AstStringUtils
	{
		public static string ToLiteralRaw(string input)
		{
			var Str = "";
			foreach (var Char in input)
			{
				switch (Char)
				{
					case '\n': Str += "\\n"; break;
					case '\r': Str += "\\r"; break;
					case '\t': Str += "\\t"; break;
					default: Str += Char; break;
				}
			}
			return Str;
		}

		public static string ToLiteral(string input)
		{
			return '"' + ToLiteralRaw(input) + '"';
		}

		public static String CaptureOutput(Action Action, bool Capture = true)
		{
			if (Capture)
			{
				var OldOut = Console.Out;
				var StringWriter = new StringWriter();
				try
				{
					Console.SetOut(StringWriter);
					Action();
				}
				finally
				{
					Console.SetOut(OldOut);
				}
				try
				{
					return StringWriter.ToString();
				}
				catch
				{
					return "";
				}
			}
			else
			{
				Action();
				return "";
			}
		}
	}
}
