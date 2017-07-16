using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public static class FileSystemUtils
	{
		public static bool ContainsFileName(this IEnumerable<FileSystemEntry> FileSystemEntry, String FileName)
		{
			foreach (var Item in FileSystemEntry)
			{
				if (Item.Name == FileName) return true;
			}
			return false;
		}

		public static bool ExistsFile(this FileSystem FileSystem, String Path)
		{
			try
			{
				FileSystem.GetFileInfo(Path);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void CopyFile(this FileSystem FileSystem, String SourcePath, String DestinationPath)
		{
			using (var SourceStream = FileSystem.OpenFile(SourcePath, FileMode.Open))
			{
				using (var DestinationStream = FileSystem.OpenFile(DestinationPath, FileMode.Create))
				{
					SourceStream.CopyToFast(DestinationStream);
				}
			}
		}

		public static void WriteFile(this FileSystem FileSystem, String Path, byte[] Data)
		{
			using (var FileStream = FileSystem.OpenFile(Path, FileMode.Create))
			{
				FileStream.Write(Data, 0, Data.Length);
			}
		}

		public static byte[] ReadFile(this FileSystem FileSystem, String Path)
		{
			using (var FileStream = FileSystem.OpenFile(Path, FileMode.Open))
			{
				return FileStream.ReadAll();
			}
		}
	}
}
