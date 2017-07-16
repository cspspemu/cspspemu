using CSPspEmu.Hle.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CSPspEmu.Core.Tests
{
    [TestClass]
    public class EncryptedPrxTest
    {
        [TestMethod]
        public void LoadTest()
        {
            var EncryptedPrx = new EncryptedPrx();
            var DecryptedData = EncryptedPrx.Decrypt(File.ReadAllBytes("../../../TestInput/EBOOT.BIN"));
            File.WriteAllBytes("../../../TestInput/test.bin", DecryptedData);
            //File.ReadAllBytes("../../../TestInput/BOOT.BIN");
        }
    }
}