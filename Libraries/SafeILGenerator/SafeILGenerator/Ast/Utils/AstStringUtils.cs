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
            var str = "";
            foreach (var Char in input)
            {
                switch (Char)
                {
                    case '\n':
                        str += "\\n";
                        break;
                    case '\r':
                        str += "\\r";
                        break;
                    case '\t':
                        str += "\\t";
                        break;
                    default:
                        str += Char;
                        break;
                }
            }
            return str;
        }

        public static string ToLiteral(string input) => '"' + ToLiteralRaw(input) + '"';

        public static string CaptureOutput(Action action, bool capture = true)
        {
            if (!capture)
            {
                action();
                return "";
            }

            var oldOut = Console.Out;
            var stringWriter = new StringWriter();
            try
            {
                Console.SetOut(stringWriter);
                action();
            }
            finally
            {
                Console.SetOut(oldOut);
            }

            try
            {
                return stringWriter.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}