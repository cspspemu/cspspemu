using System.Collections.Generic;
using CSharpUtils.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class JSONTest
    {
        sealed class TestJsonSerializable : IJsonSerializable
        {
            public string ToJson()
            {
                return "TestJsonSerializable(123)";
            }
        }

        [TestMethod]
        public void StringifyNullTest()
        {
            Assert.AreEqual("null", Json.Stringify(null));
        }

        [TestMethod]
        public void StringifyStringTest()
        {
            Assert.AreEqual("\"Hello World!\"", Json.Stringify("Hello World!"));
        }

        [TestMethod]
        public void StringifyBooleanTest()
        {
            Assert.AreEqual("true", Json.Stringify(true));
            Assert.AreEqual("false", Json.Stringify(false));
        }

        [TestMethod]
        public void StringifyIntTest()
        {
            Assert.AreEqual("777", Json.Stringify(777));
        }

        [TestMethod]
        public void StringifyDoubleTest()
        {
            Assert.AreEqual("777.777", Json.Stringify(777.777));
        }

        [TestMethod]
        public void StringifyStringArrayTest()
        {
            Assert.AreEqual("[\"a\",\"b\",\"c\",\"d\"]", Json.Stringify(new string[] {"a", "b", "c", "d"}));
        }

        [TestMethod]
        public void StringifyIntArrayTest()
        {
            Assert.AreEqual("[1,2,3,4]", Json.Stringify(new int[] {1, 2, 3, 4}));
        }

        [TestMethod]
        public void StringifyDictionaryTest()
        {
            Assert.AreEqual("{\"one\":1,\"two\":2,\"three\":3,\"four\":4}", Json.Stringify(new Dictionary<string, int>
            {
                {"one", 1},
                {"two", 2},
                {"three", 3},
                {"four", 4},
            }));
        }

        [TestMethod]
        public void StringifyJsonSerializableTest()
        {
            Assert.AreEqual(
                "TestJsonSerializable(123)",
                Json.Stringify(new TestJsonSerializable())
            );
        }
    }
}