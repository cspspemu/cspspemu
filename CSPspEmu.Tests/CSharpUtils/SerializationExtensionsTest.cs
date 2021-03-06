﻿using System.Collections.Generic;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class SerializationExtensionsTest
    {
        [Fact]
        public void ToJsonTest()
        {
            Assert.Equal(
                "{\"Hello\":1,\"World\":-1}",
                new Dictionary<string, int>()
                {
                    {"Hello", 1},
                    {"World", -1},
                }.ToJson()
            );
        }

        public struct SampleStruct
        {
            public string Id;
            public int[] Values;
        }

        [Fact(Skip = "Fails on mono")]
        public void ToXmlStringTest()
        {
            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                "<SampleStruct>\r\n" +
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
                    .Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "")
                .Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "")
            );
        }

        [Fact]
        public void FromXmlStringTest()
        {
            var SampleStruct = new SampleStruct()
            {
                Id = "Test",
                Values = new int[] {0, 1, 2, 3},
            };

            var SampleStructString1 = SampleStruct.ToXmlString();
            var SampleStructString2 = SampleStructString1.FromXmlString<SampleStruct>().ToXmlString();

            Assert.Equal(SampleStructString1, SampleStructString2);
        }
    }
}