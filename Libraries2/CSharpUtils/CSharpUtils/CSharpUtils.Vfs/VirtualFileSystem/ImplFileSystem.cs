using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public class ImplFileSystem : FileSystem
	{
		protected override void ImplSetFileTime(string Path, FileSystemEntry.FileTime FileTime)
		{
			throw new NotImplementedException();
		}

		protected override FileSystemEntry ImplGetFileInfo(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplDeleteFile(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplDeleteDirectory(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			//throw new NotImplementedException();
			return new List<FileSystemEntry>();
			//yield return new FileSystemEntry();
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, FileMode FileMode)
		{
			throw new NotImplementedException();
		}

		protected override void ImplCreateDirectory(string Path, int Mode = 0777)
		{
			throw new NotImplementedException();
		}

		public override String Title
		{
			get
			{
				return this.ToString();
			}
		}

		protected override void ImplCreateSymLink(string Pointer, string Pointee)
		{
			throw new NotImplementedException();
		}
	}
}
