using SafeILGenerator.Utils;
using Xunit;


namespace SafeILGenerator.Tests.Util
{
    
    public class IlFieldInfoTest
    {
        public int Test;

        [Fact]
        public void TestGetFieldInfo()
        {
            Assert.Equal(
                typeof(IlFieldInfoTest).GetField("Test"),
                IlFieldInfo.GetFieldInfo(() => Test)
            );
        }
    }
}