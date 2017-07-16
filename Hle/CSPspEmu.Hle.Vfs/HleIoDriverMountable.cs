using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Hle.Vfs
{
    public unsafe class HleIoDriverMountable : IHleIoDriver
    {
        Dictionary<String, IHleIoDriver> Mounts = new Dictionary<string, IHleIoDriver>();

        public IHleIoDriver GetMount(string MountAt)
        {
            return Mounts[MountAt];
        }

        public void Mount(string MountAt, IHleIoDriver HleIoDriver)
        {
            Mounts[MountAt] = HleIoDriver;
        }

        protected void ReLocatePathHandle(ref HleIoDrvFileArg HleIoDrvFileArg, ref string FileName)
        {
            foreach (var Mount in Mounts.OrderByDescending(Mount => Mount.Key.Length))
            {
                if (FileName.StartsWith(Mount.Key))
                {
                    HleIoDrvFileArg.HleIoDriver = Mount.Value;
                    FileName = FileName.Substring(Mount.Key.Length);
                    return;
                }
            }
            throw(new InvalidOperationException("Can't find mount point for '" + FileName + "'"));
        }

        public unsafe int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
        {
            ReLocatePathHandle(ref HleIoDrvFileArg, ref FileName);
            return HleIoDrvFileArg.HleIoDriver.IoOpen(HleIoDrvFileArg, FileName, Flags, Mode);
        }

        public unsafe int IoInit()
        {
            return 0;
        }

        public unsafe int IoExit()
        {
            return 0;
        }

        public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            return HleIoDrvFileArg.HleIoDriver.IoClose(HleIoDrvFileArg);
            //throw new NotImplementedException();
        }

        public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength,
            byte* OutputPointer, int OutputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
        {
            ReLocatePathHandle(ref HleIoDrvFileArg, ref Name);
            return HleIoDrvFileArg.HleIoDriver.IoMkdir(HleIoDrvFileArg, Name, Mode);
        }

        public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            ReLocatePathHandle(ref HleIoDrvFileArg, ref Name);
            return HleIoDrvFileArg.HleIoDriver.IoRmdir(HleIoDrvFileArg, Name);
        }

        public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            ReLocatePathHandle(ref HleIoDrvFileArg, ref Name);
            return HleIoDrvFileArg.HleIoDriver.IoDopen(HleIoDrvFileArg, Name);
        }

        public unsafe int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
        {
            ReLocatePathHandle(ref HleIoDrvFileArg, ref FileName);
            return HleIoDrvFileArg.HleIoDriver.IoGetstat(HleIoDrvFileArg, FileName, Stat);
        }

        public unsafe int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat, int Bits)
        {
            ReLocatePathHandle(ref HleIoDrvFileArg, ref FileName);
            return HleIoDrvFileArg.HleIoDriver.IoChstat(HleIoDrvFileArg, FileName, Stat, Bits);
        }

        public unsafe int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer,
            int InputLength, byte* OutputPointer, int OutputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }
    }
}