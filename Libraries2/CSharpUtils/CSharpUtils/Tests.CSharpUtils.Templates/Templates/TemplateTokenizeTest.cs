using System;
using System.Linq;
using NUnit.Framework;
using CSharpUtils.Templates.Tokenizers;

namespace CSharpUtilsTests.Templates
{
	/// <summary>
	/// Descripción resumida de TemplateTokenizeTest
	/// </summary>
	[TestFixture]
	public class TemplateTokenizeTest
	{
		[Test]
		public void TokenizeTest()
		{
			TokenizerAssertEquals(
				"Hello {{ 'User' + 15 }} Text ",
				"Hello ", "{{", "'User'", "+", "15", "}}", " Text "
			);
		}

		[Test]
		public void Tokenize2Test()
		{
			TokenizerAssertEquals(
				"Hello {% for n in [1, 2, 3, 4] %}{{ n }}{% endfor %}",
				"Hello ", "{%", "for", "n", "in", "[", "1", ",", "2", ",", "3", ",", "4", "]", "%}", "{{", "n", "}}", "{%", "endfor", "%}"
			);
		}

		[Test]
		public void TokenizeBinary()
		{
			TokenizerAssertEquals(
				"{{ 1 + 2 * 3 / 4 % 5 // 6 ** 7 }}",
				"{{", "1", "+", "2", "*", "3", "/", "4", "%", "5", "//", "6", "**", "7", "}}"
			);
		}

		[Test]
		public void TokenizeTernary()
		{
			TokenizerAssertEquals(
				"{{ 1 ? 1 : 0 }}",
				"{{", "1", "?", "1", ":", "0", "}}"
			);
		}

		[Test]
		public void TokenizeFilter()
		{
			TokenizerAssertEquals(
				"{{ 'Hello {0}'|format('World') }}",
				"{{", "'Hello {0}'", "|", "format", "(", "'World'", ")", "}}"
			);
		}

		[Test]
		public void TokenizeEscapeStart()
		{
			TokenizerAssertEquals(
				"A{{ '{{' }}B",
				"A", "{{", "'{{'", "}}", "B"
			);
		}

		[Test]
		public void TokenizeEscapeEnd()
		{
			TokenizerAssertEquals(
				"A{{ '}}' }}B",
				"A", "{{", "'}}'", "}}", "B"
			);
		}

		[Test]
		public void TokenizeRange()
		{
			TokenizerAssertEquals(
				"{% for n in 0..10 %}",
				"{%", "for", "n", "in", "0", "..", "10", "%}"
			);
		}

		[Test]
		public void TokenizeUnary()
		{
			TokenizerAssertEquals(
				"{{ -(1 + 2) + -3  }}",
				"{{", "-", "(", "1", "+", "2", ")", "+", "-", "3", "}}"
			);
		}

		[Test]
		public void TokenizeAccessDot()
		{
			TokenizerAssertEquals(
				"{{ test.key.subkey }}",
				"{{", "test", ".", "key", ".", "subkey", "}}"
			);
		}

		[Test]
		public void TokenizeComments()
		{
			TokenizerAssertEquals(
				"Hello {# World #}{{ test }}A{# Test #}B",
				"Hello ", "{{", "test", "}}", "A", "B"
			);
		}

		protected void TokenizerAssertEquals(String TemplateString, params String[] Tokens)
		{
			var StringTokens = TemplateTokenizer.Tokenize(new TokenizerStringReader(TemplateString)).Select(Token => Token.Text).ToArray();
			foreach (var StringToken in StringTokens)
			{
				Console.WriteLine(StringToken);
			}
			CollectionAssert.AreEqual(StringTokens, Tokens);
		}
	}
}
