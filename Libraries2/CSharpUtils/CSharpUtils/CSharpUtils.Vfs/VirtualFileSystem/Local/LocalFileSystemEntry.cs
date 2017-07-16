using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem.Local
{
	public class LocalFileSystemEntry : FileSystemEntry
	{
		protected FileSystemInfo FileSystemInfo;

		public LocalFileSystemEntry(FileSystem FileSystem, String Path, FileSystemInfo FileSystemInfo)
			: base(FileSystem, Path)
		{
			this.FileSystemInfo = FileSystemInfo;

			if (FileSystemInfo.Attributes.HasFlag(FileAttributes.Directory))
			{
				this.Type = VirtualFileSystem.FileSystemEntry.EntryType.Directory;
			}
			else
			{
				this.Type = VirtualFileSystem.FileSystemEntry.EntryType.File;
			}

			this.Time.CreationTime = FileSystemInfo.CreationTime;
			this.Time.LastWriteTime = FileSystemInfo.LastWriteTime;
			this.Time.LastAccessTime = FileSystemInfo.LastAccessTime;

			if (FileSystemInfo is FileInfo)
			{
				var ItemFile = (FileInfo)FileSystemInfo;
				this.Size = ItemFile.Length;
			}
		}
	}
}
