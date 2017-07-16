using System.IO;

public static class TextReaderExtensions
{
	public static bool HasMore(this TextReader TextReader)
	{
		return TextReader.Peek() >= 0;
	}

	public static char ReadChar(this TextReader TextReader)
	{
		return (char)TextReader.Read();
	}
}
