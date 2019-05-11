using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;
using Xunit;

namespace Tests.CSPspEmu.Hle.Vfs.Iso
{
    
    public class IsoTest
    {
        [Fact(Skip = "file not found")]
        public void IsoConstructorTest()
        {
            var csoName = "../../../TestInput/test.cso";
            var cso = new Cso(File.OpenRead(csoName));
            var iso = new IsoFile(new CompressedIsoProxyStream(cso), csoName);
            var contentNode = iso.Root.Locate("path/content.txt");
            var lines = contentNode.Open().ReadAllContentsAsString().Split('\n');
            foreach (var line in lines)
            {
                iso.Root.Locate(line);
            }
        }
    }
}