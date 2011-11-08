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

namespace CSPspEmu.Hle.Loader
{
	unsafe abstract public class ElfLoader
	{
		public Stream FileStream;
		public Elf.Header Header;
		public Elf.SectionHeader[] SectionHeaders;
		public Elf.ProgramHeader[] ProgramHeaders;
		public Elf.SectionHeader NamesSectionHeader;
		public Dictionary<string, Elf.SectionHeader> SectionHeadersByName;
		public byte[] Names;

		public String NameAt(uint Index)
		{
			fixed (byte* NamePointer = &Names[Index])
			{
				return PointerUtils.PtrToString(NamePointer, Encoding.ASCII);
			}
		}

		public void LoadAllocateAndWrite(Stream FileStream, Stream MemoryStream, MemoryPartition MemoryPartition)
		{
			Load(FileStream);
			AllocateMemory(MemoryPartition);
			WriteToMemory(MemoryStream);
		}

		virtual public void Load(Stream FileStream)
		{
			this.FileStream = FileStream;
			this.Header = FileStream.ReadStruct<Elf.Header>();
			if (this.Header.Magic != Elf.Header.ExpectedMagic)
			{
				throw(new InvalidProgramException("Not an ELF File"));
			}

			if (this.Header.Machine != Elf.Header.MachineEnum.ALLEGREX)
			{
				throw (new InvalidProgramException("Invalid Elf.Header.Machine"));
			}

			this.ProgramHeaders = FileStream.ReadStructVectorAt<Elf.ProgramHeader>(Header.ProgramHeaderOffset, Header.ProgramHeaderCount, Header.ProgramHeaderEntrySize);
			this.SectionHeaders = FileStream.ReadStructVectorAt<Elf.SectionHeader>(Header.SectionHeaderOffset, Header.SectionHeaderCount, Header.SectionHeaderEntrySize);

			this.NamesSectionHeader = this.SectionHeaders[Header.SectionHeaderStringTable];
			this.Names = FileStream.SliceWithLength(this.NamesSectionHeader.Offset, this.NamesSectionHeader.Size).ReadAll();

			this.SectionHeadersByName = new Dictionary<string, Elf.SectionHeader>();
			foreach (var SectionHeader in this.SectionHeaders)
			{
				var SectionHeaderName = NameAt(SectionHeader.Name);
				this.SectionHeadersByName[SectionHeaderName] = SectionHeader;
			}
		}

		public Stream SliceStreamForSectionHeader(Elf.SectionHeader SectionHeader)
		{
			return this.FileStream.SliceWithLength(SectionHeader.Offset, SectionHeader.Size);
		}

		public IEnumerable<Elf.SectionHeader> SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet Flag)
		{
			return SectionHeaders.Where(SectionHeader => SectionHeader.Flags.HasFlag(Flag));
		}

		virtual public void AllocateMemory(MemoryPartition MemoryPartition)
		{
			foreach (var SectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
			{
				//Console.WriteLine("Address:{0:X}", SectionHeader.Address);
				MemoryPartition.AllocateLowSize(SectionHeader.Address, SectionHeader.Size);
			}
		}

		virtual public void WriteToMemory(Stream MemoryStream)
		{
			foreach (var SectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
			{
				var SectionHeaderFileStream = FileStream.SliceWithLength(SectionHeader.Offset, SectionHeader.Size);
				var SectionHeaderMemoryStream = MemoryStream.SliceWithLength(SectionHeader.Address, SectionHeader.Size);

				Console.WriteLine("WriteToMemory('{0:X}') : 0x{1:X} : {2}", NameAt(SectionHeader.Name), SectionHeader.Address, SectionHeader.Type);

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
	}
}
