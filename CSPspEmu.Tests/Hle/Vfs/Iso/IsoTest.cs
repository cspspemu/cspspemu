using CSPspEmu.Hle.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSPspEmu.Hle.Vfs.Iso;

namespace CSPspEmu.Core.Tests
{
	[TestClass()]
	public class IsoTest
	{
		[TestMethod()]
		public void IsoConstructorTest()
		{
			var CsoName = "../../../TestInput/cube.cso";
			var Cso = new Cso(File.OpenRead(CsoName));
			var Iso = new IsoFile(new CsoProxyStream(Cso), CsoName);
			foreach (var Node in Iso.Root.Descendency())
			{
				Console.WriteLine(Node);
			}
		}
	}
}
