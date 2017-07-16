using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class SerializationExtensionsTest
    {
        [TestMethod]
        public void ToJsonTest()
        {
            Assert.AreEqual(
                "{\"Hello\":1,\"World\":-1}",
                new Dictionary<String, int>()
                {
                    {"Hello", 1},
                    {"World", -1},
                }.ToJson()
            );
        }

        public struct SampleStruct
        {
            public String Id;
            public int[] Values;
        }

        [TestMethod]
        public void ToXmlStringTest()
        {
            Assert.AreEqual(
                "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                "<SampleStruct xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
                "  <Id>Test</Id>\r\n" +
                "  <Values>\r\n" +
                "    <int>0</int>\r\n" +
                "    <int>1</int>\r\n" +
                "    <int>2</int>\r\n" +
                "    <int>3</int>\r\n" +
                "  </Values>\r\n" +
                "</SampleStruct>",
                new SampleStruct()
                {
                    Id = "Test",
                    Values = new int[] {0, 1, 2, 3},
                }.ToXmlString()
            );
        }

        [TestMethod]
        public void FromXmlStringTest()
        {
            var SampleStruct = new SampleStruct()
            {
                Id = "Test",
                Values = new int[] {0, 1, 2, 3},
            };

            var SampleStructString1 = SampleStruct.ToXmlString();
            var SampleStructString2 = SampleStructString1.FromXmlString<SampleStruct>().ToXmlString();

            Assert.AreEqual(SampleStructString1, SampleStructString2);
        }
    }
}