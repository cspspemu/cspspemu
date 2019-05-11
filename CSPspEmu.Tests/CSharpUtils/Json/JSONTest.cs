using System.Collections.Generic;
using CSharpUtils.Json;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class JSONTest
    {
        sealed class TestJsonSerializable : IJsonSerializable
        {
            public string ToJson()
            {
                return "TestJsonSerializable(123)";
            }
        }

        [Fact]
        public void StringifyNullTest()
        {
            Assert.Equal("null", Json.Stringify(null));
        }

        [Fact]
        public void StringifyStringTest()
        {
            Assert.Equal("\"Hello World!\"", Json.Stringify("Hello World!"));
        }

        [Fact]
        public void StringifyBooleanTest()
        {
            Assert.Equal("true", Json.Stringify(true));
            Assert.Equal("false", Json.Stringify(false));
        }

        [Fact]
        public void StringifyIntTest()
        {
            Assert.Equal("777", Json.Stringify(777));
        }

        [Fact]
        public void StringifyDoubleTest()
        {
            Assert.Equal("777.777", Json.Stringify(777.777));
        }

        [Fact]
        public void StringifyStringArrayTest()
        {
            Assert.Equal("[\"a\",\"b\",\"c\",\"d\"]", Json.Stringify(new string[] {"a", "b", "c", "d"}));
        }

        [Fact]
        public void StringifyIntArrayTest()
        {
            Assert.Equal("[1,2,3,4]", Json.Stringify(new int[] {1, 2, 3, 4}));
        }

        [Fact]
        public void StringifyDictionaryTest()
        {
            Assert.Equal("{\"one\":1,\"two\":2,\"three\":3,\"four\":4}", Json.Stringify(new Dictionary<string, int>
            {
                {"one", 1},
                {"two", 2},
                {"three", 3},
                {"four", 4},
            }));
        }

        [Fact]
        public void StringifyJsonSerializableTest()
        {
            Assert.Equal(
                "TestJsonSerializable(123)",
                Json.Stringify(new TestJsonSerializable())
            );
        }
    }
}