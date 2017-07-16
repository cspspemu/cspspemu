using System.IO;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Misc;

namespace CSharpUtilsTests
{
	[TestClass]
	public class Acme1FileTest
	{
		[TestMethod]
		public void LoadTest()
		{
			var Acme1File = new Acme1File();
			var TestEncoding = Encoding.UTF8;
			Acme1File.Load(new MemoryStream(TestEncoding.GetBytes(@"
## POINTER 0 [t:1;r:1;ru:a;user:a;time:1246275105]
Text
With Line

## POINTER 2 [t:1;r:1;ru:a;user:a;time:1246275060]
	 Spaces at the beggining

## POINTER 3 [t:1;r:1;ru:b;user:a;time:1254188688]
Another single-line text.

## POINTER 43 [t:1;r:1;ru:b;user:b;time:1246275060]
Multi line text
with several new lines at the end.



## POINTER 571 [t:1;r:1;ru:b;user:b;time:1246275060]
End of the file.

")), TestEncoding);
			Assert.AreEqual(
				"Text\r\nWith Line",
				Acme1File[0].Text
			);
			Assert.AreEqual(
				"	 Spaces at the beggining",
				Acme1File[2].Text
			);
			Assert.AreEqual(
				"Another single-line text.",
				Acme1File[3].Text
			);
			Assert.AreEqual(
				"Multi line text\r\nwith several new lines at the end.",
				Acme1File[43].Text
			);
			Assert.AreEqual(
				"End of the file.",
				Acme1File[571].Text
			);
			Assert.AreEqual(
				"0,2,3,43,571",
				Acme1File.Select(Entry => Entry.Id).ToStringArray()
			);
		}
	}
}
