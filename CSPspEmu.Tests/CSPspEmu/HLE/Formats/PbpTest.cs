using System.IO;
using CSPspEmu.Hle.Formats;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats
{
    
    public class PbpTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var pbp = new Pbp();
            pbp.Load(File.OpenRead("../../../TestInput/HelloJpcsp.pbp"));
        }
    }
}