using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class LinqXmlExExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string ToStringFull(this XDocument that)
        {
            return that.ToStringFull(Encoding.UTF8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToStringFull(this XDocument that, Encoding encoding)
        {
            var versionXmlStream = new MemoryStream();
            var versionXmlWriter = XmlWriter.Create(versionXmlStream, new XmlWriterSettings()
            {
                Encoding = encoding,
                Indent = true,
            });
            that.WriteTo(versionXmlWriter);
            versionXmlWriter.Flush();

            return encoding.GetString(versionXmlStream.ToArray());
        }
    }
}