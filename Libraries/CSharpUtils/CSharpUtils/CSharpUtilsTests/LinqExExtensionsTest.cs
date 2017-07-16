using System;
using System.Linq;
using CSharpUtils.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class LinqExExtensionsTest
    {
        public struct TestStruct
        {
            public int Index;
            public String Title;

            public TestStruct(String Title = "", int Index = 0)
            {
                this.Title = Title;
                this.Index = Index;
            }

            public override string ToString()
            {
                return String.Format("TestStruct(Index:{0}, Title:'{1}')", Index, Title);
            }
        }

        [TestMethod]
        public void OrderByNaturalTest()
        {
            var Items = new TestStruct[]
            {
                new TestStruct(Index: 2, Title: "Test 3"),
                new TestStruct(Index: 3, Title: "Test 10"),
                new TestStruct(Index: 0, Title: "Test 1"),
                new TestStruct(Index: 4, Title: "Test 11"),
                new TestStruct(Index: 1, Title: "Test 2"),
                new TestStruct(Index: 5, Title: "Test 21"),
            };
            var OrderedItems = Items.OrderByNatural(Item => Item.Title).ToArray();

            CollectionAssert.AreEqual(
                new int[] {0, 1, 2, 3, 4, 5},
                OrderedItems.Select(Item => Item.Index).ToArray()
            );
        }
    }
}