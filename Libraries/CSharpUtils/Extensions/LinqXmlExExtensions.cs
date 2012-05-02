using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;

static public class LinqXmlExExtensions
{
	static public String ToStringFull(this XDocument That)
	{
		return That.ToStringFull(Encoding.UTF8);
	}

	static public String ToStringFull(this XDocument That, Encoding Encoding)
	{
		var VersionXmlStream = new MemoryStream();
		var VersionXmlWriter = XmlWriter.Create(VersionXmlStream, new XmlWriterSettings()
		{
			Encoding = Encoding,
			Indent = true,
		});
		That.WriteTo(VersionXmlWriter);
		VersionXmlWriter.Flush();

		return Encoding.GetString(VersionXmlStream.ToArray());
	}
		
}
