using CSharpUtils.SpaceAssigner;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class SpaceAssigner1D_SpaceTest
    {
        [TestMethod]
        public void IntersectionInnerTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-1, +1);

            Assert.AreEqual(Space2, Space1.Intersection(Space2));
            Assert.AreEqual(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [TestMethod]
        public void IntersectionNoneTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(+3, +6);

            Assert.AreEqual(null, Space1.Intersection(Space2));
            Assert.AreEqual(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [TestMethod]
        public void IntersectionExactTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-3, +3);

            Assert.AreEqual(Space1, Space1.Intersection(Space2));
            Assert.AreEqual(Space2, Space1.Intersection(Space2));
            Assert.AreEqual(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [TestMethod]
        public void IntersectionLeftTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-6, -1);

            Assert.AreEqual(new SpaceAssigner1D.Space(-3, -1), Space1.Intersection(Space2));
            Assert.AreEqual(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [TestMethod]
        public void IntersectionRightTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(+2, +100);

            Assert.AreEqual(new SpaceAssigner1D.Space(+2, 100), Space1.Intersection(Space2));
            Assert.AreEqual(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [TestMethod]
        public void SubstractInsideTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-2, +2);

            var Spaces = Space1 - Space2;
            Assert.AreEqual(2, Spaces.Length);
            Assert.AreEqual(new SpaceAssigner1D.Space(-3, -2), Spaces[0]);
            Assert.AreEqual(new SpaceAssigner1D.Space(+2, +3), Spaces[1]);
        }

        [TestMethod]
        public void SubstractExactTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - Space1;
            Assert.AreEqual(0, Spaces.Length);
        }

        [TestMethod]
        public void SubstractNoneOutsideTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(4, 4);
            Assert.AreEqual(1, Spaces.Length);
            Assert.AreEqual(Space1, Spaces[0]);
        }

        [TestMethod]
        public void SubstractNoneInsideTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(0, 0);
            Assert.AreEqual(1, Spaces.Length);
            Assert.AreEqual(Space1, Spaces[0]);
        }

        [TestMethod]
        public void SubstractLeftExactTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(-3, 0);
            Assert.AreEqual(1, Spaces.Length);
            Assert.AreEqual(new SpaceAssigner1D.Space(0, 3), Spaces[0]);
        }

        [TestMethod]
        public void SubstractLeftTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(-4, 0);
            Assert.AreEqual(1, Spaces.Length);
            Assert.AreEqual(new SpaceAssigner1D.Space(0, 3), Spaces[0]);
        }

        [TestMethod]
        public void SubstractRightTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(+2, +100);
            Assert.AreEqual(1, Spaces.Length);
            Assert.AreEqual(new SpaceAssigner1D.Space(-3, +2), Spaces[0]);
        }
    }
}
