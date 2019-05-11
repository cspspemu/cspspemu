using CSharpUtils.Ext.SpaceAssigner;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class SpaceAssigner1D_SpaceTest
    {
        [Fact]
        public void IntersectionInnerTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-1, +1);

            Assert.Equal(Space2, Space1.Intersection(Space2));
            Assert.Equal(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [Fact]
        public void IntersectionNoneTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(+3, +6);

            Assert.Equal(null, Space1.Intersection(Space2));
            Assert.Equal(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [Fact]
        public void IntersectionExactTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-3, +3);

            Assert.Equal(Space1, Space1.Intersection(Space2));
            Assert.Equal(Space2, Space1.Intersection(Space2));
            Assert.Equal(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [Fact]
        public void IntersectionLeftTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-6, -1);

            Assert.Equal(new SpaceAssigner1D.Space(-3, -1), Space1.Intersection(Space2));
            Assert.Equal(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [Fact]
        public void IntersectionRightTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(+2, +100);

            Assert.Equal(new SpaceAssigner1D.Space(+2, 100), Space1.Intersection(Space2));
            Assert.Equal(Space1.Intersection(Space2), Space2.Intersection(Space1));
        }

        [Fact]
        public void SubstractInsideTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Space2 = new SpaceAssigner1D.Space(-2, +2);

            var Spaces = Space1 - Space2;
            Assert.Equal(2, Spaces.Length);
            Assert.Equal(new SpaceAssigner1D.Space(-3, -2), Spaces[0]);
            Assert.Equal(new SpaceAssigner1D.Space(+2, +3), Spaces[1]);
        }

        [Fact]
        public void SubstractExactTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - Space1;
            Assert.Equal(0, Spaces.Length);
        }

        [Fact]
        public void SubstractNoneOutsideTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(4, 4);
            Assert.Equal(1, Spaces.Length);
            Assert.Equal(Space1, Spaces[0]);
        }

        [Fact]
        public void SubstractNoneInsideTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(0, 0);
            Assert.Equal(1, Spaces.Length);
            Assert.Equal(Space1, Spaces[0]);
        }

        [Fact]
        public void SubstractLeftExactTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(-3, 0);
            Assert.Equal(1, Spaces.Length);
            Assert.Equal(new SpaceAssigner1D.Space(0, 3), Spaces[0]);
        }

        [Fact]
        public void SubstractLeftTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(-4, 0);
            Assert.Equal(1, Spaces.Length);
            Assert.Equal(new SpaceAssigner1D.Space(0, 3), Spaces[0]);
        }

        [Fact]
        public void SubstractRightTest()
        {
            var Space1 = new SpaceAssigner1D.Space(-3, +3);
            var Spaces = Space1 - new SpaceAssigner1D.Space(+2, +100);
            Assert.Equal(1, Spaces.Length);
            Assert.Equal(new SpaceAssigner1D.Space(-3, +2), Spaces[0]);
        }
    }
}