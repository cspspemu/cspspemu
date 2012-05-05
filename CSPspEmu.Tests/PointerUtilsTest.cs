using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils;

namespace CSPspEmu.Tests
{
	[TestClass]
	unsafe public class PointerUtilsTest
	{
		[TestMethod]
		public void TestMemset()
		{
			var Data = new byte[131];
			PointerUtils.Memset(Data, 0x3E, Data.Length);

			CollectionAssert.AreEqual(
				((byte)0x3E).Repeat(Data.Length),
				Data
			);
		}

		[TestMethod]
		public void TestMemcpy()
		{
			int SizeStart = 17;
			int SizeMiddle = 77;
			int SizeEnd = 17;
			var Dst = new byte[SizeStart + SizeMiddle + SizeEnd];
			fixed (byte* DstPtr = &Dst[SizeStart])
			{
				PointerUtils.Memcpy(DstPtr, ((byte)0x1D).Repeat(SizeMiddle).ToArray(), SizeMiddle);
			}

			var Expected = ((byte)0x00).Repeat(SizeStart).Concat(((byte)0x1D).Repeat(SizeMiddle)).Concat(((byte)0x00).Repeat(SizeEnd)).ToArray();

			//Console.WriteLine(BitConverter.ToString(Dst));
			//Console.WriteLine(BitConverter.ToString(Expected));

			CollectionAssert.AreEqual(
				Expected,
				Dst
			);
		}
	}
}
