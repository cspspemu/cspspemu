using System;
using CSharpUtils.Net;

namespace CSharpUtils.VirtualFileSystem.Ftp
{
	public class FtpFileSystemEntry : FileSystemEntry
	{
		protected FTPEntry FTPEntry;

		public FtpFileSystemEntry(FileSystem FileSystem, String Path, FTPEntry FTPEntry)
			: base(FileSystem, Path)
		{
			this.FTPEntry = FTPEntry;

			//FtpEntry.
			this.Time.LastWriteTime = FTPEntry.ModifiedTime;
			this.Size = FTPEntry.Size;
			this.UserId = FTPEntry.UserId;
			this.GroupId = FTPEntry.GroupId;
			switch (FTPEntry.Type)
			{
				case FTPEntry.FileType.Directory:
					this.Type = FileSystemEntry.EntryType.Directory;
					break;
				case FTPEntry.FileType.Link:
					this.Type = FileSystemEntry.EntryType.Link;
					break;
				default:
				case FTPEntry.FileType.File:
					this.Type = FileSystemEntry.EntryType.File;
					break;
			}
		}
	}
}
