using System;
using System.Threading;
using CSharpUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class AsyncTaskTest
    {
        [TestMethod]
        public void SimpleTest()
        {
            var Result = new AsyncTask<String>(delegate() { return "Hello"; });
            Assert.AreEqual("Hello", Result.Result);
        }

        [TestMethod]
        public void Complex1Test()
        {
            var Result = new AsyncTask<String>(delegate()
            {
                Thread.Yield();
                return "Test";
            });
            Assert.IsFalse(Result.Ready);
            Thread.Sleep(2);
            Assert.IsTrue(Result.Ready);
        }

        [TestMethod]
        public void Complex2Test()
        {
            var Result = new AsyncTask<String>(delegate()
            {
                Thread.Yield();
                return "Test";
            });
            Assert.AreEqual("Test", Result.Result);
            Assert.IsTrue(Result.Ready);
        }
    }
}