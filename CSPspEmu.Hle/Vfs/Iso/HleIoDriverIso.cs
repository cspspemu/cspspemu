using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Utils;
using CSPspEmu.Hle.Formats;

namespace CSPspEmu.Hle.Vfs.Iso
{
	public class HleIoDriverIso : IHleIoDriver
	{
		public IsoFile Iso { get; protected set; }

		public HleIoDriverIso(IsoFile Iso)
		{
			this.Iso = Iso;
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
			// disc0:/sce_lbn0x5fa0_size0x1428
			//Console.WriteLine(":::::::::" + FileName);
			if (FileName.StartsWith("/sce_"))
			{
				int Sector = 0, Size = 0;
				var Parts = FileName.Substring(5).Split('_');
				foreach (var Part in Parts)
				{
					if (Part.StartsWith("lbn"))
					{
						var Number = Part.Substring(3);
						Sector = NumberUtils.ParseIntegerConstant(Number);
					}
					else if (Part.StartsWith("size"))
					{
						var Number = Part.Substring(4);
						Size = NumberUtils.ParseIntegerConstant(Number);
					}
					else
					{
						throw(new NotImplementedException("Can't handle special filename '" + FileName + "' part '" + Part + "'"));
					}
					//Console.WriteLine(Part);
				}
				//Console.WriteLine("SPECIAL({0}, {1})", lbn, size);
				//Console.WriteLine("SPECIAL!!!!!!!!!!!!!!!!!!!!");
				HleIoDrvFileArg.FileArgument = Iso.Stream.SliceWithLength(Sector * IsoFile.SectorSize, Size);
				return 0;
			}

			//Console.WriteLine(FileName);
			var IsoNode = Iso.Root.Locate(FileName);
			HleIoDrvFileArg.FileArgument = IsoNode.Open();
			return 0;
		}

		public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			var Stream = ((Stream)HleIoDrvFileArg.FileArgument);
			Stream.Close();
			return 0;
			//throw new NotImplementedException();
		}

		public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
		{
			var Stream = ((Stream)HleIoDrvFileArg.FileArgument);
			var OutputData = new byte[OutputLength];
			int Readed = Stream.Read(OutputData, 0, OutputLength);
			Marshal.Copy(OutputData, 0, new IntPtr(OutputPointer), OutputLength);
			return Readed;
			//throw new NotImplementedException();
		}

		public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
		{
			var Stream = ((Stream)HleIoDrvFileArg.FileArgument);
			//Stream.Seek(
			return Stream.Seek(Offset, (SeekOrigin)Whence);
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
			//Console.WriteLine(FileName);
			var IsoNode = Iso.Root.Locate(FileName);

			//IsoNode.DirectoryRecord.Date
			Stat[0].Mode = 0;
			Stat[0].Mode |= SceMode.FIO_S_IRUSR | SceMode.FIO_S_IWUSR | SceMode.FIO_S_IXUSR;
			Stat[0].Mode |= SceMode.FIO_S_IRGRP | SceMode.FIO_S_IWGRP | SceMode.FIO_S_IXGRP;
			Stat[0].Mode |= SceMode.FIO_S_IROTH | SceMode.FIO_S_IWOTH | SceMode.FIO_S_IXOTH;

			if (IsoNode.IsDirectory)
			{
				Stat[0].Mode = SceMode.FIO_S_IFDIR;
				Stat[0].Attributes = IOFileModes.FIO_SO_IFDIR;
			}
			else
			{
				Stat[0].Mode = SceMode.FIO_S_IFREG;
				Stat[0].Attributes = IOFileModes.FIO_SO_IFREG | IOFileModes.FIO_SO_IROTH | IOFileModes.FIO_SO_IWOTH | IOFileModes.FIO_SO_IXOTH;
			}
			Stat[0].Size = IsoNode.DirectoryRecord.Size;
			Stat[0].TimeCreation = ScePspDateTime.FromDateTime(IsoNode.DirectoryRecord.Date);
			Stat[0].TimeLastAccess = ScePspDateTime.FromDateTime(IsoNode.DirectoryRecord.Date);
			Stat[0].TimeLastModification = ScePspDateTime.FromDateTime(IsoNode.DirectoryRecord.Date);
			Stat[0].DeviceDependentData0 = IsoNode.DirectoryRecord.Extent;

			//Stat[0].DeviceDependentData
			//throw new NotImplementedException();

			return 0;
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
