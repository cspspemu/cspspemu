using System;
using CSPspEmu.Gui.Winforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSPspEmu.Core.Tests.Gui
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var GameList = new GameList();
			GameList.HandleIso(@"e:\isos\psp\b-tonowa.iso");
		}
	}
}
