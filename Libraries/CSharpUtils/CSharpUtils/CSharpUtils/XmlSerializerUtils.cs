using System;
using System.IO;
using System.Xml.Serialization;

namespace CSharpUtils
{
    public static class XmlSerializerUtils
    {
        public static TType FromFile<TType>(String FileName)
        {
            var XmlSerializer = new XmlSerializer(typeof(TType));

            using (var XmlFile = File.OpenRead(FileName))
            {
                return (TType) XmlSerializer.Deserialize(XmlFile);
            }
        }

        public static void ToFile<TType>(String FileName, TType Object)
        {
            var XmlSerializer = new XmlSerializer(typeof(TType));

            Directory.CreateDirectory(new FileInfo(FileName).DirectoryName);

            using (var XmlFile = File.Open(FileName, FileMode.Create))
            {
                XmlSerializer.Serialize(XmlFile, Object);
            }
        }
    }
}