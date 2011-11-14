using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs
{
	unsafe public class ProxyHleIoDriver : IHleIoDriver
	{
		public IHleIoDriver ParentDriver;

		public ProxyHleIoDriver(IHleIoDriver ParentDriver)
		{
			this.ParentDriver = ParentDriver;
		}

		virtual public int IoInit()
		{
			return ParentDriver.IoInit();
		}

		virtual public int IoExit()
		{
			return ParentDriver.IoExit();
		}

		virtual public int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
		{
			return ParentDriver.IoOpen(HleIoDrvFileArg, FileName, Flags, Mode);
		}

		virtual public int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			return ParentDriver.IoClose(HleIoDrvFileArg);
		}

		virtual public int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
		{
			return ParentDriver.IoRead(HleIoDrvFileArg, OutputPointer, OutputLength);
		}

		virtual public int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
		{
			return ParentDriver.IoWrite(HleIoDrvFileArg, InputPointer, InputLength);
		}

		virtual public int IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, int Whence)
		{
			return ParentDriver.IoLseek(HleIoDrvFileArg, Offset, Whence);
		}

		virtual public int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			return ParentDriver.IoIoctl(HleIoDrvFileArg, Command, InputPointer, InputLength, OutputPointer, OutputLength);
		}

		virtual public int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			return ParentDriver.IoRemove(HleIoDrvFileArg, Name);
		}

		virtual public int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
		{
			return ParentDriver.IoMkdir(HleIoDrvFileArg, Name, Mode);
		}

		virtual public int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			return ParentDriver.IoRmdir(HleIoDrvFileArg, Name);
		}

		virtual public int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			return ParentDriver.IoDopen(HleIoDrvFileArg, Name);
		}

		virtual public int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			return ParentDriver.IoDclose(HleIoDrvFileArg);
		}

		virtual public int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir)
		{
			return ParentDriver.IoDread(HleIoDrvFileArg, dir);
		}

		virtual public int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
		{
			return ParentDriver.IoGetstat(HleIoDrvFileArg, FileName, Stat);
		}

		virtual public int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
		{
			return ParentDriver.IoChstat(HleIoDrvFileArg, FileName, stat, bits);
		}

		virtual public int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
		{
			return ParentDriver.IoRename(HleIoDrvFileArg, OldFileName, NewFileName);
		}

		virtual public int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
		{
			return ParentDriver.IoChdir(HleIoDrvFileArg, DirectoryName);
		}

		virtual public int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
		{
			return ParentDriver.IoMount(HleIoDrvFileArg);
		}

		virtual public int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
		{
			return ParentDriver.IoUmount(HleIoDrvFileArg);
		}

		virtual public int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			return ParentDriver.IoDevctl(HleIoDrvFileArg, DeviceName, Command, InputPointer, InputLength, OutputPointer, OutputLength);
		}

		virtual public int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
		{
			return ParentDriver.IoUnk21(HleIoDrvFileArg);
		}
	}
}
