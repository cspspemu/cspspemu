using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace CSPspEmu.Utils
{
    public class CStringFormater
    {
        internal class ArrayArgumentReader : IArgumentReader
        {
            int _argumentIndex = 0;
            readonly object[] _arguments;
            public ArrayArgumentReader(object[] arguments) => _arguments = arguments;
            int IArgumentReader.LoadInteger() => (int) long.Parse(_arguments[_argumentIndex++].ToString());
            long IArgumentReader.LoadLong() => long.Parse(_arguments[_argumentIndex++].ToString());
            string IArgumentReader.LoadString() => (string) _arguments[_argumentIndex++];
            float IArgumentReader.LoadFloat() => float.Parse(_arguments[_argumentIndex++].ToString());
        }

        public static string Sprintf(string format, params object[] arguments) => Sprintf(format, new ArrayArgumentReader(arguments));

        static Regex FormatRegex = new Regex(@"%(‘.|0|\x20)?(-)?(\d+)?(\.\d+)?(%|b|c|d|u|f|o|s|x|X)",
            RegexOptions.ECMAScript | RegexOptions.Compiled);

        public static string Sprintf(string format, IArgumentReader arguments)
        {
            //var formatReader = new StringReader(format);
            return FormatRegex.Replace(format, (match) =>
            {
                var pPad = match.Groups[1].Value;
                var pJustify = match.Groups[2].Value;
                var pMinLength = match.Groups[3].Value;
                var pPrecision = match.Groups[4].Value;
                var pType = match.Groups[5].Value;

                //Console.WriteLine("Match: pPad='{0}', pJustify='{1}', pMinLength='{2}', pPrecision='{3}', pType='{4}'", pPad, pJustify, pMinLength, pPrecision, pType);

                string value;
                var padChar = pPad.Length > 0 ? pPad[0] : ' ';
                var justify = pJustify.Length > 0;
                var minLength = (pMinLength.Length > 0) ? int.Parse(pMinLength) : 0;

                switch (pType)
                {
                    case "%":
                        value = "%";
                        break;
                    case "b":
                        value = Convert.ToString(arguments.LoadInteger(), 2);
                        break;
                    case "c":
                        value = "" + (char) arguments.LoadInteger();
                        break;
                    case "d":
                        value = Convert.ToString(arguments.LoadInteger(), 10);
                        break;
                    case "u":
                        value = Convert.ToString((uint) arguments.LoadInteger(), 10);
                        break;
                    case "f":
                        value = Convert.ToString(arguments.LoadFloat(), CultureInfo.DefaultThreadCurrentCulture);
                        break;
                    case "o":
                        value = Convert.ToString((uint) arguments.LoadInteger(), 8);
                        break;
                    case "s":
                        value = arguments.LoadString();
                        break;
                    case "x":
                    case "X":
                        value = Convert.ToString((uint) arguments.LoadInteger(), 16);
                        value = (pType == "x") ? value.ToLowerInvariant() : value.ToUpperInvariant();
                        break;
                    default:
                        throw(new NotImplementedException());
                }

                if (value.Length < minLength)
                {
                    value = justify
                            ? value.PadRight(minLength, padChar)
                            : value.PadLeft(minLength, padChar)
                        ;
                }

                return value;
            });
        }
    }
}