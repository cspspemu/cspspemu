using CSharpUtils.Factory;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class MockeableFactoryTest
    {
        class A
        {
            public virtual string Value
            {
                get { return "A"; }
            }
        }

        class B : A
        {
            public override string Value
            {
                get { return "B"; }
            }
        }

        [Fact]
        public void MockTypeTest()
        {
            var MockeableFactory = new MockeableFactory();
            Assert.Equal("A", MockeableFactory.New<A>().Value);
            MockeableFactory.MockType(typeof(A), typeof(B));
            Assert.Equal("B", MockeableFactory.New<A>().Value);
        }
    }
}