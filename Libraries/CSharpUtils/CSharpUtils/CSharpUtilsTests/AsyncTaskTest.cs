using System.Threading;
using CSharpUtils;
using Xunit;


namespace CSharpUtilsTests
{
    public class AsyncTaskTest
    {
        [Fact]
        public void SimpleTest()
        {
            var Result = new AsyncTask<string>(delegate() { return "Hello"; });
            Assert.Equal("Hello", Result.Result);
        }

        [Fact(Skip = "fails on mono")]
        public void Complex1Test()
        {
            var Result = new AsyncTask<string>(delegate()
            {
                Thread.Yield();
                return "Test";
            });
            Assert.False(Result.Ready);
            Thread.Sleep(2);
            Assert.True(Result.Ready);
        }

        [Fact]
        public void Complex2Test()
        {
            var Result = new AsyncTask<string>(delegate()
            {
                Thread.Yield();
                return "Test";
            });
            Assert.Equal("Test", Result.Result);
            Assert.True(Result.Ready);
        }
    }
}