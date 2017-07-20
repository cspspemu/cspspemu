using System.IO;
using System.Xml.Serialization;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class XmlSerializerUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static TType FromFile<TType>(string fileName)
        {
            var xmlSerializer = new XmlSerializer(typeof(TType));

            using (var xmlFile = File.OpenRead(fileName))
            {
                return (TType) xmlSerializer.Deserialize(xmlFile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="Object"></param>
        /// <typeparam name="TType"></typeparam>
        public static void ToFile<TType>(string fileName, TType Object)
        {
            var xmlSerializer = new XmlSerializer(typeof(TType));

            var directoryName = new FileInfo(fileName).DirectoryName;
            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
            }

            using (var xmlFile = File.Open(fileName, FileMode.Create))
            {
                xmlSerializer.Serialize(xmlFile, Object);
            }
        }
    }
}