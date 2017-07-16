using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpPlatform;

namespace Tests.CSharpPlatform
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		unsafe public void TestMatrix()
		{
			Console.WriteLine(Matrix4f.Identity.Translate(2, 2, 0));
		}
	}
}
