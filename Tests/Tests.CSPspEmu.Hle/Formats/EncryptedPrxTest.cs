using CSPspEmu.Hle.Formats;

using System.IO;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class EncryptedPrxTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var EncryptedPrx = new EncryptedPrx();
            var DecryptedData = EncryptedPrx.Decrypt(File.ReadAllBytes("../../../TestInput/EBOOT.BIN"));
            File.WriteAllBytes("../../../TestInput/test.bin", DecryptedData);
            //File.ReadAllBytes("../../../TestInput/BOOT.BIN");
        }
    }
}