using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSharpUtils.Endian;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;

namespace CSPspEmu.Hle.Formats
{
	unsafe public class IsoFile
	{
		public const uint SectorSize = 0x800;
	
		internal Stream Stream;
		public IsoNode Root;
		public PrimaryVolumeDescriptor PrimaryVolumeDescriptor;
		public string IsoPath;
	
		protected void ProcessDirectoryRecord(IsoNode ParentIsoNode)
		{
			var DirectoryStart  = ParentIsoNode.DirectoryRecord.Extent * SectorSize;
			var DirectoryLength = ParentIsoNode.DirectoryRecord.Size;
			var DirectoryStream = this.Stream.SliceWithLength(DirectoryStart, DirectoryLength);

			while (!DirectoryStream.Eof())
			{
				//writefln("%08X : %08X : %08X", directoryStream.position, directoryStart, directoryLength);
				byte DirectoryRecordSize;
				DirectoryRecordSize = (byte)DirectoryStream.ReadByte();

				// Even if a directory spans multiple sectors, the directory entries are not permitted to cross the sector boundary (unlike the path table).
				// Where there is not enough space to record an entire directory entry at the end of a sector, that sector is zero-padded and the next
				// consecutive sector is used.
				if (DirectoryRecordSize == 0)
				{
					DirectoryStream.Position = MathUtils.NextAligned(DirectoryStream.Position, SectorSize);
					//Console.WriteLine("AlignedTo: {0:X}", DirectoryStream.Position);
					continue;
				}

				DirectoryStream.Position = DirectoryStream.Position - 1;

				//Console.WriteLine("[{0}:{1:X}-{2:X}]", DirectoryRecordSize, DirectoryStream.Position, DirectoryStream.Position + DirectoryRecordSize);

				byte[] DirectoryRecordBytes = new byte[DirectoryRecordSize];
				DirectoryStream.Read(DirectoryRecordBytes, 0, DirectoryRecordSize);
				var DirectoryRecord = StructUtils.BytesToStruct<DirectoryRecord>(DirectoryRecordBytes);
			
				string name = Encoding.UTF8.GetString(DirectoryRecordBytes.Slice(sizeof(DirectoryRecord), DirectoryRecord.NameLength));

				//Console.WriteLine("{0}", name); Console.ReadKey();
			
				if (name == "\x00" || name == "\x01") continue;
			
				//writefln("   %s", name);
			
				var childIsoNode = new IsoNode(this, DirectoryRecord, name, ParentIsoNode);
				ParentIsoNode._Childs.Add(childIsoNode);
				ParentIsoNode._childsByName[childIsoNode.Name] = childIsoNode;
				ParentIsoNode._childsByNameUpperCase[childIsoNode.Name.ToUpper()] = childIsoNode;
			}
		
			foreach (var child in ParentIsoNode._Childs)
			{
				if (child.IsDirectory) ProcessDirectoryRecord(child);
			}
		}
	
		public IsoFile() {
		
		}

		public IsoFile(Stream Stream)
		{
			SetStream(Stream);
		}

		public IsoFile(string path)
		{
			SetStream(File.OpenRead(path), path);
		}

		public IsoFile(Stream stream, string path = "<unknown>")
		{
			SetStream(stream, path);
		}

		public void SetStream(Stream Stream, string Path = "<unknown>")
		{
			this.Stream = Stream;
			this.IsoPath = Path;
			LoadRootIsoNode();
		}

		protected void LoadRootIsoNode()
		{
			this.Stream.Position = SectorSize * 0x10;
			this.PrimaryVolumeDescriptor = this.Stream.ReadStruct<PrimaryVolumeDescriptor>();
			this.Root = new IsoNode(this, PrimaryVolumeDescriptor.DirectoryRecord);
			ProcessDirectoryRecord(this.Root);
		}
	}

	/// <summary>
	/// 8.4 Primary Volume Descriptor
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct PrimaryVolumeDescriptor
	{
		public VolumeDescriptorHeader VolumeDescriptorHeader;

		public byte Pad1;
		fixed byte SystemId_[0x20];
		fixed byte VolumeId_[0x20];
		public ulong Pad2;
		public u32b VolumeSpaceSize;
		fixed ulong Pad3[4];
		public uint VolumeSetSize;
		public uint VolumeSequenceNumber;
		public u16b LogicalBlockSize;
		public u32b PathTableSize;
		public uint TypeLPathTable;
		public uint OptType1PathTable;
		public uint TypeMPathTable;
		public uint OptTypeMPathTable;

		public DirectoryRecord DirectoryRecord;

		public byte Pad4;
		fixed byte VolumeSetId_[0x80];
		fixed byte PublisherId_[0x80];
		fixed byte PreparerId_[0x80];
		fixed byte ApplicationId_[0x80];
		fixed byte CopyrightFileId_[37];
		fixed byte AbstractFileId_[37];
		fixed byte BibliographicFileId_[37];

		public IsoDate CreationDate;
		public IsoDate ModificationDate;
		public IsoDate ExpirationDate;
		public IsoDate EffectiveDate;
		public byte FileStructureVersion;
		public byte Pad5;
		fixed byte ApplicationData_[0x200];
		fixed byte Pad6_[653];
	}

	/// <summary>
	/// Both Byte Order Types
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct u16b
	{
		ushort LittleEndianValue;
		ushort BigEndianValue;

		public ushort Value
		{
			get
			{
				return LittleEndianValue;
			}
			set
			{
				LittleEndianValue = value;
				BigEndianValue = MathUtils.ByteSwap(value);
			}
		}

		public static implicit operator ushort(u16b Item)
		{
			return Item.Value;
		}
	}

	/// <summary>
	/// Both Byte Order Types
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct u32b
	{
		uint_le LittleEndianValue;
		uint_be BigEndianValue;

		public uint Value
		{
			get
			{
				return LittleEndianValue;
			}
			set
			{
				LittleEndianValue = value;
				BigEndianValue = value;
			}
		}

		public static implicit operator uint(u32b Item)
		{
			return Item.Value;
		}
	}

	/// <summary>
	/// 8.4.26 Volume Creation Date and Time (BP 814 to 830)
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct IsoDate
	{
		fixed byte Data[17];

		private int MapGet(int Offset, int Size)
		{
			int Value = 0;
			fixed (byte* DataPtr = Data)
			{
				for (int n = 0; n < Size; n++)
				{
					Value *= 10;
					Value += ((char)DataPtr[Offset + n]) - '0';
				}
			}
			return 0;
		}

		private void MapSet(int Offset, int Size, int Value)
		{
			fixed (byte* DataPtr = Data)
			{
				for (int n = Size - 1; n >= 0; n--)
				{
					DataPtr[Offset + n] = (byte)((char)(Value % 10) + '0');
					Value /= 10;
				}
			}
		}

		public int Year { get { return MapGet(0, 4); } set { MapSet(0, 4, value); } }
		public int Month { get { return MapGet(4, 2); } set { MapSet(4, 2, value); } }
		public int Day { get { return MapGet(6, 2); } set { MapSet(6, 2, value); } }
		public int Hour { get { return MapGet(8, 2); } set { MapSet(8, 2, value); } }
		public int Minute { get { return MapGet(10, 2); } set { MapSet(10, 2, value); } }
		public int Second { get { return MapGet(12, 2); } set { MapSet(12, 2, value); } }
		public int HSecond { get { return MapGet(14, 2); } set { MapSet(14, 2, value); } }
		public int Offset { get { return MapGet(16, 1); } set { MapSet(16, 1, value); } }

		public DateTime DateTime
		{
			get
			{
				try
				{
					return new DateTime(Year, Month, Day, Hour, Minute, Second);
				}
				catch
				{
					return DateTime.MinValue;
				}
			}
		}

		public override string ToString()
		{
			return String.Format("Iso.Date({0})", DateTime);
		}
	}

	/// <summary>
	/// 9.1 Format of a Directory Record
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DirectoryRecord
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DateStruct
		{
			public byte Year;
			public byte Month;
			public byte Day;
			public byte Hour;
			public byte Minute;
			public byte Second;
			public byte Offset;

			public static implicit operator DateTime(DateStruct Date)
			{
				return new DateTime(Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute, Date.Second);
			}
		}

		[Flags]
		public enum FlagsEnum : byte
		{
			Unknown1 = 1 << 0,
			Directory = 1 << 1,
			Unknown2 = 1 << 2,
			Unknown3 = 1 << 3,
			Unknown4 = 1 << 4,
			Unknown5 = 1 << 5,
		}

		/// <summary>
		/// Length of the entry.
		/// </summary>
		public byte Length;

		/// <summary>
		/// 
		/// </summary>
		public byte ExtendedAttributeLength;

		/// <summary>
		/// Sector of the file in the disc.
		/// </summary>
		public u32b Extent;

		/// <summary>
		/// Size of the file.
		/// </summary>
		public u32b Size;

		/// <summary>
		/// 9.1.5 Recording Date and Time (BP 19 to 25)
		/// </summary>
		public DateStruct Date;

		/// <summary>
		/// 
		/// </summary>
		public FlagsEnum Flags;

		/// <summary>
		/// 
		/// </summary>
		public byte FileUnitSize;

		/// <summary>
		/// 
		/// </summary>
		public byte Interleave;

		/// <summary>
		/// 
		/// </summary>
		public u16b VolumeSequenceNumber;

		/// <summary>
		/// 
		/// </summary>
		public byte NameLength;

		/// <summary>
		/// 
		/// </summary>
		public ulong Offset
		{
			get
			{
				return Extent * IsoFile.SectorSize;
			}
		}

		public override string ToString()
		{
			return String.Format("DirectoryRecord(Length={0})", Length);
		}
	}

	/// <summary>
	/// 8 Volume Descriptors
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct VolumeDescriptorHeader
	{
		public enum TypeEnum : byte
		{
			/// <summary>
			/// 8.2 Boot Record
			/// </summary>
			BootRecord = 0x00,

			/// <summary>
			/// 8.3 Volume Descriptor Set Terminator
			/// </summary>
			VolumePartitionSetTerminator = 0xFF,

			/// <summary>
			/// 8.4 Primary Volume Descriptor
			/// </summary>
			PrimaryVolumeDescriptor = 0x01,

			/// <summary>
			/// 8.5 Supplementary Volume Descriptor
			/// </summary>
			SupplementaryVolumeDescriptor = 0x02,

			/// <summary>
			/// 8.6 Volume Partition Descriptor
			/// </summary>
			VolumePartitionDescriptor = 0x03,
		}

		public TypeEnum Type;
		public fixed byte Id[5];
		public byte Version;
	}

	public class IsoNode : IDisposable
	{
		public IsoFile Iso;
		public DirectoryRecord DirectoryRecord { get; protected set; }
		internal List<IsoNode> _Childs = new List<IsoNode>();
		internal Dictionary<string, IsoNode> _childsByName = new Dictionary<string, IsoNode>();
		internal Dictionary<string, IsoNode> _childsByNameUpperCase = new Dictionary<string, IsoNode>();
		private IsoNode _Parent;
		public string FullPath;
		public string Name;

		internal IsoNode(IsoFile Iso, DirectoryRecord directoryRecord, string name = "", IsoNode parent = null)
		{
			this.Iso = Iso;
			this._Parent = parent;
			this.DirectoryRecord = directoryRecord;
			if (parent != null)
			{
				this.FullPath = parent.FullPath + "/" + name;
			}
			else
			{
				this.FullPath = name;
			}
			this.Name = name;

			//writefln("%s", this.fullPath);
		}

		public bool IsDirectory
		{
			get
			{
				return (DirectoryRecord.Flags & DirectoryRecord.FlagsEnum.Directory) != 0;
			}
		}
		public IEnumerable<IsoNode> Childs { get { return _Childs; } }
		public IsoNode Parent { get { return _Parent; } }

		public IEnumerable<IsoNode> Descendency()
		{
			foreach (var Child in Childs)
			{
				yield return Child;
				if (Child.IsDirectory)
				{
					foreach (var Descendant in Child.Descendency())
					{
						yield return Descendant;
					}
				}
			}
		}

		protected IsoNode AccessChild(string ChildName)
		{
			if (ChildName == "" || ChildName == ".") return this;
			if (ChildName == "..") return (Parent != null) ? Parent : this;
			ChildName = ChildName.ToUpper();

			if (!_childsByNameUpperCase.ContainsKey(ChildName))
			{
				throw (new FileNotFoundException(String.Format("Can't find '{0}' on '{1}'", ChildName, this)));
			}
			return _childsByNameUpperCase[ChildName];
		}

		public IsoNode Locate(string path)
		{
			int index = path.IndexOf('/');
			string childName, descendencyPath;
			if (index < 0)
			{
				childName = path;
				descendencyPath = "";
			}
			else
			{
				childName = path.Substring(0, index);
				descendencyPath = path.Substring(index + 1);
			}
			IsoNode childIsoNode = AccessChild(childName);
			if (descendencyPath != "") childIsoNode = childIsoNode.Locate(descendencyPath);
			return childIsoNode;
		}

		public Stream Open()
		{
			return Iso.Stream.SliceWithLength((long)DirectoryRecord.Offset, (long)DirectoryRecord.Size);
		}

		/*
		void saveTo(string outFileName = null)
		{
			if (outFileName is null) outFileName = this.name;
			BufferedFile outFile = new BufferedFile(outFileName, FileMode.OutNew);
			outFile.copyFrom(open);
			outFile.flush();
			outFile.close();
		}
		*/

		//alias locate opIndex;

		/*
		public IsoRecursiveIterator descendency() {
			return new IsoRecursiveIterator(this);
		}
		*/

		/*
		int opApply(int delegate(ref IsoNode) dg) {
			int result = 0;
		
			foreach (child; _childs) {
				result = dg(child);
				if (result) break;
			}
			return result;
		}
		
		*/

		public override string ToString()
		{
			return String.Format("IsoNode('{0}', {1})", FullPath, DirectoryRecord);
		}

		public void Dispose()
		{
		}
	}
}
