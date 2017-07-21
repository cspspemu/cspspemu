using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;
using Xunit;


namespace CSPspEmu.Core.Tests
{
    
    public class IsoTest
    {
        [Fact(Skip = "file not found")]
        public void IsoConstructorTest()
        {
            var CsoName = "../../../TestInput/test.cso";
            var Cso = new Cso(File.OpenRead(CsoName));
            var Iso = new IsoFile(new CompressedIsoProxyStream(Cso), CsoName);
            var ContentNode = Iso.Root.Locate("path/content.txt");
            var Lines = ContentNode.Open().ReadAllContentsAsString().Split('\n');
            foreach (var Line in Lines)
            {
                Iso.Root.Locate(Line);
            }
        }
    }
}