using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core;
using CSPspEmu.Hle.Modules.iofilemgr;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Iso;
using CSPspEmu.Hle.Modules.rtc;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Formats;

namespace CSPspEmu.Hle.Modules.Tests.iofilemgr.umd
{
	[TestClass]
	unsafe public class ioctlTest : BaseModuleTest
	{
		[Inject]
		IoFileMgrForUser IoFileMgrForUser = null;

		[Inject]
		HleIoManager HleIoManager = null;

		protected override void Initialize()
		{
			var Iso = IsoLoader.GetIso("../../../pspautotests/input/cube.cso");
			var Umd = new HleIoDriverIso(Iso);
			HleIoManager.SetDriver("disc:", Umd);
			HleIoManager.SetDriver("umd:", Umd);
			HleIoManager.SetDriver("host:", Umd);
			HleIoManager.SetDriver(":", Umd);
			HleIoManager.Chdir("disc0:/PSP_GAME/USRDIR");
		}

		[TestMethod]
		public void GetPrimaryVolumeDescriptorTest()
		{
			var buffer = new byte[0x800];
			fixed (byte* bufferPointer = buffer)
			{
				var result = IoFileMgrForUser.sceIoIoctl(
					FileHandle: IoFileMgrForUser.sceIoOpen("disc0:/PSP_GAME/SYSDIR/BOOT.BIN", HleIoFlags.Read, SceMode.All),
					Command: (uint)HleIoDriverIso.UmdCommandEnum.GetPrimaryVolumeDescriptor,
					InputPointer: null,
					InputLength: 0,
					OutputPointer: bufferPointer,
					OutputLength: buffer.Length
				);

				Assert.AreEqual(0, result, "Expected no error");
				Assert.AreEqual(
					((char)0x01) + "CD001",
					Encoding.ASCII.GetString(buffer.Slice(0, 6).ToArray()),
					"Expected the PrimaryVolumeDescriptor magic"
				);
			}
		}
	}
}
