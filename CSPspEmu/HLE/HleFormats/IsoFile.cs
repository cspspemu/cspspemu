using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSharpUtils.Endian;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
    public unsafe class IsoFile : IDisposable
    {
        public const uint SectorSize = 0x800;

        public Stream Stream { get; internal set; }
        public IsoNode Root;
        public PrimaryVolumeDescriptor PrimaryVolumeDescriptor;
        public string IsoPath;

        protected void ProcessDirectoryRecord(IsoNode parentIsoNode)
        {
            var directoryStart = parentIsoNode.DirectoryRecord.Extent * SectorSize;
            var directoryLength = parentIsoNode.DirectoryRecord.Size;
            var directoryStream = Stream.SliceWithLength(directoryStart, directoryLength);

            while (!directoryStream.Eof())
            {
                //writefln("%08X : %08X : %08X", directoryStream.position, directoryStart, directoryLength);
                var directoryRecordSize = (byte) directoryStream.ReadByte();

                // Even if a directory spans multiple sectors, the directory entries are not permitted to cross the sector boundary (unlike the path table).
                // Where there is not enough space to record an entire directory entry at the end of a sector, that sector is zero-padded and the next
                // consecutive sector is used.
                if (directoryRecordSize == 0)
                {
                    directoryStream.Position = MathUtils.NextAligned(directoryStream.Position, SectorSize);
                    //Console.WriteLine("AlignedTo: {0:X}", DirectoryStream.Position);
                    continue;
                }

                directoryStream.Position = directoryStream.Position - 1;

                //Console.WriteLine("[{0}:{1:X}-{2:X}]", DirectoryRecordSize, DirectoryStream.Position, DirectoryStream.Position + DirectoryRecordSize);

                var directoryRecordBytes = new byte[directoryRecordSize];
                directoryStream.Read(directoryRecordBytes, 0, directoryRecordSize);
                var directoryRecord = StructUtils.BytesToStruct<DirectoryRecord>(directoryRecordBytes);

                var name =
                    Encoding.UTF8.GetString(directoryRecordBytes.Slice(sizeof(DirectoryRecord),
                        directoryRecord.NameLength));

                //Console.WriteLine("{0}", name); Console.ReadKey();

                if (name == "\x00" || name == "\x01") continue;

                //writefln("   %s", name);

                var childIsoNode = new IsoNode(this, directoryRecord, name, parentIsoNode);
                parentIsoNode.Childs2.Add(childIsoNode);
                parentIsoNode.ChildsByName[childIsoNode.Name] = childIsoNode;
                parentIsoNode.ChildsByNameUpperCase[childIsoNode.Name.ToUpper()] = childIsoNode;
            }

            foreach (var child in parentIsoNode.Childs2)
            {
                if (child.IsDirectory) ProcessDirectoryRecord(child);
            }
        }

        public IsoFile()
        {
        }

        public IsoFile(Stream stream) => SetStream(stream);

        public IsoFile(string path) => SetStream(File.OpenRead(path), path);

        public IsoFile(Stream stream, string path = "<unknown>") => SetStream(stream, path);

        public void SetStream(Stream stream, string path = "<unknown>")
        {
            Stream = stream;
            IsoPath = path;
            LoadRootIsoNode();
        }

        protected void LoadRootIsoNode()
        {
            Stream.Position = SectorSize * 0x10;
            PrimaryVolumeDescriptor = Stream.ReadStruct<PrimaryVolumeDescriptor>();
            Root = new IsoNode(this, PrimaryVolumeDescriptor.DirectoryRecord);
            ProcessDirectoryRecord(Root);
        }

        public void Dispose() => Stream.Dispose();
    }

    /// <summary>
    /// 8.4 Primary Volume Descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PrimaryVolumeDescriptor
    {
        public VolumeDescriptorHeader VolumeDescriptorHeader;

        public byte Pad1;
        fixed byte SystemId_[0x20];
        fixed byte VolumeId_[0x20];
        public ulong Pad2;
        public U32B VolumeSpaceSize;
        fixed ulong Pad3[4];
        public uint VolumeSetSize;
        public uint VolumeSequenceNumber;
        public U16B LogicalBlockSize;
        public U32B PathTableSize;
        public uint TypeLPathTable;
        public uint OptType1PathTable;
        public uint TypeMPathTable;
        public uint OptTypeMPathTable;

        public DirectoryRecord DirectoryRecord;

        public byte Pad4;
        private fixed byte VolumeSetId_[0x80];
        private fixed byte PublisherId_[0x80];
        private fixed byte PreparerId_[0x80];
        private fixed byte ApplicationId_[0x80];
        private fixed byte CopyrightFileId_[37];
        private fixed byte AbstractFileId_[37];
        private fixed byte BibliographicFileId_[37];

        public IsoDate CreationDate;
        public IsoDate ModificationDate;
        public IsoDate ExpirationDate;
        public IsoDate EffectiveDate;
        public byte FileStructureVersion;
        public byte Pad5;
        private fixed byte ApplicationData_[0x200];
        private fixed byte Pad6_[653];
    }

    /// <summary>
    /// Both Byte Order Types
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct U16B
    {
        private ushort LittleEndianValue;
        private ushort BigEndianValue;

        public ushort Value
        {
            get => LittleEndianValue;
            set
            {
                LittleEndianValue = value;
                BigEndianValue = MathUtils.ByteSwap(value);
            }
        }

        public static implicit operator ushort(U16B item) => item.Value;
    }

    /// <summary>
    /// Both Byte Order Types
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct U32B
    {
        private UintLe LittleEndianValue;
        private UintBe BigEndianValue;

        public uint Value
        {
            get => LittleEndianValue;
            set
            {
                LittleEndianValue = value;
                BigEndianValue = value;
            }
        }

        public static implicit operator uint(U32B item)
        {
            return item.Value;
        }
    }

    /// <summary>
    /// 8.4.26 Volume Creation Date and Time (BP 814 to 830)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct IsoDate
    {
        fixed byte Data[17];

        private int MapGet(int offset, int size)
        {
            var value = 0;
            fixed (byte* dataPtr = Data)
            {
                for (var n = 0; n < size; n++)
                {
                    value *= 10;
                    value += (char) dataPtr[offset + n] - '0';
                }
            }
            return value;
        }

        private void MapSet(int offset, int size, int value)
        {
            fixed (byte* dataPtr = Data)
            {
                for (int n = size - 1; n >= 0; n--)
                {
                    dataPtr[offset + n] = (byte) ((char) (value % 10) + '0');
                    value /= 10;
                }
            }
        }

        public int Year
        {
            get => MapGet(0, 4);
            set => MapSet(0, 4, value);
        }

        public int Month
        {
            get => MapGet(4, 2);
            set => MapSet(4, 2, value);
        }

        public int Day
        {
            get => MapGet(6, 2);
            set => MapSet(6, 2, value);
        }

        public int Hour
        {
            get => MapGet(8, 2);
            set => MapSet(8, 2, value);
        }

        public int Minute
        {
            get => MapGet(10, 2);
            set => MapSet(10, 2, value);
        }

        public int Second
        {
            get => MapGet(12, 2);
            set => MapSet(12, 2, value);
        }

        public int HSecond
        {
            get => MapGet(14, 2);
            set => MapSet(14, 2, value);
        }

        public int Offset
        {
            get => MapGet(16, 1);
            set => MapSet(16, 1, value);
        }

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

        public override string ToString() => $"Iso.Date({DateTime})";
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

            public static implicit operator DateTime(DateStruct date)
            {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
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
        public U32B Extent;

        /// <summary>
        /// Size of the file.
        /// </summary>
        public U32B Size;

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
        public U16B VolumeSequenceNumber;

        /// <summary>
        /// 
        /// </summary>
        public byte NameLength;

        /// <summary>
        /// 
        /// </summary>
        public ulong Offset => Extent * IsoFile.SectorSize;

        public override string ToString() => $"DirectoryRecord(Length={Length})";
    }

    /// <summary>
    /// 8 Volume Descriptors
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct VolumeDescriptorHeader
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

        public string IdString
        {
            get
            {
                fixed (byte* idPtr = Id) return PointerUtils.PtrToString(idPtr, 5, Encoding.UTF8);
            }
        }
    }

    public class IsoNode : IDisposable
    {
        public IsoFile Iso;
        public DirectoryRecord DirectoryRecord { get; protected set; }
        public List<IsoNode> Childs2 = new List<IsoNode>();
        internal Dictionary<string, IsoNode> ChildsByName = new Dictionary<string, IsoNode>();
        internal Dictionary<string, IsoNode> ChildsByNameUpperCase = new Dictionary<string, IsoNode>();
        public string FullPath;
        public string Name;

        internal IsoNode(IsoFile iso, DirectoryRecord directoryRecord, string name = "", IsoNode parent = null)
        {
            Iso = iso;
            Parent = parent;
            DirectoryRecord = directoryRecord;
            if (parent != null)
            {
                FullPath = parent.FullPath + "/" + name;
            }
            else
            {
                FullPath = name;
            }
            Name = name;

            //writefln("%s", this.fullPath);
        }

        public bool IsDirectory => (DirectoryRecord.Flags & DirectoryRecord.FlagsEnum.Directory) != 0;

        public IEnumerable<IsoNode> Childs => Childs2;

        public IsoNode Parent { get; }

        public IEnumerable<IsoNode> Descendency()
        {
            foreach (var child in Childs)
            {
                yield return child;
                if (child.IsDirectory)
                {
                    foreach (var descendant in child.Descendency())
                    {
                        yield return descendant;
                    }
                }
            }
        }

        protected IsoNode AccessChild(string childName)
        {
            if (childName == "" || childName == ".") return this;
            if (childName == "..") return Parent ?? this;
            childName = childName.ToUpper();

            if (!ChildsByNameUpperCase.ContainsKey(childName))
            {
                throw new FileNotFoundException($"Can't find '{childName}' on '{this}'");
            }
            return ChildsByNameUpperCase[childName];
        }

        public IsoNode Locate(string path)
        {
            var index = path.IndexOf('/');
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
            var childIsoNode = AccessChild(childName);
            if (descendencyPath != "") childIsoNode = childIsoNode.Locate(descendencyPath);
            return childIsoNode;
        }

        public Stream Open() => Iso.Stream.SliceWithLength((long) DirectoryRecord.Offset, DirectoryRecord.Size);

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
            return $"IsoNode('{FullPath}', {DirectoryRecord})";
        }

        public void Dispose()
        {
            Iso.Dispose();
        }
    }
}