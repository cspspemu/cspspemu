using CSPspEmu.Hle.Formats;

using System.IO;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class PbpTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var Pbp = new Pbp();
            Pbp.Load(File.OpenRead("../../../TestInput/HelloJpcsp.pbp"));
        }
    }
}