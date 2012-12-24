using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.Tests
{
	[TestFixture]
	public class BaseModuleTest
	{
		[SetUp]
		public void SetUp()
		{
			TestHleUtils.CreateInjectContext(this);
		}
	}
}
