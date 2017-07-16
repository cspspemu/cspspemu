using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem.Local
{
	public class LocalTemporalFileSystem : LocalFileSystem
	{
		public LocalTemporalFileSystem() : base(GetTempPath())
		{
		}

		public override void Shutdown()
		{
			Directory.Delete(this.RootPath, true);
		}

		public static String GetTempPath()
		{
			String DirectoryPath = Path.GetTempPath() + @"\" + System.Guid.NewGuid() + "_" + System.Guid.NewGuid();
			Directory.CreateDirectory(DirectoryPath);
			return DirectoryPath;
		}
	}
}
