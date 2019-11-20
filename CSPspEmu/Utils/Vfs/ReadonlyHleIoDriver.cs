using System;

namespace CSPspEmu.Hle.Vfs
{
    public unsafe class ReadonlyHleIoDriver : ProxyHleIoDriver
    {
        public ReadonlyHleIoDriver(IHleIoDriver HleIoDriver)
            : base(HleIoDriver)
        {
        }

        public override int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
        {
            //if (Mode)
            // ...
            return base.IoOpen(HleIoDrvFileArg, FileName, Flags, Mode);
        }

        public override int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
        {
            throw new NotImplementedException();
        }

        public override int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
        {
            Console.Error.WriteLine("Tried to create directory on a readonly filesystem '{0}'", Name);
            //throw(new NotImplementedException());
            return 0;
        }

        public override int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public override int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
        {
            throw new NotImplementedException();
        }

        public override int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }
    }
}