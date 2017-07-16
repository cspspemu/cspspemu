using System.Collections.Generic;
using CSharpUtils.Http;
using NUnit.Framework;

namespace CSharpUtilsTests.Http
{
	[TestFixture]
	public class HttpHeaderTest
	{
		[Test]
		public void TestMethod1()
		{
			HttpHeader ContentType = new HttpHeader("Content-Type", "multipart/form-data; boundary=----WebKitFormBoundaryIMw3ByBOPx38V6Bd");
			var ContentTypeParts = ContentType.ParseValue("type");
			CollectionAssert.AreEqual(new Dictionary<string, string>() {
				{ "type", "multipart/form-data" },
				{ "boundary", "----WebKitFormBoundaryIMw3ByBOPx38V6Bd" },
			}, ContentTypeParts);

			ContentType = new HttpHeader("Content-Type", "multipart/form-data; boundary=\"----WebKitFormBoundaryIMw3ByBOPx38V6Bd\"");
			ContentTypeParts = ContentType.ParseValue("type");
			CollectionAssert.AreEqual(new Dictionary<string, string>() {
				{ "type", "multipart/form-data" },
				{ "boundary", "----WebKitFormBoundaryIMw3ByBOPx38V6Bd" },
			}, ContentTypeParts);
		}
	}
}
