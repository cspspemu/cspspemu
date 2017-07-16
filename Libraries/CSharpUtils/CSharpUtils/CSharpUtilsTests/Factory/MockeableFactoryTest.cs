using System;
using CSharpUtils.Factory;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class MockeableFactoryTest
    {
        class A
        {
            public virtual String Value
            {
                get { return "A"; }
            }
        }

        class B : A
        {
            public override String Value
            {
                get { return "B"; }
            }
        }

        [Test]
        public void MockTypeTest()
        {
            var MockeableFactory = new MockeableFactory();
            Assert.AreEqual("A", MockeableFactory.New<A>().Value);
            MockeableFactory.MockType(typeof(A), typeof(B));
            Assert.AreEqual("B", MockeableFactory.New<A>().Value);
        }
    }
}