using System;
using System.Collections.Generic;

namespace CSharpUtils.VirtualFileSystem
{
	public class ProxyFileSystem : FileSystem
	{
		protected FileSystem ParentFileSystem;

		public ProxyFileSystem(FileSystem ParentFileSystem)
		{
			this.ParentFileSystem = ParentFileSystem;
		}

		public override void TryInitialize()
		{
			this.ParentFileSystem.TryInitialize();
		}

		protected override void ImplSetFileTime(string Path, FileSystemEntry.FileTime FileTime)
		{
			//this.ParentFileSystem.ImplSetFileTime(Path, FileTime);
			this.ParentFileSystem.SetFileTime(Path, FileTime);
		}

		protected override FileSystemEntry ImplGetFileInfo(string Path)
		{
			//return this.ParentFileSystem.ImplGetFileInfo(Path);
			return this.ParentFileSystem.GetFileInfo(Path);
		}

		protected override void ImplDeleteFile(string Path)
		{
			//this.ParentFileSystem.ImplDeleteFile(Path);
			this.ParentFileSystem.DeleteFile(Path);
		}

		protected override void ImplDeleteDirectory(string Path)
		{
			//this.ParentFileSystem.ImplDeleteDirectory(Path);
			this.ParentFileSystem.DeleteDirectory(Path);
		}

		protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			//this.ParentFileSystem.ImplMoveFile(ExistingFileName, NewFileName, ReplaceExisiting);
			this.ParentFileSystem.MoveFile(ExistingFileName, NewFileName, ReplaceExisiting);
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			//return this.ParentFileSystem.ImplFindFiles(Path);
			return this.ParentFileSystem.FindFiles(Path);
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			//return this.ParentFileSystem.ImplOpenFile(FileName, FileMode);
			return this.ParentFileSystem.OpenFile(FileName, FileMode);
		}

#if false
		protected override void ImplWriteFile(FileSystemFileStream FileStream, byte[] Buffer, int Offset, int Count)
		{
			//this.ParentFileSystem.ImplWriteFile(FileStream, Buffer, Offset, Count);
			this.ParentFileSystem.WriteFile(FileStream, Buffer, Offset, Count);
		}

		protected override int ImplReadFile(FileSystemFileStream FileStream, byte[] Buffer, int Offset, int Count)
		{
			//return this.ParentFileSystem.ImplReadFile(FileStream, Buffer, Offset, Count);
			return this.ParentFileSystem.ReadFile(FileStream, Buffer, Offset, Count);
		}

		protected override void ImplCloseFile(FileSystemFileStream FileStream)
		{
			//this.ParentFileSystem.ImplCloseFile(FileStream);
			this.ParentFileSystem.CloseFile(FileStream);
		}
#endif

		protected override void ImplCreateDirectory(string Path, int Mode = 0777)
		{
			//this.ParentFileSystem.ImplCreateDirectory(Path, Mode);
			this.ParentFileSystem.CreateDirectory(Path, Mode);
		}

		public override String Title
		{
			get
			{
				return this.ParentFileSystem.Title;
			}
		}

		protected override void ImplCreateSymLink(string Pointer, string Pointee)
		{
			throw new NotImplementedException();
		}
	}
}
