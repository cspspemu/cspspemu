using NUnit.Framework;
using CSharpUtils.Html;

namespace CSharpUtilsTests.Html
{
	[TestFixture]
	public class HtmlUtilsTest
	{
		[Test]
		public void TestEscapeHtmlCharacters()
		{
			Assert.AreEqual("&lt;p&gt;&quot;test", HtmlUtils.EscapeHtmlCharacters("<p>\"test"));
		}
	}
}
