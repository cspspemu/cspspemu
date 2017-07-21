
using CSPspEmu.Hle.Formats.Font;
using Xunit;

namespace CSPspEmu.Core.Tests.Hle.Formats.Font
{
    
    public class PGFTest
    {
        [Fact(Skip = "file not found")]
        public void TestMethod1()
        {
            var PGF = new PGF().Load("../../../TestInput/ltn0.pgf");
            var Bitmap = PGF.GetGlyph('H').Face.GetBitmap();
            Bitmap.Save("../../../TestOutput/test.png");
        }
    }
}