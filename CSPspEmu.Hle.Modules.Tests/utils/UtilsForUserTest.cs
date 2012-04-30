using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Hle.Modules.utils;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.Tests.utils
{
	[TestClass]
	public class UtilsForUserTest : BaseModuleTest
	{
		[Inject]
		UtilsForUser UtilsForUser = null;

		[TestMethod]
		public void TestMt19937()
		{
			SceKernelUtilsMt19937Context SceKernelUtilsMt19937Context;

			UtilsForUser.sceKernelUtilsMt19937Init(out SceKernelUtilsMt19937Context, 0x00000000);
			Assert.AreEqual<uint>(0x39747020, UtilsForUser.sceKernelUtilsMt19937UInt(ref SceKernelUtilsMt19937Context));

			UtilsForUser.sceKernelUtilsMt19937Init(out SceKernelUtilsMt19937Context, 0x76543210);

			Assert.AreEqual<uint>(0x1BDC5797, UtilsForUser.sceKernelUtilsMt19937UInt(ref SceKernelUtilsMt19937Context));

			for (int n = 0; n < 1234; n++)
			{
				UtilsForUser.sceKernelUtilsMt19937UInt(ref SceKernelUtilsMt19937Context);
			}

			Assert.AreEqual<uint>(0xE5051779, UtilsForUser.sceKernelUtilsMt19937UInt(ref SceKernelUtilsMt19937Context));
		}
	}
}
