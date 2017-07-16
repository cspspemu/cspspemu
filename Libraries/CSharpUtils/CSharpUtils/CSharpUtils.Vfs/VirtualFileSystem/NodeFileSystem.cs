using System;
using System.Collections.Generic;

namespace CSharpUtils.VirtualFileSystem
{
	public abstract class NodeFileSystem : ImplFileSystem
	{
		public class Node
		{
			public NodeFileSystem NodeFileSystem;
			public String Name;
			public Node Parent;
			public Dictionary<String, Node> Childs = new Dictionary<String, Node>();
			protected FileSystemFileStream _FileSystemFileStream;
			public FileSystemEntry FileSystemEntry;

			public Node SynchronizeAlways() {
				this.FileSystemEntry.SpecialFlags |= FileSystemEntry.SpecialFlagsTypes.SynchronizeAlways;
				return this;
			}

			public FileSystemFileStream FileSystemFileStream
			{
				get
				{
					return _FileSystemFileStream;
				}
				set
				{
					_FileSystemFileStream = value;
					FileSystemEntry.Type = (value == null) ? FileSystemEntry.EntryType.Directory : FileSystemEntry.EntryType.File;
				}
			}

			public long Size
			{
				get
				{
					return (FileSystemFileStream != null) ? FileSystemFileStream.Length : 0;
				}
			}

			public bool IsFile
			{
				get
				{
					return !IsDirectory;
				}
			}

			public bool IsDirectory
			{
				get
				{
					return FileSystemFileStream == null;
					//return (FileSystemEntry.Type == FileSystemEntry.EntryType.Directory);
				}
			}

			protected String _FullName;
			public String FullName
			{
				get
				{
					if (_FullName == null)
					{
						_FullName = Name;
						if (Parent != null)
						{
							var ParentFullName = Parent.FullName;
							if (ParentFullName.Length > 0)
							{
								_FullName = Parent.FullName + "/" + Name;
							}
						}
					}
					return _FullName;
				}
			}
			public Node Root
			{
				get
				{
					if (Parent != null) return Parent.Root;
					return this;
				}
			}

			public Node(NodeFileSystem NodeFileSystem, Node Parent, String Name)
			{
				this.NodeFileSystem = NodeFileSystem;
				this.Parent = Parent;
				this.Name = Name;
				this.FileSystemEntry = new FileSystemEntry(NodeFileSystem, this.FullName);
				this.FileSystemFileStream = null;
			}

			protected Node AccessChild(String Name, bool CreateNode = false)
			{
				if ((Name == "") || (Name == "."))
				{
					return this;
				}
				if (Name == "..")
				{
					return Parent;
				}

				if (!Childs.ContainsKey(Name))
				{
					if (CreateNode)
					{
						Childs[Name] = new Node(this.NodeFileSystem, this, Name);
					}
					else
					{
						throw(new Exception(String.Format("Can't Access To '{0}/{1}'", FullName, Name)));
					}
				}
				return Childs[Name];
			}

			public Node Access(String Path, bool CreateNode = false)
			{
				int Index;
				// Has components
				if ((Index = Path.IndexOf('/')) >= 0)
				{
					if (Index == 0)
					{
						//Console.WriteLine("[1]");
					}
					if (Index == Path.Length - 1)
					{
						//Console.WriteLine("[2]");
					}
					return AccessChild(Path.Substring(0, Index), CreateNode).Access(Path.Substring(Index + 1), CreateNode);
				}
				// No more components
				else
				{
					return AccessChild(Path, CreateNode);
				}
			}
		}

		public Node RootNode;

		public NodeFileSystem()
		{
			Initialize();
		}

		public void Initialize()
		{
			RemoveAllFiles();
		}

		protected void SetRootNode()
		{
			RootNode = new Node(this, null, "");
		}

		public void RemoveAllFiles()
		{
			SetRootNode();
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			foreach (var Child in RootNode.Access(Path).Childs.Values)
			{
				yield return Child.FileSystemEntry;
			}
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			return RootNode.Access(FileName).FileSystemFileStream;
			//return base.ImplOpenFile(FileName, FileMode);
		}

		public override String Title
		{
			get
			{
				return String.Format("nodefs://");
			}
		}
	}
}
