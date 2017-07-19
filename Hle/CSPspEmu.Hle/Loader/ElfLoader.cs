//#define DEBUG_ELF_LOADER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSPspEmu.Hle.Formats;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Loader
{
    public unsafe class ElfLoader
    {
        public Stream FileStream;
        public Stream MemoryStream;
        public MemoryPartition MemoryPartition;
        protected uint BaseAddress;

        public Elf.HeaderStruct Header;
        public Elf.SectionHeader[] SectionHeaders;
        public Elf.ProgramHeader[] ProgramHeaders;
        public Elf.SectionHeader NamesSectionHeader;
        public Dictionary<string, Elf.SectionHeader> SectionHeadersByName { get; protected set; }
        protected byte[] StringTable;

        public string GetStringFromStringTable(uint index)
        {
            fixed (byte* namePointer = &StringTable[index])
                return PointerUtils.PtrToString(namePointer, Encoding.ASCII);
        }

        public virtual void Load(Stream fileStream, string name)
        {
            fileStream = new MemoryStream(fileStream.ReadAll());

            FileStream = fileStream;

            Header = fileStream.ReadStruct<Elf.HeaderStruct>();
            if (Header.Magic != Elf.HeaderStruct.MagicEnum.ExpectedValue)
                throw new InvalidProgramException($"Not an ELF File \'{name}\'");

            if (Header.Machine != Elf.HeaderStruct.MachineEnum.ALLEGREX)
                throw new InvalidProgramException("Invalid Elf.Header.Machine");

            ProgramHeaders = fileStream.ReadStructVectorAt<Elf.ProgramHeader>(Header.ProgramHeaderOffset,
                Header.ProgramHeaderCount, Header.ProgramHeaderEntrySize);
            SectionHeaders = fileStream.ReadStructVectorAt<Elf.SectionHeader>(Header.SectionHeaderOffset,
                Header.SectionHeaderCount, Header.SectionHeaderEntrySize);

            NamesSectionHeader = SectionHeaders[Header.SectionHeaderStringTable];
            StringTable = fileStream.SliceWithLength(NamesSectionHeader.Offset, NamesSectionHeader.Size)
                .ReadAll();

            SectionHeadersByName = new Dictionary<string, Elf.SectionHeader>();
            foreach (var sectionHeader in SectionHeaders)
            {
                var sectionHeaderName = GetStringFromStringTable(sectionHeader.Name);
                SectionHeadersByName[sectionHeaderName] = sectionHeader;
            }

            Console.WriteLine("ProgramHeaders:{0}", ProgramHeaders.Length);
            foreach (var programHeader in ProgramHeaders)
            {
                Console.WriteLine("{0}", programHeader.ToStringDefault());
            }

            Console.WriteLine("SectionHeaders:{0}", SectionHeaders.Length);
            foreach (var sectionHeader in SectionHeaders)
            {
                Console.WriteLine("{0}:{1}", GetStringFromStringTable(sectionHeader.Name),
                    sectionHeader.ToStringDefault());
            }

            if (NeedsRelocation && ProgramHeaders.Length > 1)
            {
                //throw (new NotImplementedException("Not implemented several ProgramHeaders yet using relocation"));
            }
        }

        public void AllocateAndWrite(Stream memoryStream, MemoryPartition memoryPartition, uint baseAddress = 0)
        {
            MemoryStream = memoryStream;
            MemoryPartition = memoryPartition;
            BaseAddress = baseAddress;

            AllocateMemory();
            WriteToMemory();

            //((PspMemoryStream)MemoryStream).Memory.Dump("after_allocate_and_write_dump.bin");
        }

        protected void AllocateMemory()
        {
#if true
            var lowest = 0xFFFFFFFFU;
            var highest = 0U;
            foreach (var sectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
            {
                lowest = Math.Min(lowest, BaseAddress + sectionHeader.Address);
                highest = Math.Max(highest, (uint) (BaseAddress + sectionHeader.Address + sectionHeader.Size));
            }
            foreach (var programHeader in ProgramHeaders)
            {
                lowest = Math.Min(lowest, BaseAddress + programHeader.VirtualAddress);
                highest = Math.Max(highest,
                    BaseAddress + programHeader.VirtualAddress + programHeader.MemorySize);
            }

            MemoryPartition.AllocateLowHigh(lowest, highest, Name: "Elf");
#else
			foreach (var SectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
			{
				MemoryPartition.AllocateLowSize(BaseAddress + SectionHeader.Address, SectionHeader.Size);
			}
#endif
        }

        protected void WriteToMemory()
        {
            foreach (var sectionHeader in SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet.Allocate))
            {
                var sectionHeaderFileStream = FileStream.SliceWithLength(sectionHeader.Offset, sectionHeader.Size);
                var sectionHeaderMemoryStream =
                    MemoryStream.SliceWithLength(sectionHeader.Address + BaseAddress, sectionHeader.Size);

                Console.WriteLine("WriteToMemory('{0:X}') : 0x{1:X} : {2} : {3}",
                    GetStringFromStringTable(sectionHeader.Name), sectionHeader.Address, sectionHeader.Type,
                    sectionHeader.Size);
                Console.WriteLine("   0x{0:X} - 0x{1:X}", sectionHeader.Address + BaseAddress, sectionHeader.Size);

                switch (sectionHeader.Type)
                {
                    case Elf.SectionHeader.TypeEnum.ProgramBits:
                        //Console.WriteLine(SectionHeaderFileStream.ReadAll().ToHexString());
                        sectionHeaderMemoryStream.WriteStream(sectionHeaderFileStream);
                        break;
                    case Elf.SectionHeader.TypeEnum.NoBits:
                        sectionHeaderMemoryStream.WriteByteRepeated(0, sectionHeader.Size);
                        break;
                    default:
                        break;
                }
            }
        }

        public Stream ProgramHeaderFileStream(Elf.ProgramHeader programHeader) => FileStream.SliceWithLength(programHeader.Offset, programHeader.FileSize);

        public Stream SectionHeaderFileStream(Elf.SectionHeader sectionHeader) => FileStream.SliceWithLength(sectionHeader.Offset, sectionHeader.Size);

        public Stream SectionHeaderMemoryStream(Elf.SectionHeader sectionHeader) => MemoryStream.SliceWithLength(BaseAddress + sectionHeader.Address, sectionHeader.Size);

        protected IEnumerable<Elf.SectionHeader> SectionHeadersWithFlag(Elf.SectionHeader.FlagsSet flag) => SectionHeaders.Where(sectionHeader => sectionHeader.Flags.HasFlag(flag));

        public bool IsPrx => Header.Type.HasFlag(Elf.HeaderStruct.TypeEnum.Prx);

        public bool NeedsRelocation => IsPrx || (Header.EntryPoint < PspMemory.MainOffset);
    }
}