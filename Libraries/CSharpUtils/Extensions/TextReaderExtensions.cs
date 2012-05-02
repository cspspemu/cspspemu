using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

static public class TextReaderExtensions
{
	static public bool HasMore(this TextReader TextReader)
	{
		return TextReader.Peek() >= 0;
	}

	static public char ReadChar(this TextReader TextReader)
	{
		return (char)TextReader.Read();
	}
}
