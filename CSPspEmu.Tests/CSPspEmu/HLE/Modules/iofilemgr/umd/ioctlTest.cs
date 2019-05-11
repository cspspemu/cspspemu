
using CSPspEmu.Hle.Modules.iofilemgr;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Iso;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Formats;
using Xunit;

namespace CSPspEmu.Hle.Modules.Tests.iofilemgr.umd
{
    
    public unsafe class IoctlTest : BaseModuleTest, IInjectInitialize
    {
        [Inject] protected IoFileMgrForUser IoFileMgrForUser = null;

        [Inject] protected HleIoManager HleIoManager = null;

        protected SceUID BootBinFileHandle;

        void IInjectInitialize.Initialize()
        {
            var iso = IsoLoader.GetIso("../../../pspautotests/input/iotest.iso");
            var umd = new HleIoDriverIso(iso);
            HleIoManager.SetDriver("disc:", umd);
            HleIoManager.Chdir("disc0:/PSP_GAME/USRDIR");
            BootBinFileHandle =
                IoFileMgrForUser.sceIoOpen("disc0:/PSP_GAME/SYSDIR/BOOT.BIN", HleIoFlags.Read, SceMode.All);
        }

        [Fact(Skip = "file not found")]
        public void GetPrimaryVolumeDescriptorTest()
        {
            var primaryVolumeDescriptor = default(PrimaryVolumeDescriptor);
            var result = IoFileMgrForUser.sceIoIoctl(
                FileHandle: BootBinFileHandle,
                Command: (uint) HleIoDriverIso.UmdCommandEnum.GetPrimaryVolumeDescriptor,
                InputPointer: null,
                InputLength: 0,
                OutputPointer: (byte*) &primaryVolumeDescriptor,
                OutputLength: sizeof(PrimaryVolumeDescriptor)
            );

            //Assert.Equal(0, result, "Expected no error");
            Assert.Equal(0, result);
            Assert.Equal(
                "CD001",
                primaryVolumeDescriptor.VolumeDescriptorHeader.IdString
            );
            Assert.Equal(
                VolumeDescriptorHeader.TypeEnum.PrimaryVolumeDescriptor,
                primaryVolumeDescriptor.VolumeDescriptorHeader.Type
            );
        }

        [Fact(Skip = "file not found")]
        //[Fact]
        public void GetSectorSizeTest()
        {
            uint sectorSize;

            var result = IoFileMgrForUser.sceIoIoctl(
                FileHandle: BootBinFileHandle,
                Command: (uint) HleIoDriverIso.UmdCommandEnum.GetSectorSize,
                InputPointer: null,
                InputLength: 0,
                OutputPointer: (byte*) &sectorSize,
                OutputLength: sizeof(uint)
            );

            Assert.Equal(IsoFile.SectorSize, sectorSize);
        }
    }
}