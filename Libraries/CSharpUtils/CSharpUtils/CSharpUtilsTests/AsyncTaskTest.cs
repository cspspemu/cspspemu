using System;
using System.Threading;
using CSharpUtils;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class AsyncTaskTest
    {
        [Test]
        public void SimpleTest()
        {
            var Result = new AsyncTask<string>(delegate() { return "Hello"; });
            Assert.AreEqual("Hello", Result.Result);
        }

        [Test]
        public void Complex1Test()
        {
            var Result = new AsyncTask<string>(delegate()
            {
                Thread.Yield();
                return "Test";
            });
            Assert.IsFalse(Result.Ready);
            Thread.Sleep(2);
            Assert.IsTrue(Result.Ready);
        }

        [Test]
        public void Complex2Test()
        {
            var Result = new AsyncTask<string>(delegate()
            {
                Thread.Yield();
                return "Test";
            });
            Assert.AreEqual("Test", Result.Result);
            Assert.IsTrue(Result.Ready);
        }
    }
}