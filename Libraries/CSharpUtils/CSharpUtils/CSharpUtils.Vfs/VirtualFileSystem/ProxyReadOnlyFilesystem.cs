using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public class ProxyReadOnlyFilesystem : ProxyFileSystem
	{
		public ProxyReadOnlyFilesystem(FileSystem ParentFileSystem) : base(ParentFileSystem)
		{
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			switch (FileMode)
			{
				case FileMode.Open:
					return base.ImplOpenFile(FileName, FileMode);
				default:
					throw(new NotImplementedException());
			}
		}

		sealed protected override void ImplDeleteFile(string Path)
		{
			throw(new NotImplementedException());
		}

		sealed protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			throw (new NotImplementedException());
		}
	}
}
