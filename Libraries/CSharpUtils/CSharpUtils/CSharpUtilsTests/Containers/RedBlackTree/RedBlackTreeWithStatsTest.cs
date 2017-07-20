using System.Linq;
using CSharpUtils.Containers.RedBlackTree;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    public class RedBlackTreeWithStatsTest
    {
        RedBlackTreeWithStats<int> Stats;

        public RedBlackTreeWithStatsTest()
        {
            Stats = new RedBlackTreeWithStats<int>();
            Stats.Add(5);
            //Stats.PrintTree();
            //Console.WriteLine("-------------------------------");
            Stats.Add(4);
            //Stats.PrintTree();
            //Console.WriteLine("-------------------------------");
            Stats.Add(6);
            Stats.Add(3);
            Stats.Add(2);
            Stats.Add(1);
            Stats.Add(11);
            Stats.Add(7);
            Stats.Add(8);
            Stats.Add(9);
            Stats.Add(10);
            Stats.Add(0);
        }

        [Fact]
        public void Test1()
        {
            Stats.DebugValidateTree();
        }

        [Fact]
        public void Test2()
        {
            Assert.Equal(5, Stats.RealRootNode.Value);
            Assert.Equal(0, Stats.RealRootNode.LeftMostNode.Value);
            Assert.Equal(11, Stats.RealRootNode.RightMostNode.Value);

            Assert.Equal(12, Stats.All.Count);
            Assert.Equal(
                "0,1,2,3,4,5,6,7,8,9,10,11",
                Stats.All.ToStringArray()
            );
            foreach (var Item in Stats.All)
            {
                Assert.True(Stats.All.Contains(Item));
            }
            Assert.False(Stats.All.Contains(-1));
            Assert.False(Stats.All.Contains(12));
        }

        [Fact]
        public void Test3()
        {
            var Slice1 = Stats.All.Slice(1, 4);
            Assert.Equal(3, Slice1.Count);
            Assert.Equal(
                "1,2,3",
                Slice1.ToStringArray()
            );
            Assert.Equal(
                "0,4,5,6,7,8,9,10,11",
                Stats.All.Where(Item => !Slice1.Contains(Item)).ToStringArray()
            );
        }

        [Fact]
        public void Test4()
        {
            var Slice1 = Stats.All.Slice(1, 6).Slice(2, 4);
            Assert.Equal(2, Slice1.Count);
            Assert.Equal(
                "3,4",
                Slice1.ToStringArray()
            );
            Assert.Equal(
                "0,1,2,5,6,7,8,9,10,11",
                Stats.All.Where(Item => !Slice1.Contains(Item)).ToStringArray()
            );
        }

        [Fact]
        public void Test5()
        {
            var Stats2 = new RedBlackTreeWithStats<int>();
            Assert.Equal(0, Stats2.All.ToArray().Length);
        }

        [Fact]
        public void CappedCollectionTest()
        {
            var Stats2 = new RedBlackTreeWithStats<int>();
            Stats2.CappedToNumberOfElements = 4;
            Stats2.Add(0);
            Stats2.Add(1);
            Stats2.Add(2);
            Stats2.Add(3);
            Assert.Equal("0,1,2,3", Stats2.All.ToStringArray());
            Assert.Equal(4, Stats2.Count);
            Stats2.Add(4);
            Assert.Equal("0,1,2,3", Stats2.All.ToStringArray());
            Assert.Equal(4, Stats2.Count);
            Stats2.Add(-1);
            Assert.Equal("-1,0,1,2", Stats2.All.ToStringArray());
            Assert.Equal(4, Stats2.Count);
        }
    }
}