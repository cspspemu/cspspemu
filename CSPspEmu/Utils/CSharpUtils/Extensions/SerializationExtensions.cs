using System.IO;
using System.Xml.Serialization;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="singleQuotes"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ToJson<T>(this T Object, bool singleQuotes = false)
        {
            return Json.Json.Encode(Object, singleQuotes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Struct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ToXmlString<T>(this T Struct)
        {
            var serializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, Struct);
            return stringWriter.ToString();
            //return Str
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FromXmlString<T>(this string xmlString)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T) serializer.Deserialize(new StringReader(xmlString));
        }
    }
}