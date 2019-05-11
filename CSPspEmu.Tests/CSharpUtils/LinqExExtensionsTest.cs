using System.Linq;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class LinqExExtensionsTest
    {
        public struct TestStruct
        {
            public int Index;
            public string Title;

            public TestStruct(string Title = "", int Index = 0)
            {
                this.Title = Title;
                this.Index = Index;
            }

            public override string ToString()
            {
                return $"TestStruct(Index:{Index}, Title:'{Title}')";
            }
        }

        [Fact]
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

            Assert.Equal(
                new int[] {0, 1, 2, 3, 4, 5},
                OrderedItems.Select(Item => Item.Index).ToArray()
            );
        }
    }
}