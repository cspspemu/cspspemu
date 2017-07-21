
using CSPspEmu.Hle.Modules.iofilemgr;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Iso;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Formats;
using Xunit;

namespace CSPspEmu.Hle.Modules.Tests.iofilemgr.umd
{
    
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

        [Fact(Skip = "file not found")]
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

            //Assert.Equal(0, result, "Expected no error");
            Assert.Equal(0, result);
            Assert.Equal(
                "CD001",
                PrimaryVolumeDescriptor.VolumeDescriptorHeader.IdString
            );
            Assert.Equal(
                VolumeDescriptorHeader.TypeEnum.PrimaryVolumeDescriptor,
                PrimaryVolumeDescriptor.VolumeDescriptorHeader.Type
            );
        }

        [Fact(Skip = "file not found")]
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

            Assert.Equal(IsoFile.SectorSize, SectorSize);
        }
    }
}