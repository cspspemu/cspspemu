

using Xunit;

namespace CSharpUtilsTests
{
    
    public class TypeUtilsTest
    {
        public class MyType
        {
        }

        public class MyExtendedType1 : MyType
        {
        }

        public class MyExtendedType2 : MyType
        {
        }

        [Fact(Skip = "Inconclusive")]
        public void GetTypesExtendingTest()
        {
            /*
            Assert.Equals(
                "",
                TypeUtils.GetTypesExtending(typeof(MyType)).ToStringArray()
            );
            */
            //Assert.Inconclusive();
        }
    }
}