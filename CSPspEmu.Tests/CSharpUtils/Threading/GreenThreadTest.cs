using System.Collections.Generic;
using System.Threading;
using CSharpUtils.Extensions;

using CSharpUtils.Threading;
using Xunit;

namespace CSharpUtilsTests
{
    
    public class GreenThreadTest
    {
        [Fact]
        public void Test1Test()
        {
            var Values = new List<int>();

            var Thread1 = new GreenThread();
            var Thread2 = new GreenThread();
            Thread1.InitAndStartStopped(() =>
            {
                Values.Add(1);
                GreenThread.Yield();
                Values.Add(3);
            });
            Thread2.InitAndStartStopped(() =>
            {
                Values.Add(2);
                GreenThread.Yield();
                Values.Add(4);
            });

            Thread.Sleep(20);
            // Inits stopped.
            Assert.Equal("[]", Values.ToJson());
            Thread1.SwitchTo();
            Assert.Equal("[1]", Values.ToJson());
            Thread2.SwitchTo();
            Assert.Equal("[1,2]", Values.ToJson());
            Thread.Sleep(20);
            Thread1.SwitchTo();
            Assert.Equal("[1,2,3]", Values.ToJson());
            Thread2.SwitchTo();
            Assert.Equal("[1,2,3,4]", Values.ToJson());
        }
    }
}