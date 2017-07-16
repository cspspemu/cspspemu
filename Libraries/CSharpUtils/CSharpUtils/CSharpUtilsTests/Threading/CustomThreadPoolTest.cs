using CSharpUtils.Threading;
using System.Threading;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
	[TestClass]
	public class CustomThreadPoolTest
	{
		[TestMethod]
		public void TestStop()
		{
			var CustomThreadPool = new CustomThreadPool(2);
			CustomThreadPool.Stop();
		}

		[TestMethod]
		public void Test()
		{
			var CustomThreadPool = new CustomThreadPool(2);
			var Results0 = new List<int>();
			var Results1 = new List<int>();
			var CountdownEvent = new CountdownEvent(2);

			CustomThreadPool.AddTask(0, () =>
			{
				Thread.Sleep(10);
				Results0.Add(0);
			});
			CustomThreadPool.AddTask(0, () =>
			{
				Results0.Add(1);
				CountdownEvent.Signal();
			});
			CustomThreadPool.AddTask(1, () =>
			{
				Results1.Add(0);
				CountdownEvent.Signal();
			});

			CountdownEvent.Wait();
			Thread.Sleep(10);
			Assert.IsTrue(CustomThreadPool.GetLoopIterCount(0) <= 2);
			Assert.IsTrue(CustomThreadPool.GetLoopIterCount(1) <= 2);
			Assert.AreEqual("0,1", Results0.ToStringArray());
			Assert.AreEqual("0", Results1.ToStringArray());
			CustomThreadPool.Stop();
		}
	}
}
