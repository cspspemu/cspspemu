using System.Collections.Generic;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class ListExtensionsTest
    {
        [Fact]
        public void LowerBoundTest()
        {
            var Items = new List<int>(new int[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100});
            Assert.Equal("70,60,50,40,30,20,10", Items.LowerBound(75).ToStringArray());
            Assert.Equal("70,60,50,40,30,20,10", Items.LowerBound(70, true).ToStringArray());
            Assert.Equal("60,50,40,30,20,10", Items.LowerBound(70, false).ToStringArray());

            Assert.Equal("70,80,90,100", Items.UpperBound(65).ToStringArray());
            Assert.Equal("70,80,90,100", Items.UpperBound(70, true).ToStringArray());
            Assert.Equal("80,90,100", Items.UpperBound(70, false).ToStringArray());
        }

        [Fact]
        public void LowerBoundEmptyTest()
        {
            var Items0 = new List<int>(new int[] { });
            Assert.Equal("", Items0.LowerBound(75).ToStringArray());
            Assert.Equal("", Items0.UpperBound(65).ToStringArray());
        }

        [Fact]
        public void LowerBoundOneTest()
        {
            var Items1 = new List<int>(new int[] {1});
            Assert.Equal("1", Items1.LowerBound(75).ToStringArray());
            Assert.Equal("", Items1.UpperBound(65).ToStringArray());
        }
    }
}