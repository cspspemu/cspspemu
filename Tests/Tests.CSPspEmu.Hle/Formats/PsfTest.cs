using CSPspEmu.Hle.Formats;

using System;
using System.IO;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class PsfTest
    {
        [Fact(Skip = "file not found")]
        public void LoadTest()
        {
            var Psf = new Psf();
            Psf.Load(File.OpenRead("../../../TestInput/PARAM.SFO"));
            foreach (var Pair in Psf.EntryDictionary)
            {
                Console.WriteLine("{0}:{1}", Pair.Key, Pair.Value);
            }

            Assert.Equal(Psf.EntryDictionary["BOOTABLE"], 1);
            Assert.Equal(Psf.EntryDictionary["CATEGORY"], "UG");
            Assert.Equal(Psf.EntryDictionary["DISC_ID"], "TEST99999");
            Assert.Equal(Psf.EntryDictionary["DISC_NUMBER"], 1);
            Assert.Equal(Psf.EntryDictionary["DISC_TOTAL"], 1);
            Assert.Equal(Psf.EntryDictionary["DISC_VERSION"], "9.99");
            Assert.Equal(Psf.EntryDictionary["PARENTAL_LEVEL"], 5);
            Assert.Equal(Psf.EntryDictionary["PSP_SYSTEM_VER"], "3.33");
            Assert.Equal(Psf.EntryDictionary["REGION"], 32768);
            Assert.Equal(Psf.EntryDictionary["TITLE"], "GAME TITLE TITLE");
        }
    }
}