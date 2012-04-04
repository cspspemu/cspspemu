using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSPspEmu.Hle
{
	public class CStringFormater
	{
		internal class ArrayArgumentReader : IArgumentReader
		{
			int ArgumentIndex = 0;
			object[] Arguments;

			public ArrayArgumentReader(object[] Arguments)
			{
				this.Arguments = Arguments;
			}

			int IArgumentReader.LoadInteger()
			{
				return (int)long.Parse(Arguments[ArgumentIndex++].ToString());
			}

			long IArgumentReader.LoadLong()
			{
				return (long)long.Parse(Arguments[ArgumentIndex++].ToString());
			}

			string IArgumentReader.LoadString()
			{
				return (string)Arguments[ArgumentIndex++];
			}

			float IArgumentReader.LoadFloat()
			{
				return float.Parse(Arguments[ArgumentIndex++].ToString());
			}
		}

		static public string Sprintf(string Format, params object[] Arguments)
		{
			return Sprintf(Format, new ArrayArgumentReader(Arguments));
		}

		static Regex FormatRegex = new Regex(@"%(‘.|0|\x20)?(-)?(\d+)?(\.\d+)?(%|b|c|d|u|f|o|s|x|X)", RegexOptions.ECMAScript | RegexOptions.Compiled);

		static public string Sprintf(string Format, IArgumentReader Arguments)
		{
			var FormatReader = new StringReader(Format);
			return FormatRegex.Replace(Format, (Match) =>
			{
				var pPad = Match.Groups[1].Value;
				var pJustify = Match.Groups[2].Value;
				var pMinLength = Match.Groups[3].Value;
				var pPrecision = Match.Groups[4].Value;
				var pType = Match.Groups[5].Value;

				//Console.WriteLine("Match: pPad='{0}', pJustify='{1}', pMinLength='{2}', pPrecision='{3}', pType='{4}'", pPad, pJustify, pMinLength, pPrecision, pType);

				string Value = "";
				char PadChar = (pPad.Length > 0) ? pPad[0] : ' ';
				bool Justify = (pJustify.Length > 0) ? true : false;
				int MinLength = (pMinLength.Length > 0) ? int.Parse(pMinLength) : 0;

				switch (pType)
				{
					case "%": Value = "%"; break;
					case "b": Value = Convert.ToString(Arguments.LoadInteger(), 2); break;
					case "c": Value = "" + (char)Arguments.LoadInteger(); break;
					case "d": Value = Convert.ToString((int)Arguments.LoadInteger(), 10); break;
					case "u": Value = Convert.ToString((uint)Arguments.LoadInteger(), 10); break;
					case "f": Value = Convert.ToString(Arguments.LoadFloat()); break;
					case "o": Value = Convert.ToString((uint)Arguments.LoadInteger(), 8); break;
					case "s": Value = Arguments.LoadString(); break;
					case "x":
					case "X":
						Value = Convert.ToString((uint)Arguments.LoadInteger(), 16);
						Value =  (pType == "x") ? Value.ToLowerInvariant() : Value.ToUpperInvariant();
						break;
					default:
						throw(new NotImplementedException());
				}

				if (Value.Length < MinLength)
				{
					Value = Justify
						? Value.PadRight(MinLength, PadChar)
						: Value.PadLeft(MinLength, PadChar)
					;
				}

				return Value;
			});
		}
	}
}
