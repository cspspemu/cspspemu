using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace CSharpUtils
{
	static public class XmlSerializerUtils
	{
		static public TType FromFile<TType>(String FileName)
		{
			var XmlSerializer = new XmlSerializer(typeof(TType));

			using (var XmlFile = File.OpenRead(FileName))
			{
				return (TType)XmlSerializer.Deserialize(XmlFile);
			}
		}

		static public void ToFile<TType>(String FileName, TType Object)
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
