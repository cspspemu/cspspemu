namespace CSPspEmu.Hle.Vfs
{
    public unsafe class ProxyHleIoDriver : IHleIoDriver
    {
        public IHleIoDriver ParentDriver;

        public ProxyHleIoDriver(IHleIoDriver ParentDriver)
        {
            this.ParentDriver = ParentDriver;
        }

        public virtual int IoInit()
        {
            return ParentDriver.IoInit();
        }

        public virtual int IoExit()
        {
            return ParentDriver.IoExit();
        }

        public virtual int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
        {
            return ParentDriver.IoOpen(HleIoDrvFileArg, FileName, Flags, Mode);
        }

        public virtual int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            return ParentDriver.IoClose(HleIoDrvFileArg);
        }

        public virtual int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
        {
            return ParentDriver.IoRead(HleIoDrvFileArg, OutputPointer, OutputLength);
        }

        public virtual int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
        {
            return ParentDriver.IoWrite(HleIoDrvFileArg, InputPointer, InputLength);
        }

        public virtual long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
        {
            return ParentDriver.IoLseek(HleIoDrvFileArg, Offset, Whence);
        }

        public virtual int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength,
            byte* OutputPointer, int OutputLength)
        {
            return ParentDriver.IoIoctl(HleIoDrvFileArg, Command, InputPointer, InputLength, OutputPointer,
                OutputLength);
        }

        public virtual int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            return ParentDriver.IoRemove(HleIoDrvFileArg, Name);
        }

        public virtual int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
        {
            return ParentDriver.IoMkdir(HleIoDrvFileArg, Name, Mode);
        }

        public virtual int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            return ParentDriver.IoRmdir(HleIoDrvFileArg, Name);
        }

        public virtual int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            return ParentDriver.IoDopen(HleIoDrvFileArg, Name);
        }

        public virtual int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            return ParentDriver.IoDclose(HleIoDrvFileArg);
        }

        public virtual int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir)
        {
            return ParentDriver.IoDread(HleIoDrvFileArg, dir);
        }

        public virtual int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
        {
            return ParentDriver.IoGetstat(HleIoDrvFileArg, FileName, Stat);
        }

        public virtual int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
        {
            return ParentDriver.IoChstat(HleIoDrvFileArg, FileName, stat, bits);
        }

        public virtual int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
        {
            return ParentDriver.IoRename(HleIoDrvFileArg, OldFileName, NewFileName);
        }

        public virtual int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
        {
            return ParentDriver.IoChdir(HleIoDrvFileArg, DirectoryName);
        }

        public virtual int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            return ParentDriver.IoMount(HleIoDrvFileArg);
        }

        public virtual int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            return ParentDriver.IoUmount(HleIoDrvFileArg);
        }

        public virtual int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command,
            byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
        {
            return ParentDriver.IoDevctl(HleIoDrvFileArg, DeviceName, Command, InputPointer, InputLength, OutputPointer,
                OutputLength);
        }

        public virtual int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
        {
            return ParentDriver.IoUnk21(HleIoDrvFileArg);
        }
    }
}