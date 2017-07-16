using System;
using SafeILGenerator.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeILGenerator.Tests.Util
{
    [TestClass]
    public class ILFieldInfoTest
    {
        public int Test;

        [TestMethod]
        public void TestGetFieldInfo()
        {
            Assert.AreEqual(
                typeof(ILFieldInfoTest).GetField("Test"),
                IlFieldInfo.GetFieldInfo(() => Test)
            );
        }
    }
}