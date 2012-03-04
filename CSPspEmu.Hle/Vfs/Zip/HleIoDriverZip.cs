using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Formats.Archive;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Vfs.Zip
{
	public class HleIoDriverZip : IHleIoDriver
	{
		protected ZipArchive ZipArchive;

		public HleIoDriverZip(ZipArchive ZipArchive)
		{
			this.ZipArchive = ZipArchive;
		}

		public unsafe int IoInit()
		{
			return 0;
		}

		public unsafe int IoExit()
		{
			return 0;
		}

		public unsafe int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
		{
			try
			{
				HleIoDrvFileArg.FileArgument = ZipArchive[FileName].OpenUncompressedStream();
				return 0;
			}
			catch (KeyNotFoundException)
			{
				throw(new FileNotFoundException("Can't find file '" + FileName + "' on ZipFile"));
			}
		}

		public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			var FileArgument = (Stream)HleIoDrvFileArg.FileArgument;
			FileArgument.Close();
			return 0;
		}

		public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
		{
			var FileArgument = (Stream)HleIoDrvFileArg.FileArgument;
			return FileArgument.ReadToPointer(OutputPointer, OutputLength);
		}

		public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
		{
			var FileArgument = (Stream)HleIoDrvFileArg.FileArgument;
			return FileArgument.Seek(Offset, (SeekOrigin)Whence);
		}

		public unsafe int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
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
			try
			{
				var ZipEntry = ZipArchive[FileName];
				Stat->Attributes = IOFileModes.CanRead | IOFileModes.File;
				Stat->Mode = SceMode.All;
				Stat->Size = ZipEntry.OpenUncompressedStream().Length;
				return 0;
			}
			catch (KeyNotFoundException)
			{
				throw (new FileNotFoundException("Can't find file '" + FileName + "' on ZipFile"));
			}
		}

		public unsafe int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
		{
			throw new NotImplementedException();
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

		public unsafe int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
		{
			throw new NotImplementedException();
		}
	}
}
