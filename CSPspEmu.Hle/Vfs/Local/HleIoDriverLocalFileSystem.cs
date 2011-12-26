using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle.Vfs.Local
{
	public class HleIoDriverLocalFileSystem : IHleIoDriver
	{
		protected string BasePath;

		public HleIoDriverLocalFileSystem(string BasePath)
		{
			this.BasePath = BasePath;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		static public string GetSanitizedPath(string Path)
		{
			var Parts = new Stack<string>();
			foreach (var Part in Path.Split('/', '\\'))
			{
				switch (Part)
				{
					case "": if (Parts.Count == 0) Parts.Push(""); break;
					case ".": break;
					case "..": if (Parts.Count > 0) Parts.Pop(); break;
					default: Parts.Push(Part); break;
				}
			}

			return String.Join("/", Parts.Reverse());
		}

		protected string GetFullNormalizedAndSanitizedPath(string Path)
		{
			var Normalized = BasePath + "/" + GetSanitizedPath(Path);
			Normalized = Normalized.Replace('/', '\\').Replace("\\\\", "\\");
			return Normalized;
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
			var RealFileName = GetFullNormalizedAndSanitizedPath(FileName);
			FileMode FileMode = FileMode.Open;
			FileAccess FileAccess = 0;
			bool Append = (Flags & HleIoFlags.Append) != 0;
			bool Read = (Flags & HleIoFlags.Read) != 0;
			bool Write = (Flags & HleIoFlags.Write) != 0;
			bool Truncate = (Flags & HleIoFlags.Truncate) != 0;
			bool Create = (Flags & HleIoFlags.Create) != 0;

			if (Append)
			{
				FileMode = FileMode.Append;
			}
			else if (Create)
			{
				FileMode = FileMode.Create;
			}
			else if (Truncate)
			{
				FileMode = FileMode.Truncate;
			}

			if (Read) FileAccess |= FileAccess.Read;
			if (Write) FileAccess |= FileAccess.Write;
			//if (Append) FileMode |= FileMode.Open;

			HleIoDrvFileArg.FileArgument = File.Open(RealFileName, FileMode, FileAccess, FileShare.Read);
			return 0;
		}

		public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
			FileStream.Close();
			return 0;
		}

		public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
		{
			try
			{
				var Buffer = new byte[OutputLength];
				var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
				Console.WriteLine("ReadPosition: {0}", FileStream.Position);
				int Readed = FileStream.Read(Buffer, 0, OutputLength);
				for (int n = 0; n < Readed; n++) *OutputPointer++ = Buffer[n];
				return Readed;
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
				return -1;
			}
		}

		public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
		{
			try
			{
				var Buffer = new byte[InputLength];
				for (int n = 0; n < InputLength; n++) Buffer[n]  = * InputPointer++;

				var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
				FileStream.Write(Buffer, 0, InputLength);
				FileStream.Flush();
				return InputLength;
			}
			catch (Exception)
			{
				//Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
		{
			var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
			switch (Whence)
			{
				case SeekAnchor.Set:
					FileStream.Position = Offset;
					break;
				case SeekAnchor.Cursor:
					FileStream.Position = FileStream.Position + Offset;
					break;
				case SeekAnchor.End:
					FileStream.Position = FileStream.Length + Offset;
					break;
			}
			return FileStream.Position;
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
			var RealFileName = GetFullNormalizedAndSanitizedPath(Name);
			Directory.CreateDirectory(RealFileName);
			//HleIoDrvFileArg.
			//throw new NotImplementedException();
			return 0;
		}

		public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			var RealFileName = GetFullNormalizedAndSanitizedPath(Name);
			try
			{
				//Console.Error.WriteLine("'{0}'", RealFileName);
				var Items = new List<HleIoDirent>();

				Items.Add(CreateFakeDirectoryHleIoDirent("."));
				Items.Add(CreateFakeDirectoryHleIoDirent(".."));
				Items.AddRange(new DirectoryInfo(RealFileName).EnumerateFiles().Select(Item => ConvertFileSystemInfoToHleIoDirent(Item)));
				Items.AddRange(new DirectoryInfo(RealFileName).EnumerateDirectories().Select(Item => ConvertFileSystemInfoToHleIoDirent(Item)));

				HleIoDrvFileArg.FileArgument = new DirectoryEnumerator<HleIoDirent>(Items.ToArray());
				return 0;
			}
			catch (DirectoryNotFoundException DirectoryNotFoundException)
			{
				Console.WriteLine(DirectoryNotFoundException);
				return -1;
			}
		}

		public unsafe int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			//throw new NotImplementedException();
			return 0;
		}

		unsafe static public HleIoDirent CreateFakeDirectoryHleIoDirent(string Name)
		{
			var HleIoDirent = default(HleIoDirent);
			PointerUtils.StoreStringOnPtr(Name, Encoding.UTF8, HleIoDirent.Name);
			HleIoDirent.Stat.Size = 0;
			HleIoDirent.Stat.Mode = SceMode.Directory | (SceMode)Convert.ToInt32("777", 8);
			HleIoDirent.Stat.Attributes = IOFileModes.Directory;
			HleIoDirent.Stat.DeviceDependentData0 = 10;
			return HleIoDirent;
		}

		unsafe static public HleIoDirent ConvertFileSystemInfoToHleIoDirent(FileSystemInfo FileSystemInfo)
		{
			var HleIoDirent = default(HleIoDirent);
			var FileInfo = (FileSystemInfo as FileInfo);
			var DirectoryInfo = (FileSystemInfo as DirectoryInfo);
			{
				if (DirectoryInfo != null)
				{
					HleIoDirent.Stat.Size = 0;
					HleIoDirent.Stat.Mode = SceMode.Directory | (SceMode)Convert.ToInt32("777", 8);
					HleIoDirent.Stat.Attributes = IOFileModes.Directory;
					PointerUtils.StoreStringOnPtr(FileSystemInfo.Name, Encoding.UTF8, HleIoDirent.Name);
				}
				else
				{
					HleIoDirent.Stat.Size = FileInfo.Length;
					HleIoDirent.Stat.Mode = SceMode.File | (SceMode)Convert.ToInt32("777", 8);
					//HleIoDirent.Stat.Attributes = IOFileModes.File | IOFileModes.CanRead | IOFileModes.CanWrite | IOFileModes.CanExecute;
					HleIoDirent.Stat.Attributes = IOFileModes.File;
					PointerUtils.StoreStringOnPtr(FileSystemInfo.Name.ToUpper(), Encoding.UTF8, HleIoDirent.Name);
				}

				HleIoDirent.Stat.DeviceDependentData0 = 10;
			}
			return HleIoDirent;
		}

		public unsafe int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* IoDirent)
		{
			var Enumerator = (DirectoryEnumerator<HleIoDirent>)HleIoDrvFileArg.FileArgument;

			// More items.
			if (Enumerator.MoveNext())
			{
				//Console.Error.WriteLine("'{0}'", Enumerator.Current.ToString());
				*IoDirent = Enumerator.Current;
				/*
				
				*/
			}
			// No more items.
			else
			{
			}

			return Enumerator.GetLeft();
		}

		public unsafe int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
		{
			var RealFileName = GetFullNormalizedAndSanitizedPath(FileName);
			//Console.WriteLine(RealFileName);
			var FileInfo = new FileInfo(RealFileName);
			Stat->Size = FileInfo.Length;
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
