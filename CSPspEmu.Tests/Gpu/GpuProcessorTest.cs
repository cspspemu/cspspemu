using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Gpu;

namespace CSPspEmu.Core.Tests.Gpu
{
	[TestClass]
	public class GpuProcessorTest
	{
		//PspMemory Memory;
		//GpuProcessor Gpu;

		[TestInitialize]
		public void SetUp()
		{
			//Memory = new LazyPspMemory();
			//Gpu = new GpuProcessor(Memory);
		}

		[TestMethod]
		public void TestGpuProcessor()
		{
			//GpuDisplayList GpuDisplayList = Gpu.CreateDisplayList();
		}
	}
}
