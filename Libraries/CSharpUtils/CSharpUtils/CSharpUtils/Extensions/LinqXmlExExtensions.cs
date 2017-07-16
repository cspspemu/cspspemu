using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public static class LinqXmlExExtensions
{
    public static String ToStringFull(this XDocument That)
    {
        return That.ToStringFull(Encoding.UTF8);
    }

    public static String ToStringFull(this XDocument That, Encoding Encoding)
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