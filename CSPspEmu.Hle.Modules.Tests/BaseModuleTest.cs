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
		protected virtual void SetMocks(InjectContext InjectContext)
		{
		}

		protected virtual void Initialize()
		{
		}

		[TestInitialize]
		public void SetUp()
		{
			var InjectContext = new InjectContext();

			InjectContext.SetInstanceType<PspMemory, LazyPspMemory>();
			SetMocks(InjectContext);

			InjectContext.InjectDependencesTo(this);
			Initialize();
		}
	}
}
