using CSPspEmu.Hle.Formats.Font;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats.Font
{
    
    public class PgfTest
    {
        [Fact(Skip = "file not found")]
        public void TestMethod1()
        {
            var pgf = new Pgf().Load("../../../TestInput/ltn0.pgf");
            var bitmap = pgf.GetGlyph('H').Face.GetBitmap();
            bitmap.Save("../../../TestOutput/test.png");
        }
    }
}