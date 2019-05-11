using System.IO;
using System.Text;
using CSharpUtils.Ext.Compression.Zip;
using CSharpUtils.Extensions;

using Xunit;

namespace CSharpUtilsTests
{
    public class ZipArchiveTest
    {
        [Fact(Skip = "Fails on mono")]
        public void LoadTest()
        {
            var ZipArchive = new ZipArchive();
            ZipArchive.Load(File.OpenRead(Config.ProjectTestInputPath + @"\TestInputMounted.zip"));
            var Text = ZipArchive.Files["CompressedFile.txt"].OpenRead().ReadAllContentsAsString(Encoding.UTF8, false);
            Assert.Equal(Text.Substr(-13), "compressible.");
        }
    }
}