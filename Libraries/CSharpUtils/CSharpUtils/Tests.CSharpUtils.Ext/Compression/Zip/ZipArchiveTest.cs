using System.IO;
using System.Text;
using CSharpUtils.Ext.Compression.Zip;
using CSharpUtils.Extensions;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class ZipArchiveTest
    {
        [Test]
        public void LoadTest()
        {
            var ZipArchive = new ZipArchive();
            ZipArchive.Load(File.OpenRead(Config.ProjectTestInputPath + @"\TestInputMounted.zip"));
            var Text = ZipArchive.Files["CompressedFile.txt"].OpenRead().ReadAllContentsAsString(Encoding.UTF8, false);
            Assert.AreEqual(Text.Substr(-13), "compressible.");
        }
    }
}