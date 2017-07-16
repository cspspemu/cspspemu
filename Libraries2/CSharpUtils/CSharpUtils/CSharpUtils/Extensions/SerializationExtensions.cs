using System;
using System.IO;
using System.Xml.Serialization;

public static class SerializationExtensions
{
	public static String ToJson<T>(this T Object, bool SingleQuotes = false)
	{
		return CSharpUtils.Json.JSON.Encode(Object, SingleQuotes);
	}

	public static String ToXmlString<T>(this T Struct)
	{
		var Serializer = new XmlSerializer(typeof(T));
		var StringWriter = new StringWriter();
		Serializer.Serialize(StringWriter, Struct);
		return StringWriter.ToString();
		//return Str
	}

	public static T FromXmlString<T>(this String XmlString)
	{
		var Serializer = new XmlSerializer(typeof(T));
		return (T)Serializer.Deserialize(new StringReader(XmlString));
	}
}
