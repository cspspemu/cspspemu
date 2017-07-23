using System;
using System.IO;
using CSPspEmu.Hle.Formats;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats
{
    
    public class PsfTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var psf = new Psf();
            psf.Load(File.OpenRead("../../../TestInput/PARAM.SFO"));
            foreach (var pair in psf.EntryDictionary)
            {
                Console.WriteLine("{0}:{1}", pair.Key, pair.Value);
            }

            Assert.Equal(psf.EntryDictionary["BOOTABLE"], 1);
            Assert.Equal(psf.EntryDictionary["CATEGORY"], "UG");
            Assert.Equal(psf.EntryDictionary["DISC_ID"], "TEST99999");
            Assert.Equal(psf.EntryDictionary["DISC_NUMBER"], 1);
            Assert.Equal(psf.EntryDictionary["DISC_TOTAL"], 1);
            Assert.Equal(psf.EntryDictionary["DISC_VERSION"], "9.99");
            Assert.Equal(psf.EntryDictionary["PARENTAL_LEVEL"], 5);
            Assert.Equal(psf.EntryDictionary["PSP_SYSTEM_VER"], "3.33");
            Assert.Equal(psf.EntryDictionary["REGION"], 32768);
            Assert.Equal(psf.EntryDictionary["TITLE"], "GAME TITLE TITLE");
        }
    }
}