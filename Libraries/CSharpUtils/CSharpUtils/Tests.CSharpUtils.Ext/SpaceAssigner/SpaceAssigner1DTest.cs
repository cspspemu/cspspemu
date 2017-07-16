using System;
using CSharpUtils.SpaceAssigner;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class SpaceAssigner1DTest
    {
        SpaceAssigner1D SpaceAssigner1D;

        [TestInitialize]
        public void TestInitialize()
        {
            this.SpaceAssigner1D = new SpaceAssigner1D();
        }

        [TestMethod]
        public void CombineContiguousTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 100));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(100, 200));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-100, 0));

            Assert.AreEqual("SpaceAssigner1D(Space(Min=-100, Max=200))", SpaceAssigner1D.ToString());
        }

        [TestMethod]
        public void CombineContiguous2Test()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(4, 5));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(3, 4));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-1, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 2));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 1));

            Assert.AreEqual("SpaceAssigner1D(Space(Min=-1, Max=2),Space(Min=3, Max=5))", SpaceAssigner1D.ToString());
        }

        [TestMethod]
        public void CombineContiguousDisorderedTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(3, 4));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-1, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 2));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 1));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-5, 6));

            //Console.WriteLine(SpaceAssigner1D);
            Assert.AreEqual("SpaceAssigner1D(Space(Min=-5, Max=6))", SpaceAssigner1D.ToString());
        }

        [TestMethod]
        public void SubstractTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-7, -4));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(5, 8));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(3, 4));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-1, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 2));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 1));

            SpaceAssigner1D.Substract(new SpaceAssigner1D.Space(-5, 6));

            //Console.WriteLine(SpaceAssigner1D);
            Assert.AreEqual("SpaceAssigner1D(Space(Min=-7, Max=-5),Space(Min=6, Max=8))", SpaceAssigner1D.ToString());
        }

        [TestMethod]
        public void AllocateTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-7, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 10));
            var Space3 = SpaceAssigner1D.Allocate(9);
            var Space1 = SpaceAssigner1D.Allocate(3);
            var Space2 = SpaceAssigner1D.Allocate(4);

            Assert.AreEqual(new SpaceAssigner1D.Space(-7, -4), Space1);
            Assert.AreEqual(new SpaceAssigner1D.Space(-4, 0), Space2);
            Assert.AreEqual(new SpaceAssigner1D.Space(1, 10), Space3);
            Assert.AreEqual("SpaceAssigner1D()", SpaceAssigner1D.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AllocateNotEnoughSpaceTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 10));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(10, 20));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(21, 100));
            var Space1 = SpaceAssigner1D.Allocate(200);
        }
    }
}