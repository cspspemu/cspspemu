using System;

namespace CSPspEmu.Hle.Vfs
{
    public unsafe class ProxyHleIoDriver : AbstractHleIoDriver, IHleIoDriver
    {
        public IHleIoDriver ParentDriver;
        public ProxyHleIoDriver(IHleIoDriver ParentDriver) => this.ParentDriver = ParentDriver;
        public virtual int IoInit() => ParentDriver.IoInit();
        public virtual int IoExit() => ParentDriver.IoExit();
        public virtual int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode) => ParentDriver.IoOpen(HleIoDrvFileArg, FileName, Flags, Mode);
        public virtual int IoClose(HleIoDrvFileArg HleIoDrvFileArg) => ParentDriver.IoClose(HleIoDrvFileArg);
        public virtual int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength) => ParentDriver.IoRead(HleIoDrvFileArg, OutputPointer, OutputLength);
        public virtual int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength) => ParentDriver.IoWrite(HleIoDrvFileArg, InputPointer, InputLength);
        public virtual long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence) => ParentDriver.IoLseek(HleIoDrvFileArg, Offset, Whence);
        public virtual int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, Span<byte> Input, Span<byte> Output) => ParentDriver.IoIoctl(HleIoDrvFileArg, Command, Input, Output);
        public virtual int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name) => ParentDriver.IoRemove(HleIoDrvFileArg, Name);
        public virtual int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode) => ParentDriver.IoMkdir(HleIoDrvFileArg, Name, Mode);
        public virtual int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name) => ParentDriver.IoRmdir(HleIoDrvFileArg, Name);
        public virtual int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name) => ParentDriver.IoDopen(HleIoDrvFileArg, Name);
        public virtual int IoDclose(HleIoDrvFileArg HleIoDrvFileArg) => ParentDriver.IoDclose(HleIoDrvFileArg);
        public virtual int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir) => ParentDriver.IoDread(HleIoDrvFileArg, dir);
        public virtual int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat) => ParentDriver.IoGetstat(HleIoDrvFileArg, FileName, Stat);
        public virtual int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits) => ParentDriver.IoChstat(HleIoDrvFileArg, FileName, stat, bits);
        public virtual int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName) => ParentDriver.IoRename(HleIoDrvFileArg, OldFileName, NewFileName);
        public virtual int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName) => ParentDriver.IoChdir(HleIoDrvFileArg, DirectoryName);
        public virtual int IoMount(HleIoDrvFileArg HleIoDrvFileArg) => ParentDriver.IoMount(HleIoDrvFileArg);
        public virtual int IoUmount(HleIoDrvFileArg HleIoDrvFileArg) => ParentDriver.IoUmount(HleIoDrvFileArg);
        public virtual int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, Span<byte> Input,
            Span<byte> Output, ref bool DoDleay) => ParentDriver.IoDevctl(HleIoDrvFileArg, DeviceName, Command, Input, Output, ref DoDleay);
        public virtual int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg) => ParentDriver.IoUnk21(HleIoDrvFileArg);
    }
}