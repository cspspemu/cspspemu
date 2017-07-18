using System;
using System.Collections;
using System.Globalization;

namespace CSharpUtils.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class Json
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static object Decode(string format)
        {
            return Parse(format);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToEncode"></param>
        /// <param name="singleQuotes"></param>
        /// <returns></returns>
        public static string Encode(object objectToEncode, bool singleQuotes = false)
        {
            return Stringify(objectToEncode, singleQuotes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static object Parse(string format)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToEncode"></param>
        /// <param name="singleQuotes"></param>
        /// <returns></returns>
        public static string Stringify(object objectToEncode, bool singleQuotes = false)
        {
            if (objectToEncode == null)
            {
                return "null";
            }

            if (objectToEncode is string)
            {
                var quote = singleQuotes ? '\'' : '"';
                return quote + Escape(objectToEncode as string) + quote;
            }

            if (objectToEncode is bool)
            {
                return (bool) objectToEncode ? "true" : "false";
            }

            if (objectToEncode is IDictionary)
            {
                var dict = objectToEncode as IDictionary;
                var str = "";
                foreach (var key in dict.Keys)
                {
                    var value = dict[key];
                    if (str.Length > 0) str += ",";
                    str += Stringify(key.ToString(), singleQuotes) + ":" + Stringify(value, singleQuotes);
                }
                return "{" + str + "}";
            }

            if (objectToEncode is IEnumerable)
            {
                var list = objectToEncode as IEnumerable;
                var str = "";
                foreach (var item in list)
                {
                    if (str.Length > 0) str += ",";
                    str += Stringify(item, singleQuotes);
                }
                return "[" + str + "]";
            }

            var jsonSerializable = objectToEncode as IJsonSerializable;
            if (jsonSerializable != null)
            {
                return jsonSerializable.ToJson();
            }

            double numericResult;
            var numericStr = Convert.ToString(objectToEncode, CultureInfo.InvariantCulture.NumberFormat);
            return double.TryParse(numericStr, out numericResult) ? numericStr : Stringify(objectToEncode.ToString(), singleQuotes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringToEscape"></param>
        /// <returns></returns>
        protected static string Escape(string stringToEscape)
        {
            var ret = "";
            foreach (var c in stringToEscape)
            {
                switch (c)
                {
                    case '/':
                    case '\"':
                    case '\'':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        ret += '\\' + c;
                        break;
                    default:
                        if (c > 255)
                        {
                            ret += "\\u" + Convert.ToString(c, 16).PadLeft(4, '0');
                        }
                        else
                        {
                            ret += c;
                        }
                        break;
                }
            }
            return ret;
        }
    }
}