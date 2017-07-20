using SafeILGenerator.Utils;
using Xunit;


namespace SafeILGenerator.Tests.Util
{
    
    public class ILFieldInfoTest
    {
        public int Test;

        [Fact]
        public void TestGetFieldInfo()
        {
            Assert.Equal(
                typeof(ILFieldInfoTest).GetField("Test"),
                IlFieldInfo.GetFieldInfo(() => Test)
            );
        }
    }
}