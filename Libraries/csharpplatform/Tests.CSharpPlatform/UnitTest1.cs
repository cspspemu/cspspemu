using System;
using NUnit.Framework;
using CSharpPlatform;

namespace Tests.CSharpPlatform
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        unsafe public void TestMatrix()
        {
            Console.WriteLine(Matrix4f.Identity.Translate(2, 2, 0));
        }
    }
}