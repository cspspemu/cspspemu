using CSPspEmu.Hle.Formats;
using NUnit.Framework;
using System.IO;

namespace CSPspEmu.Core.Tests
{
    [TestFixture]
    public class EncryptedPrxTest
    {
        [Test]
        public void LoadTest()
        {
            var EncryptedPrx = new EncryptedPrx();
            var DecryptedData = EncryptedPrx.Decrypt(File.ReadAllBytes("../../../TestInput/EBOOT.BIN"));
            File.WriteAllBytes("../../../TestInput/test.bin", DecryptedData);
            //File.ReadAllBytes("../../../TestInput/BOOT.BIN");
        }
    }
}