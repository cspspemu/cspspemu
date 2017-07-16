using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem.Memory
{
	public class MemoryFileSystem : NodeFileSystem
	{
		public event Action OnTryInitialize;

		public override void TryInitialize()
		{
			if (OnTryInitialize != null)
			{
				OnTryInitialize();
			}
		}

		public Node AddFile(String AddFileName, Lazy<Stream> Contents)
		{
			AddFileName = AbsoluteNormalizePath(AddFileName);
			var Node = RootNode.Access(AddFileName, true);
			Node.FileSystemEntry.Time = new FileSystemEntry.FileTime()
			{
				CreationTime = DateTime.Now,
				LastAccessTime = DateTime.Now,
				LastWriteTime = DateTime.Now,
			};
			Node.FileSystemFileStream = new FileSystemFileStreamStream(this, Contents);
			return Node;
		}

		public Node AddFile(String AddFileName, Stream Contents)
		{
			return AddFile(AddFileName, new Lazy<Stream>(() => Contents));
		}

		public String _Title = "memory://";

		public override String Title
		{
			get
			{
				return _Title;
			}
		}
	}
}
