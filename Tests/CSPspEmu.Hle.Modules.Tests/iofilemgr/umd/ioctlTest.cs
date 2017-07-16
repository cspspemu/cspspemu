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
    public unsafe class ioctlTest : BaseModuleTest, IInjectInitialize
    {
        [Inject] IoFileMgrForUser IoFileMgrForUser = null;

        [Inject] HleIoManager HleIoManager = null;

        SceUID BootBinFileHandle;

        void IInjectInitialize.Initialize()
        {
            var Iso = IsoLoader.GetIso("../../../pspautotests/input/iotest.iso");
            var Umd = new HleIoDriverIso(Iso);
            HleIoManager.SetDriver("disc:", Umd);
            HleIoManager.Chdir("disc0:/PSP_GAME/USRDIR");
            BootBinFileHandle =
                IoFileMgrForUser.sceIoOpen("disc0:/PSP_GAME/SYSDIR/BOOT.BIN", HleIoFlags.Read, SceMode.All);
        }

        [TestMethod]
        public void GetPrimaryVolumeDescriptorTest()
        {
            var PrimaryVolumeDescriptor = default(PrimaryVolumeDescriptor);
            var result = IoFileMgrForUser.sceIoIoctl(
                FileHandle: BootBinFileHandle,
                Command: (uint) HleIoDriverIso.UmdCommandEnum.GetPrimaryVolumeDescriptor,
                InputPointer: null,
                InputLength: 0,
                OutputPointer: (byte*) &PrimaryVolumeDescriptor,
                OutputLength: sizeof(PrimaryVolumeDescriptor)
            );

            Assert.AreEqual(0, result, "Expected no error");
            Assert.AreEqual(
                "CD001",
                PrimaryVolumeDescriptor.VolumeDescriptorHeader.IdString
            );
            Assert.AreEqual(
                VolumeDescriptorHeader.TypeEnum.PrimaryVolumeDescriptor,
                PrimaryVolumeDescriptor.VolumeDescriptorHeader.Type
            );
        }

        [TestMethod]
        public void GetSectorSizeTest()
        {
            uint SectorSize;

            var result = IoFileMgrForUser.sceIoIoctl(
                FileHandle: BootBinFileHandle,
                Command: (uint) HleIoDriverIso.UmdCommandEnum.GetSectorSize,
                InputPointer: null,
                InputLength: 0,
                OutputPointer: (byte*) &SectorSize,
                OutputLength: sizeof(uint)
            );

            Assert.AreEqual(IsoFile.SectorSize, SectorSize);
        }
    }
}