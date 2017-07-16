using System;
using System.Collections;
using System.Globalization;

namespace CSharpUtils.Json
{
    public class JSON
    {
        public static object Decode(string Format)
        {
            return Parse(Format);
        }

        public static string Encode(object ObjectToEncode, bool SingleQuotes = false)
        {
            return Stringify(ObjectToEncode, SingleQuotes);
        }

        public static object Parse(string Format)
        {
            throw (new NotImplementedException());
        }

        public static string Stringify(object ObjectToEncode, bool SingleQuotes = false)
        {
            if (ObjectToEncode == null)
            {
                return "null";
            }

            if (ObjectToEncode is string)
            {
                var Quote = SingleQuotes ? '\'' : '"';
                return Quote + Escape(ObjectToEncode as string) + Quote;
            }

            if (ObjectToEncode is bool)
            {
                return (((bool) ObjectToEncode) == true) ? "true" : "false";
            }

            if (ObjectToEncode is IDictionary)
            {
                var Dict = ObjectToEncode as IDictionary;
                var Str = "";
                foreach (var Key in Dict.Keys)
                {
                    var Value = Dict[Key];
                    if (Str.Length > 0) Str += ",";
                    Str += Stringify(Key.ToString(), SingleQuotes) + ":" + Stringify(Value, SingleQuotes);
                }
                return "{" + Str + "}";
            }

            if (ObjectToEncode is IEnumerable)
            {
                var List = ObjectToEncode as IEnumerable;
                var Str = "";
                foreach (var Item in List)
                {
                    if (Str.Length > 0) Str += ",";
                    Str += Stringify(Item, SingleQuotes);
                }
                return "[" + Str + "]";
            }

            var IJsonSerializable = ObjectToEncode as IJsonSerializable;
            if (IJsonSerializable != null)
            {
                return IJsonSerializable.ToJson();
            }

            double NumericResult;
            string NumericStr = Convert.ToString(ObjectToEncode, CultureInfo.InvariantCulture.NumberFormat);
            if (Double.TryParse(NumericStr, out NumericResult))
            {
                return NumericStr;
            }
            else
            {
                //throw (new NotImplementedException("Don't know how to encode '" + ObjectToEncode + "'."));
                return Stringify(ObjectToEncode.ToString(), SingleQuotes);
            }
        }

        protected static string Escape(string StringToEscape)
        {
            var Ret = "";
            foreach (var C in StringToEscape)
            {
                switch (C)
                {
                    case '/':
                    case '\"':
                    case '\'':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        Ret += '\\' + C;
                        break;
                    default:
                        if (C > 255)
                        {
                            Ret += "\\u" + Convert.ToString(C, 16).PadLeft(4, '0');
                        }
                        else
                        {
                            Ret += C;
                        }
                        break;
                }
            }
            return Ret;
        }
    }
}