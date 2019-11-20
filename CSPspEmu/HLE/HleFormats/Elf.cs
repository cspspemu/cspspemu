using System;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
    public unsafe class Elf : IFormatDetector
    {
        public struct HeaderStruct
        {
            public enum MagicEnum : uint
            {
                ExpectedValue = 0x464C457F
            }

            public enum TypeEnum : ushort
            {
                Executable = 0x0002,
                Prx = 0xFFA0,
            }

            public enum MachineEnum : ushort
            {
                Allegrex = 8,
            }

            // e_ident 16 bytes.

            /// <summary>
            /// Magic identifying the file : [0x7F, 'E', 'L', 'F']
            /// </summary>
            public MagicEnum Magic;

            /// <summary>
            /// ?
            /// </summary>
            public byte Class;

            /// <summary>
            /// ?
            /// </summary>
            public byte Data;

            /// <summary>
            /// ?
            /// </summary>
            public byte IdVersion;

            /// <summary>
            /// Padding
            /// </summary>
            public fixed byte Padding[9];

            /// <summary>
            /// Identifies object file type
            /// </summary>
            public TypeEnum Type;

            /// <summary>
            /// Architecture build  = Machine.ALLEGREX
            /// </summary>
            public MachineEnum Machine;

            /// <summary>
            /// Object file version
            /// </summary>
            public uint Version;

            /// <summary>
            /// Virtual address of code entry. Module EntryPoint (PC)
            /// </summary>
            public uint EntryPoint;

            /// <summary>
            /// Program header table's file offset in bytes
            /// </summary>
            public uint ProgramHeaderOffset;

            /// <summary>
            /// Section header table's file offset in bytes
            /// </summary>
            public uint SectionHeaderOffset;

            /// <summary>
            /// Processor specific flags
            /// </summary>
            public uint Flags;

            /// <summary>
            /// ELF header size in bytes
            /// </summary>
            public ushort ElfHeaderSize;

            // Program Header.

            /// <summary>
            /// Program header size (all the same size)
            /// </summary>
            public ushort ProgramHeaderEntrySize;

            /// <summary>
            /// Number of program headers
            /// </summary>
            public ushort ProgramHeaderCount;

            // Section Header.
            /// <summary>
            /// Section header size (all the same size)
            /// </summary>
            public ushort SectionHeaderEntrySize;

            /// <summary>
            /// Number of section headers
            /// </summary>
            public ushort SectionHeaderCount;

            /// <summary>
            /// Section header table index of the entry associated with the section name string table
            /// </summary>
            public ushort SectionHeaderStringTable;
        }

        public struct ProgramHeader
        {
            public enum TypeEnum : uint
            {
                NoLoad = 0,
                Load = 1,
                Reloc1 = 0x700000A0,
                Reloc2 = 0x700000A1,
            }

            [Flags]
            public enum FlagsSet : uint
            {
                Executable = 0x1,

                // Note: demo PRX's were found to be not writable
                Writable = 0x2,
                Readable = 0x4,
            }

            /// <summary>
            /// Type of segment (p_type)
            /// </summary>
            public TypeEnum Type;

            /// <summary>
            /// Offset for segment's first byte in file (p_offset)
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Virtual address for segment (p_vaddr)
            /// </summary>
            public uint VirtualAddress;

            /// <summary>
            /// Physical address for segment (p_paddr)
            /// </summary>
            public uint PsysicalAddress;

            /// <summary>
            /// Segment image size in file (p_filesz)
            /// </summary>
            public uint FileSize;

            /// <summary>
            /// Segment image size in memory (p_memsz)
            /// </summary>
            public uint MemorySize;

            /// <summary>
            /// Flags Bits (p_flags)
            /// </summary>
            public FlagsSet Flags;

            /// <summary>
            /// Alignment (p_align)
            /// </summary>
            public uint Alignment;
        }

        /// <summary>
        /// ELF Section Header
        /// </summary>
        public struct SectionHeader
        {
            public enum TypeEnum : uint
            {
                Null = 0,
                ProgramBits = 1,
                Symtab = 2,
                Strtab = 3,
                Rela = 4,
                Hash = 5,
                Dynamic = 6,
                Note = 7,
                NoBits = 8,
                Relocation = 9,
                Shlib = 10,
                Dynsym = 11,

                Loproc = 0x70000000,
                Hiproc = 0x7FFFFFFF,
                Louser = 0x80000000,
                Hiuser = 0xFFFFFFFF,

                PrxRelocation = Loproc | 0xA0,
                PrxRelocationFw5 = Loproc | 0xA1,
            }

            public enum FlagsSet : uint
            {
                None = 0,
                Write = 1,
                Allocate = 2,
                Execute = 4
            }

            /// <summary>
            /// Position relative to .shstrtab of a stringz with the name.
            /// </summary>
            public uint Name;

            /// <summary>
            /// Type of this section header.
            /// </summary>
            public TypeEnum Type;

            /// <summary>
            /// Flags associated to this section header.
            /// </summary>
            public FlagsSet Flags;

            /// <summary>
            /// Memory address where it should be stored.
            /// </summary>
            public uint Address;

            /// <summary>
            /// File position where is the data related to this section header.
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Size of the section header.
            /// </summary>
            public int Size;

            /// <summary>
            /// ?
            /// </summary>
            public uint Link;

            /// <summary>
            /// ?
            /// </summary>
            public uint Info;

            /// <summary>
            /// ?
            /// </summary>
            public uint AddressAlign;

            /// <summary>
            /// ?
            /// </summary>
            public uint EntitySize;

            public override string ToString()
            {
                return this.ToStringDefault();
            }
        }

        public struct Reloc
        {
            public enum TypeEnum : byte
            {
                None = 0,
                Mips16 = 1,
                Mips32 = 2,
                MipsRel32 = 3,
                Mips26 = 4,
                MipsHi16 = 5,
                MipsLo16 = 6,
                MipsGpRel16 = 7,
                MipsLiteral = 8,
                MipsGot16 = 9,
                MipsPc16 = 10,
                MipsCall16 = 11,
                MipsGpRel32 = 12,
                StopRelocation = 0xFF,
            }

            /// <summary>
            /// Address relative to OffsetBase where is the pointer to relocate. (r_offset)
            /// </summary>
            public uint PointerAddress;

            /// <summary>
            /// Packed data containing AddressBase, OffsetBase and Type. (r_info)
            /// </summary>
            public uint Info;

            /// <summary>
            /// Program Header Index where is located the referenced data (pointee) (ADDR_BASE)
            /// </summary>
            public uint PointeeSectionHeaderBase => (Info >> 16) & 0xFF;

            /// <summary>
            /// Program Header Index where is located the data to relocation (pointer) (OFS_BASE)
            /// </summary>
            public uint PointerSectionHeaderBase => (Info >> 8) & 0xFF;

            /// <summary>
            /// Type of relocation (R_TYPE)
            /// </summary>
            public TypeEnum Type => (TypeEnum) ((Info >> 0) & 0xFF);

            public override string ToString()
            {
                return this.ToStringDefault();
            }
        }

        public struct Symbol
        {
            public enum TypeEnum : byte
            {
                NoType = 0,
                Object = 1,
                Function = 2,
                Section = 3,
                File = 4,
                LoProc = 13,
                HiProc = 15,
            }

            public enum BindEnum : byte
            {
                Local = 0,
                Global = 1,
                Weak = 2,
                LoProc = 13,
                HiProc = 15,
            }

            /// <summary>
            /// ?
            /// </summary>
            public string Name;

            /// <summary>
            /// ?
            /// </summary>
            public uint Value;

            /// <summary>
            /// ?
            /// </summary>
            public uint Size;

            /// <summary>
            /// ?
            /// </summary>
            public TypeEnum Type;

            /// <summary>
            /// ?
            /// </summary>
            public BindEnum Bind;

            /// <summary>
            /// ?
            /// </summary>
            public byte Other;

            /// <summary>
            /// ?
            /// </summary>
            public ushort Index;
        }
    }
}