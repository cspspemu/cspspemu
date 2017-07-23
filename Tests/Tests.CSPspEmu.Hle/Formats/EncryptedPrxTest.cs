using System.IO;
using CSPspEmu.Hle.Formats;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats
{
    
    public class EncryptedPrxTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var encryptedPrx = new EncryptedPrx();
            var decryptedData = encryptedPrx.Decrypt(File.ReadAllBytes("../../../TestInput/EBOOT.BIN"));
            File.WriteAllBytes("../../../TestInput/test.bin", decryptedData);
            //File.ReadAllBytes("../../../TestInput/BOOT.BIN");
        }
    }
}