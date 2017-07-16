using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CSharpUtils.VirtualFileSystem.Local
{
	public class LocalFileSystem : ImplFileSystem
	{
		protected String RootPath;

		[DebuggerHidden]
		override protected bool CaseInsensitiveFileSystem { get { return true; } }

		[DebuggerHidden]
		public LocalFileSystem(String RootPath, bool CreatePath = false)
		{
			if (CreatePath && !Directory.Exists(RootPath))
			{
				Directory.CreateDirectory(RootPath);
			}
			this.RootPath = AbsoluteNormalizePath(RootPath);
		}

		[DebuggerHidden]
		protected String RealPath(String Path)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return CombinePath(RootPath, Path);
			}
			return CombinePath(RootPath, Path).Replace('/', '\\');
		}

		[DebuggerHidden]
		override protected FileSystemEntry ImplGetFileInfo(String Path)
		{
			String CachedRealPath = RealPath(Path);
			FileSystemInfo FileSystemInfo;
			if (Directory.Exists(CachedRealPath)) {
				FileSystemInfo = new DirectoryInfo(CachedRealPath);
			} else {
				FileSystemInfo = new FileInfo(CachedRealPath);
			}
			
			return new LocalFileSystemEntry(this, Path, FileSystemInfo);
		}

		[DebuggerHidden]
		override protected IEnumerable<FileSystemEntry> ImplFindFiles(String Path)
		{
			String CachedRealPath = RealPath(Path);

			DirectoryInfo DirectoryInfo = new DirectoryInfo(CachedRealPath);

			foreach (var Item in DirectoryInfo.EnumerateFileSystemInfos())
			{
				if (!Item.FullName.StartsWith(CachedRealPath))
				{
					throw(new Exception("Unexpected FullName"));
				}

				if (Item.Attributes.HasFlag(FileAttributes.Hidden))
				{
					continue;
				}

				var FileSystemEntry = new LocalFileSystemEntry(this, Path + "/" + Item.Name, Item);


				//FileSystemEntry.Size = File.get
				//Item.Attributes == FileAttributes.
				//
				
				//FileSystemEntry.Time = Item.
				yield return FileSystemEntry;
			}
		}

		override protected void ImplCreateDirectory(String Path, int Mode = 0777)
		{
			Directory.CreateDirectory(RealPath(Path));
		}

		override protected void ImplDeleteFile(string Path)
		{
			File.Delete(RealPath(Path));
		}

		override protected FileSystemFileStream ImplOpenFile(String FileName, FileMode FileMode)
		{
			//var Stream = File.Open(RealPath(FileName), FileMode, FileAccess.Read, FileShare.ReadWrite);
			var Stream = File.Open(RealPath(FileName), FileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
			return new FileSystemFileStreamStream(this, Stream);
		}

		protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			var RealExistingFileName = RealPath(ExistingFileName);
			var RealNewFileName = RealPath(NewFileName);

			if (ReplaceExisiting && File.Exists(RealNewFileName))
			{
				File.Delete(RealNewFileName);
			}
			File.Move(RealExistingFileName, RealNewFileName);
		}

		public override String Title
		{
			get
			{
				//return "local://" + RootPath;
				return RootPath;
			}
		}
	}
}
