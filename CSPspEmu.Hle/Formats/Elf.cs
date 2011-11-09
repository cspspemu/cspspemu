using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
	unsafe public class Elf : IFormatDetector
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
				ALLEGREX = 8,
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
			}

			/// <summary>
			/// Type of segment
			/// </summary>
			public TypeEnum Type;

			/// <summary>
			/// Offset for segment's first byte in file
			/// </summary>
			public uint Offset;

			/// <summary>
			/// Virtual address for segment
			/// </summary>
			public uint VirtualAddress;

			/// <summary>
			/// Physical address for segment
			/// </summary>
			public uint PsysicalAddress;

			/// <summary>
			/// Segment image size in file
			/// </summary>
			public uint FileSize;

			/// <summary>
			/// Segment image size in memory
			/// </summary>
			public uint MemorySize;

			/// <summary>
			/// Flags
			/// </summary>
			public uint Flags;

			/// <summary>
			/// Alignment
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
				SYMTAB = 2,
				STRTAB = 3,
				RELA = 4,
				HASH = 5,
				DYNAMIC = 6,
				NOTE = 7,
				NoBits = 8,
				REL = 9,
				SHLIB = 10,
				DYNSYM = 11,

				LOPROC = 0x70000000, HIPROC = 0x7FFFFFFF,
				LOUSER = 0x80000000, HIUSER = 0xFFFFFFFF,

				PRXRELOC = (LOPROC | 0xA0),
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
			public uint Size;

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

		public enum ModuleNids : uint
		{
			MODULE_INFO = 0xF01D73A7,
			MODULE_BOOTSTART = 0xD3744BE0,
			MODULE_REBOOT_BEFORE = 0x2F064FA6,
			MODULE_START = 0xD632ACDB,
			MODULE_START_THREAD_PARAMETER = 0x0F7C276C,
			MODULE_STOP = 0xCEE8593C,
			MODULE_STOP_THREAD_PARAMETER = 0xCF0CC697,
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
			}

			/// <summary>
			/// ?
			/// </summary>
			public uint Offset;

			/// <summary>
			/// ?
			/// </summary>
			public uint Info;

			/// <summary>
			/// ?
			/// </summary>
			public uint SymbolIndex { get { return Info >> 8; } }

			/// <summary>
			/// ?
			/// </summary>
			public TypeEnum Type { get { return (TypeEnum)(Info & 0xFF); } }
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
			public String Name;

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
