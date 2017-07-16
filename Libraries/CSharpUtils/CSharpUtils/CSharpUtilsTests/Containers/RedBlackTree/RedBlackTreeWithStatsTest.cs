using System.Linq;
using CSharpUtils.Containers.RedBlackTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class RedBlackTreeWithStatsTest
    {
        RedBlackTreeWithStats<int> Stats;

        [TestInitialize]
        public void Initialize1()
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

        [TestMethod]
        public void Test1()
        {
            Stats.DebugValidateTree();
        }

        [TestMethod]
        public void Test2()
        {
            Assert.AreEqual(5, Stats.RealRootNode.Value);
            Assert.AreEqual(0, Stats.RealRootNode.LeftMostNode.Value);
            Assert.AreEqual(11, Stats.RealRootNode.RightMostNode.Value);

            Assert.AreEqual(12, Stats.All.Count);
            Assert.AreEqual(
                "0,1,2,3,4,5,6,7,8,9,10,11",
                Stats.All.ToStringArray()
            );
            foreach (var Item in Stats.All)
            {
                Assert.IsTrue(Stats.All.Contains(Item));
            }
            Assert.IsFalse(Stats.All.Contains(-1));
            Assert.IsFalse(Stats.All.Contains(12));
        }

        [TestMethod]
        public void Test3()
        {
            var Slice1 = Stats.All.Slice(1, 4);
            Assert.AreEqual(3, Slice1.Count);
            Assert.AreEqual(
                "1,2,3",
                Slice1.ToStringArray()
            );
            Assert.AreEqual(
                "0,4,5,6,7,8,9,10,11",
                Stats.All.Where(Item => !Slice1.Contains(Item)).ToStringArray()
            );
        }

        [TestMethod]
        public void Test4()
        {
            var Slice1 = Stats.All.Slice(1, 6).Slice(2, 4);
            Assert.AreEqual(2, Slice1.Count);
            Assert.AreEqual(
                "3,4",
                Slice1.ToStringArray()
            );
            Assert.AreEqual(
                "0,1,2,5,6,7,8,9,10,11",
                Stats.All.Where(Item => !Slice1.Contains(Item)).ToStringArray()
            );
        }

        [TestMethod]
        public void Test5()
        {
            var Stats2 = new RedBlackTreeWithStats<int>();
            Assert.AreEqual(0, Stats2.All.ToArray().Length);
        }

        [TestMethod]
        public void CappedCollectionTest()
        {
            var Stats2 = new RedBlackTreeWithStats<int>();
            Stats2.CappedToNumberOfElements = 4;
            Stats2.Add(0);
            Stats2.Add(1);
            Stats2.Add(2);
            Stats2.Add(3);
            Assert.AreEqual("0,1,2,3", Stats2.All.ToStringArray());
            Assert.AreEqual(4, Stats2.Count);
            Stats2.Add(4);
            Assert.AreEqual("0,1,2,3", Stats2.All.ToStringArray());
            Assert.AreEqual(4, Stats2.Count);
            Stats2.Add(-1);
            Assert.AreEqual("-1,0,1,2", Stats2.All.ToStringArray());
            Assert.AreEqual(4, Stats2.Count);
        }
    }
}