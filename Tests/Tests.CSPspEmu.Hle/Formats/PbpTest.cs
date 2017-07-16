using CSPspEmu.Hle.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CSPspEmu.Core.Tests
{
    [TestClass]
    public class PbpTest
    {
        [TestMethod]
        public void LoadTest()
        {
            var Pbp = new Pbp();
            Pbp.Load(File.OpenRead("../../../TestInput/HelloJpcsp.pbp"));
        }
    }
}