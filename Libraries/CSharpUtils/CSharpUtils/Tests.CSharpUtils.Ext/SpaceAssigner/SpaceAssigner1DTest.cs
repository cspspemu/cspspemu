using System;
using CSharpUtils.Ext.SpaceAssigner;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class SpaceAssigner1DTest
    {
        SpaceAssigner1D SpaceAssigner1D;

        public SpaceAssigner1DTest()
        {
            this.SpaceAssigner1D = new SpaceAssigner1D();
        }

        [Fact]
        public void CombineContiguousTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 100));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(100, 200));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-100, 0));

            Assert.Equal("SpaceAssigner1D(Space(Min=-100, Max=200))", SpaceAssigner1D.ToString());
        }

        [Fact]
        public void CombineContiguous2Test()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(4, 5));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(3, 4));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-1, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 2));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 1));

            Assert.Equal("SpaceAssigner1D(Space(Min=-1, Max=2),Space(Min=3, Max=5))", SpaceAssigner1D.ToString());
        }

        [Fact]
        public void CombineContiguousDisorderedTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(3, 4));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-1, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 2));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 1));

            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-5, 6));

            //Console.WriteLine(SpaceAssigner1D);
            Assert.Equal("SpaceAssigner1D(Space(Min=-5, Max=6))", SpaceAssigner1D.ToString());
        }

        [Fact]
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
            Assert.Equal("SpaceAssigner1D(Space(Min=-7, Max=-5),Space(Min=6, Max=8))", SpaceAssigner1D.ToString());
        }

        [Fact]
        public void AllocateTest()
        {
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(-7, 0));
            SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(1, 10));
            var Space3 = SpaceAssigner1D.Allocate(9);
            var Space1 = SpaceAssigner1D.Allocate(3);
            var Space2 = SpaceAssigner1D.Allocate(4);

            Assert.Equal(new SpaceAssigner1D.Space(-7, -4), Space1);
            Assert.Equal(new SpaceAssigner1D.Space(-4, 0), Space2);
            Assert.Equal(new SpaceAssigner1D.Space(1, 10), Space3);
            Assert.Equal("SpaceAssigner1D()", SpaceAssigner1D.ToString());
        }

        [Fact]
        public void AllocateNotEnoughSpaceTest()
        {
            Assert.Throws<Exception>(() =>
            {
                SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(0, 10));
                SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(10, 20));
                SpaceAssigner1D.AddAvailable(new SpaceAssigner1D.Space(21, 100));
                var Space1 = SpaceAssigner1D.Allocate(200);
            });
        }
    }
}