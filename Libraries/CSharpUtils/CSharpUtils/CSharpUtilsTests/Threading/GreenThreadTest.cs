using System.Collections.Generic;
using System.Threading;
using CSharpUtils.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Threading;

namespace CSharpUtilsTests
{
    [TestClass]
    public class GreenThreadTest
    {
        [TestMethod]
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
            Assert.AreEqual("[]", Values.ToJson());
            Thread1.SwitchTo();
            Assert.AreEqual("[1]", Values.ToJson());
            Thread2.SwitchTo();
            Assert.AreEqual("[1,2]", Values.ToJson());
            Thread.Sleep(20);
            Thread1.SwitchTo();
            Assert.AreEqual("[1,2,3]", Values.ToJson());
            Thread2.SwitchTo();
            Assert.AreEqual("[1,2,3,4]", Values.ToJson());
        }
    }
}