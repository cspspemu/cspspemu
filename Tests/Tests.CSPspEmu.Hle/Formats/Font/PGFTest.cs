using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Hle.Formats.Font;

namespace CSPspEmu.Core.Tests.Hle.Formats.Font
{
	[TestClass]
	public class PGFTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var PGF = new PGF().Load("../../../TestInput/ltn0.pgf");
			var Bitmap = PGF.GetGlyph('H').Face.GetBitmap();
			Bitmap.Save("../../../TestOutput/test.png");
		}
	}
}
