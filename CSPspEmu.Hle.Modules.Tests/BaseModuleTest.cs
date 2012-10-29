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
		protected virtual void SetMocks(PspEmulatorContext PspEmulatorContext)
		{
		}

		protected virtual void Initialize()
		{
		}

		[TestInitialize]
		public void SetUp()
		{
			var PspEmulatorContext = new PspEmulatorContext(new PspConfig());

			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			SetMocks(PspEmulatorContext);

			PspEmulatorContext.InjectDependencesTo(this);
			Initialize();
		}
	}
}
