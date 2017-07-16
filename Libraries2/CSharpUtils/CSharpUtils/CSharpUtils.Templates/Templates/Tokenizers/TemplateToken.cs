using System;
using CSharpUtils.Templates.Utils;

namespace CSharpUtils.Templates.Tokenizers
{
	public class TemplateToken
	{
		public String Text;
		//public String OriginalText;
	}

	public class RawTemplateToken : TemplateToken
	{
	}

	public class OpenTagTemplateToken : TemplateToken
	{
	}

	public class CloseTagTemplateToken : TemplateToken
	{
	}

	public class StringLiteralTemplateToken : TemplateToken
	{
		public String UnescapedText
		{
			get
			{
				return StringUtils.UnescapeString(Text);
			}
		}
	}

	public class NumericLiteralTemplateToken : TemplateToken
	{
	}

	public class OperatorTemplateToken : TemplateToken
	{
	}

	public class IdentifierTemplateToken : TemplateToken
	{
	}

	public class EndOfFileTemplateToken : TemplateToken
	{
	}
}
