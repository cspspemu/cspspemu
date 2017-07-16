using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public class FileSystemEntry
	{
		public enum EntryType
		{
			Unknown   = 0,
			File      = 1,
			Directory = 2,
			Link      = 3,
		}

		public enum ExtendedFlagsTypes
		{
			None       = 0,
			Hidden     = 1,
			Compressed = 2,
			Encrypted  = 4,
			System     = 8,
			Archive    = 16,
			Device     = 32,
		}

		public enum SpecialFlagsTypes
		{
			None = 0,
			SynchronizeAlways = 1,
		}

		public class FileTime
		{
			protected DateTimeRange _CreationTime;
			protected DateTimeRange _LastAccessTime;
			protected DateTimeRange _LastWriteTime;

			public FileTime()
			{
				var MinDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
				_CreationTime = MinDateTime;
				_LastAccessTime = MinDateTime;
				_LastWriteTime = MinDateTime;
			}


			protected static DateTimeRange Normalize(DateTimeRange value)
			{
				var MinDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
				if (value < MinDateTime)
				{
					return MinDateTime;
				}
				return value;
			}

			public DateTimeRange CreationTime {
				get
				{
					return _CreationTime;
				}
				set
				{
					_CreationTime = Normalize(value);
				}
			}
			
			public DateTimeRange LastAccessTime {
				get
				{
					return _LastAccessTime;
				}
				set
				{
					_LastAccessTime = Normalize(value);
				}
			}

			public DateTimeRange LastWriteTime
			{
				get
				{
					return _LastWriteTime;
				}
				set
				{
					_LastWriteTime = Normalize(value);
				}
			}
		}

		public FileSystem FileSystem;
		public String Path;
		public FileTime Time = new FileTime();
		public long Size;
		public int UserId;
		public int GroupId;
		public EntryType Type = EntryType.Unknown;
		public ExtendedFlagsTypes ExtendedFlags;
		public SpecialFlagsTypes SpecialFlags;

		public String FullName { get {
			return Path;
		} }

		public String Name { get {
			return Path.Substring(Path.LastIndexOf('/') + 1);
		} }

		public FileSystemEntry(FileSystem FileSystem, String Path)
		{
			this.FileSystem = FileSystem;
			this.Path = Path;
		}

		public virtual Stream Open(FileMode FileMode)
		{
			return FileSystem.OpenFile(Path, FileMode);
		}
		
		public override string ToString()
		{
			return "FileSystemEntry(FullName=" + FullName + ", Name=" + Name + ", Size=" + Size + ", Type=" + Type + ")";
		}

		public FileSystemEntry Clone()
		{
			var That = new FileSystemEntry(this.FileSystem, this.Path)
			{
				ExtendedFlags = this.ExtendedFlags,
				FileSystem = this.FileSystem,
				Path = this.Path,
				Size = this.Size,
				Time = this.Time,
				Type = this.Type,
				GroupId = this.GroupId,
				UserId = this.UserId,
			};
			return That;
		}
	}
}
