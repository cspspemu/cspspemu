using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Loader
{
	unsafe public class Elf
	{
		public struct Header
		{
			public enum TypeEnum    : ushort { Executable = 0x0002, Prx = 0xFFA0 }
			public enum MachineEnum : ushort { ALLEGREX = 8 }

			// e_ident 16 bytes.
			public fixed byte Magic[4];                    ///  = [0x7F, 'E', 'L', 'F']
			public byte Class;                             ///
			public byte Data;                              ///
			public byte IdVersion;                         ///
			public fixed byte Padding[9];                  /// Padding.

			public TypeEnum Type;                          /// Identifies object file type
			public MachineEnum Machine;                    /// Architecture build  = Machine.ALLEGREX
			public uint Version;                           /// Object file version
			public uint EntryPoint;                        /// Virtual address of code entry. Module EntryPoint (PC)
			public uint ProgramHeaderOffset;               /// Program header table's file offset in bytes
			public uint SectionHeaderOffset;               /// Section header table's file offset in bytes
			public uint Flags;                             /// Processor specific flags
			public ushort ElfHeaderSize;                   /// ELF header size in bytes

			// Program Header.
			public ushort ProgramHeaderEntrySize;          /// Program header size (all the same size)
			public ushort ProgramHeaderCount;              /// Number of program headers

			// Section Header.
			public ushort SectionHeaderEntrySize;          /// Section header size (all the same size)
			public ushort SectionHeaderCount;              /// Number of section headers
			public ushort SectionHeaderStringTable;        /// Section header table index of the entry associated with the section name string table
		}

		public struct ProgramHeader
		{
			public enum TypeEnum : uint { NoLoad = 0, Load = 1 }

			public TypeEnum Type;        /// Type of segment
			public uint Offset;          /// Offset for segment's first byte in file
			public uint VirtualAddress;  /// Virtual address for segment
			public uint PsysicalAddress; /// Physical address for segment
			public uint FileSize;        /// Segment image size in file
			public uint MemorySize;      /// Segment image size in memory
			public uint Flags;           /// Flags
			public uint Alignment;       /// Alignment
		}

		public struct SectionHeader { // ELF Section Header
			public enum TypeEnum : uint {
				NULL = 0, PROGBITS = 1, SYMTAB = 2, STRTAB = 3, RELA = 4, HASH = 5, DYNAMIC = 6, NOTE = 7, NOBITS = 8, REL = 9, SHLIB = 10, DYNSYM = 11,

				LOPROC = 0x70000000, HIPROC = 0x7FFFFFFF,
				LOUSER = 0x80000000, HIUSER = 0xFFFFFFFF,

				PRXRELOC = (LOPROC | 0xA0),
			}

			public enum FlagsEnum : uint { None = 0, Write = 1, Allocate = 2, Execute = 4 }

			public uint Name;          /// Position relative to .shstrtab of a stringz with the name.
			public TypeEnum Type;      /// Type of this section header.
			public FlagsEnum Flags;    /// Flags associated to this section header.
			public uint Address;       /// Memory address where it should be stored.
			public uint Offset;        /// File position where is the data related to this section header.
			public uint Size;          /// Size of the section header.
			public uint Link;          ///
			public uint Info;          ///
			public uint AddressAlign;  ///
			public uint EntitySize;    ///
		}

		public enum ModuleNids : uint {
			MODULE_INFO                   = 0xF01D73A7,
			MODULE_BOOTSTART              = 0xD3744BE0,
			MODULE_REBOOT_BEFORE          = 0x2F064FA6,
			MODULE_START                  = 0xD632ACDB,
			MODULE_START_THREAD_PARAMETER = 0x0F7C276C,
			MODULE_STOP                   = 0xCEE8593C,
			MODULE_STOP_THREAD_PARAMETER  = 0xCF0CC697,
		}
	}

	public class ElfLoader
	{
	}
}
