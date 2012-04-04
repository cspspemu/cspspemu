using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CSPspEmu.Hle.Formats;
using CSharpUtils.Streams;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Loader
{
	unsafe public class ElfLoader
	{
		public Stream FileStream;
		public Stream MemoryStream;
		public MemoryPartition MemoryPartition;
		protected uint BaseAddress = 0;

		public Elf.HeaderStruct Header;
		public Elf.SectionHeader[] SectionHeaders;
		public Elf.ProgramHeader[] ProgramHeaders;
		public Elf.SectionHeader NamesSectionHeader;
		public Dictionary<string, Elf.SectionHeader> SectionHeadersByName { get; protected set; }
		protected byte[] StringTable;

		public String GetStringFromStringTable(uint Index)
		{
			fixed (byte* NamePointer = &StringTable[Index])
			{
				return PointerUtils.PtrToString(NamePointer, Encoding.ASCII);
			}
		}

		virtual public void Load(Stream FileStream, string Name)
		{
			FileStream = new MemoryStream(FileStream.ReadAll());

			this.FileStream = FileStream;

			this.Header = FileStream.ReadStruct<Elf.HeaderStruct>();
			if (this.Header.Magic != Elf.HeaderStruct.MagicEnum.ExpectedValue)
			{
				throw(new InvalidProgramException("Not an ELF File '" + Name + "'"));
			}

			if (this.Header.Machine != Elf.HeaderStruct.MachineEnum.ALLEGREX)
			{
				throw (new InvalidProgramException("Invalid Elf.Header.Machine"));
			}

			this.ProgramHeaders = FileStream.ReadStructVectorAt<Elf.ProgramHeader>(Header.ProgramHeaderOffset, Header.ProgramHeaderCount, Header.ProgramHeaderEntrySize);
			this.SectionHeaders = FileStream.ReadStructVectorAt<Elf.SectionHeader>(Header.SectionHeaderOffset, Header.SectionHeaderCount, Header.SectionHeaderEntrySize);

			this.NamesSectionHeader = this.SectionHeaders[Header.SectionHeaderStringTable];
			this.StringTable = FileStream.SliceWithLength(this.NamesSectionHeader.Offset, this.NamesSectionHeader.Size).ReadAll();

			this.SectionHeadersByName = new Dictionary<string, Elf.SectionHeader>();
			foreach (var SectionHeader in this.SectionHeaders)
			{
				var SectionHeaderName = GetStringFromStringTable(SectionHeader.Name);
				this.SectionHeadersByName[SectionHeaderName] = SectionHeader;
			}

			Console.WriteLine("ProgramHeaders:{0}", this.ProgramHeaders.Length);
			foreach (var ProgramHeader in ProgramHeaders)
			{
				Console.WriteLine("{0}", ProgramHeader.ToStringDefault());
			}

			Console.WriteLine("SectionHeaders:{0}", this.SectionHeaders.Length);
			foreach (var SectionHeader in SectionHeaders)
			{
				Console.WriteLine("{0}:{1}", GetStringFromStringTable(SectionHeader.Name), SectionHeader.ToStringDefault());
			}

			if (NeedsRelocation && this.ProgramHeaders.Length > 1)
			{
				//throw (new NotImplementedException("Not implemented several ProgramHeaders yet using relocation"));
			}
		}

		public void AllocateAndWrite(Stream MemoryStream, MemoryPartition MemoryPartition, uint BaseAddress = 0)
		{
			this.MemoryStream = MemoryStream;
			this.MemoryPartition = MemoryPartition;
			this.BaseAddress = BaseAddress;

			AllocateMemory();
			WriteToMemory();

#if DEBUG
			((PspMemoryStream)MemoryStream).Memory.Dump("after_allocate_and_write_dump.bin");
#endif
		}

		protected void AllocateMemory()
		{
#if true
			uint Lowest = 0xFFFFFFFF;
			uint Highest = 0;
			foreach (var SectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
			{
				/*
				Console.WriteLine(
					"AllocateLowSize:(0x{0:X}:0x{1:X}) : 0x{2:X} : {3}",
					SectionHeader.Address,
					SectionHeader.Address + BaseAddress,
					SectionHeader.Size,
					SectionHeader
				);
				*/
				Lowest = Math.Min(Lowest, (uint)(BaseAddress + SectionHeader.Address));
				Highest = Math.Max(Highest, (uint)(BaseAddress + SectionHeader.Address + SectionHeader.Size));
			}
			foreach (var ProgramHeader in ProgramHeaders)
			{
				Lowest = Math.Min(Lowest, (uint)(BaseAddress + ProgramHeader.VirtualAddress));
				Highest = Math.Max(Highest, (uint)(BaseAddress + ProgramHeader.VirtualAddress + ProgramHeader.MemorySize));
			}

			MemoryPartition.AllocateLowHigh(Lowest, Highest, Name: "Elf");
#else
			foreach (var SectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
			{
				MemoryPartition.AllocateLowSize(BaseAddress + SectionHeader.Address, SectionHeader.Size);
			}
#endif
		}

		protected void WriteToMemory()
		{
			foreach (var SectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
			{
				var SectionHeaderFileStream = FileStream.SliceWithLength(SectionHeader.Offset, SectionHeader.Size);
				var SectionHeaderMemoryStream = MemoryStream.SliceWithLength(SectionHeader.Address + BaseAddress, SectionHeader.Size);

				Console.WriteLine("WriteToMemory('{0:X}') : 0x{1:X} : {2} : {3}", GetStringFromStringTable(SectionHeader.Name), SectionHeader.Address, SectionHeader.Type, SectionHeader.Size);

				switch (SectionHeader.Type)
				{
					case Elf.SectionHeader.TypeEnum.ProgramBits:
						//Console.WriteLine(SectionHeaderFileStream.ReadAll().ToHexString());
						SectionHeaderMemoryStream.WriteStream(SectionHeaderFileStream);
						break;
					case Elf.SectionHeader.TypeEnum.NoBits:
						SectionHeaderMemoryStream.WriteByteRepeated(0, SectionHeader.Size);
						break;
					default:
						break;
				}
			}
		}

		public Stream ProgramHeaderFileStream(Elf.ProgramHeader ProgramHeader)
		{
			return this.FileStream.SliceWithLength(ProgramHeader.Offset, ProgramHeader.FileSize);
		}

		public Stream SectionHeaderFileStream(Elf.SectionHeader SectionHeader)
		{
			return this.FileStream.SliceWithLength(SectionHeader.Offset, SectionHeader.Size);
		}

		public Stream SectionHeaderMemoryStream(Elf.SectionHeader SectionHeader)
		{
			return this.MemoryStream.SliceWithLength(SectionHeader.Offset, SectionHeader.Size);
		}

		protected IEnumerable<Elf.SectionHeader> SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet Flag)
		{
			return SectionHeaders.Where(SectionHeader => SectionHeader.Flags.HasFlag(Flag));
		}

		public bool IsPrx
		{
			get
			{
				return Header.Type.HasFlag(Elf.HeaderStruct.TypeEnum.Prx);
			}
		}

		public bool NeedsRelocation
		{
			get
			{
				return IsPrx || (Header.EntryPoint < PspMemory.MainOffset);
				//return SectionHeaders.Any(SectionHeader => SectionHeader.Type == Elf.SectionHeader.TypeEnum.PRXRELOC);
			}
		}
	}
}
