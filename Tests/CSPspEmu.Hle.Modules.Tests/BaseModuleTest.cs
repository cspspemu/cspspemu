using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.Tests
{
	[TestClass]
	public class BaseModuleTest
	{
		[TestInitialize]
		public void SetUp()
		{
			TestHleUtils.CreateInjectContext(this);
		}
	}
}
