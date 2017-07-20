using NUnit.Framework;
using CSPspEmu.Hle.Formats.Font;

namespace CSPspEmu.Core.Tests.Hle.Formats.Font
{
    [TestFixture]
    public class PGFTest
    {
        [Test]
        [Ignore("file not found")]
        public void TestMethod1()
        {
            var PGF = new PGF().Load("../../../TestInput/ltn0.pgf");
            var Bitmap = PGF.GetGlyph('H').Face.GetBitmap();
            Bitmap.Save("../../../TestOutput/test.png");
        }
    }
}