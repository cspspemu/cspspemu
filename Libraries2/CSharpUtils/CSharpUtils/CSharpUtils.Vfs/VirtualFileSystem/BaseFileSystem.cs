using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpUtils.VirtualFileSystem
{
	abstract public partial class FileSystem : IDisposable
	{
		public struct MountStruct
		{
			public FileSystem FileSystem;
			public String Path;
		}

		internal SortedDictionary<String, MountStruct> MountedFileSystems = new SortedDictionary<string, MountStruct>();

		String CurrentWorkingPath = "";
		virtual protected bool CaseInsensitiveFileSystem { get { return false; } }

		~FileSystem()
		{
			Shutdown();
		}

		virtual public void Shutdown()
		{
		}

		static public String CombinePath(String BasePath, String PathToCombine)
		{
			return BasePath.TrimEnd('/') + "/" + PathToCombine.TrimStart('/');
		}

		static public String AbsoluteNormalizePath(String Path, String CurrentWorkingPath = "")
		{
			var Components = new LinkedList<String>();

			// Normalize slashes.
			Path = Path.Replace('\\', '/');

			// Relative Path
			if (Path.StartsWith("/"))
			{
				Path = CurrentWorkingPath + "/" + Path;
			}

			// Normalize Components
			foreach (var Component in Path.Split('/'))
			{
				switch (Component)
				{
					case "": case ".": break;
					case "..": Components.RemoveLast(); break;
					default: Components.AddLast(Component); break;
				}
			}

			return String.Join("/", Components).TrimStart('/');
		}

		public String ComparablePath(String Path)
		{
			if (CaseInsensitiveFileSystem)
			{
				return Path.ToLower();
			}
			else
			{
				return Path;
			}
		}

		virtual protected void Access(String Path, out FileSystem NewFileSystem, out String NewPath)
		{
			// Normalize Components
			Path = AbsoluteNormalizePath(Path, CurrentWorkingPath);
			var ComparePath = ComparablePath(Path);

			// Check MountedFileSystems.
			foreach (var Item in MountedFileSystems)
			{
				var CheckMountedPath = ComparablePath(Item.Key);
				var MountInfo = Item.Value;

				if (
					ComparePath.StartsWith(CheckMountedPath) &&
					(
						(CheckMountedPath.Length == ComparePath.Length) ||
						(ComparePath.Substring(CheckMountedPath.Length, 1) == "/")
					)
				) {
					var NewAccessPath = ComparePath.Substring(CheckMountedPath.Length);
					if (MountInfo.Path != "/")
					{
						NewAccessPath = MountInfo.Path + "/" + NewAccessPath;
					}
					// Use Mounted File System.
					MountInfo.FileSystem.Access(NewAccessPath, out NewFileSystem, out NewPath);
					return;
				}
			}
			NewFileSystem = this;
			NewPath = Path;
		}

		public void Mount(String Path, FileSystem FileSystemToMount, String FileSystemToMountPath = "/")
		{
			String FinalPath = AbsoluteNormalizePath(Path, CurrentWorkingPath);
			MountedFileSystems[FinalPath] = new MountStruct()
			{
				FileSystem = FileSystemToMount,
				Path = FileSystemToMountPath,
			};
		}

		public void UnMount(String Path)
		{
			String FinalPath = AbsoluteNormalizePath(Path, CurrentWorkingPath);
			MountedFileSystems.Remove(FinalPath);
		}

		public void Dispose()
		{
			Shutdown();
		}

		public FileSystemFromPath FileSystemFromPath(String Path, bool AllowAccessingParent = false)
		{
			return new FileSystemFromPath(this, Path, AllowAccessingParent);
		}

		public virtual void TryInitialize()
		{
		}

		public virtual String Title
		{
			get
			{
				return "BaseFileSystem";
			}
		}

		public void Copy(string SrcFile, string DstFile, bool Overwrite = false)
		{
			if (!Overwrite && Exists(DstFile))
			{
				throw(new System.IO.IOException("File already exists"));
			}

			using (var Src = OpenFile(SrcFile, FileMode.Open))
			using (var Dst = OpenFile(DstFile, FileMode.Truncate))
			{
				Src.CopyTo(Dst, (int)Math.Min(Src.Length, 2 * 1024 * 1024));
			}
		}
	}
}
