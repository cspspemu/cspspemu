using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Cpu;
using CSharpUtils;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Loader
{
    public struct InitInfoStruct
    {
        public uint Pc;
        public uint Gp;
    }

    public class ElfPspLoader
    {
        static Logger _logger = Logger.GetLogger("Loader");

        [Inject] protected ElfLoader ElfLoader;

        [Inject] protected HleModuleManager ModuleManager;

        [Inject] protected ElfConfig ElfConfig;

        [Inject] protected InjectContext InjectContext;

        private ElfPspLoader()
        {
        }

        protected uint BaseAddress;

        protected HleModuleGuest HleModuleGuest;

        Stream _relocOutputStream;
        StreamWriter _relocOutput;

        StreamWriter RelocOutput
        {
            get
            {
                if (_relocOutput == null)
                {
//#if !DEBUG
#if true
                    //_RelocOutput = new StreamWriter(_RelocOutputStream = new MemoryStream());
                    _relocOutput = null;
                    _relocOutputStream = null;
#else
					_RelocOutputStream = File.Open("reloc.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
					_RelocOutput = new StreamWriter(_RelocOutputStream);
					_RelocOutput.AutoFlush = true;
#endif
                }
                return _relocOutput;
            }
        }

        public HleModuleGuest LoadModule(Stream fileStream, Stream memoryStream, MemoryPartition memoryPartition,
            HleModuleManager moduleManager, string gameTitle, string moduleName, bool isMainModule)
        {
            HleModuleGuest = InjectContext.NewInstance<HleModuleGuest>();

            ElfLoader = new ElfLoader();
            ModuleManager = moduleManager;

            var magic = fileStream.SliceWithLength(0, 4).ReadString(4);
            _logger.Info("Magic: '{0}'", magic);
            if (magic == "~PSP")
            {
                try
                {
                    var decryptedData = new EncryptedPrx().Decrypt(fileStream.ReadAll(), true);
                    File.WriteAllBytes("last_decoded_prx.bin", decryptedData);
                    fileStream = new MemoryStream(decryptedData);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception);
                    throw;
                }
            }

            ElfLoader.Load(fileStream, moduleName);

            ElfConfig.InfoExeHasRelocation = ElfLoader.NeedsRelocation;

            if (ElfLoader.NeedsRelocation)
            {
                var dummyPartition = memoryPartition.Allocate(0x4000, Name: "Dummy");
                BaseAddress = memoryPartition.ChildPartitions.OrderByDescending(partition => partition.Size).First()
                    .Low;
                _logger.Info("BASE ADDRESS (Try    ): 0x{0:X}", BaseAddress);
                BaseAddress = MathUtils.NextAligned(BaseAddress, 0x1000);
                _logger.Info("BASE ADDRESS (Aligned): 0x{0:X}", BaseAddress);
            }
            else
            {
                BaseAddress = 0;
            }

            ElfConfig.RelocatedBaseAddress = BaseAddress;
            ElfConfig.GameTitle = gameTitle;

            ElfLoader.AllocateAndWrite(memoryStream, memoryPartition, BaseAddress);

            LoadModuleInfo();

            if (ElfLoader.NeedsRelocation)
            {
                RelocateFromHeaders();
            }

            LoadModuleInfo();

            //Console.WriteLine(this.ModuleInfo.ToStringDefault());

            HleModuleGuest.InitInfo = new InitInfoStruct()
            {
                Pc = ElfLoader.Header.EntryPoint + BaseAddress,
                Gp = HleModuleGuest.ModuleInfo.GP,
            };

            UpdateModuleImports();
            UpdateModuleExports();

            moduleManager.LoadedGuestModules.Add(HleModuleGuest);

            return HleModuleGuest;
        }

        protected void LoadModuleInfo()
        {
            var sectionHeaderName = ".rodata.sceModuleInfo";
            var programHeader = ElfLoader.ProgramHeaders.FirstOrDefault();
            Stream stream;
            if (ElfLoader.SectionHeadersByName.ContainsKey(sectionHeaderName))
            {
                var sectionHeader = ElfLoader.SectionHeadersByName[".rodata.sceModuleInfo"];
                stream = ElfLoader.SectionHeaderMemoryStream(sectionHeader);
                Console.WriteLine("LoadModuleInfo: .rodata.sceModuleInfo 0x{0:X8}[{1}]",
                    BaseAddress + sectionHeader.Address, sectionHeader.Size);
            }
            else
            {
                var moduleInfoAddress =
                    (uint) (BaseAddress + (programHeader.PsysicalAddress & 0x7FFFFFFFL) - programHeader.Offset);
                var size = Marshal.SizeOf(typeof(ElfPsp.ModuleInfo));
                stream = ElfLoader.MemoryStream.SliceWithLength(moduleInfoAddress, size);
                Console.WriteLine("LoadModuleInfo: 0x{0:X8}[{1}]", moduleInfoAddress, size);
            }

            HleModuleGuest.ModuleInfo = stream.ReadStruct<ElfPsp.ModuleInfo>();
            Console.WriteLine("{0}", HleModuleGuest.ModuleInfo.ToStringDefault());
        }

        protected void RelocateFromHeaders()
        {
            if ((BaseAddress & 0xFFFF) != 0)
            {
                //throw(new NotImplementedException("Can't relocate with the BaseAddress.LO16 != 0"));
            }

            // Relocate from program headers
            var relocProgramIndex = 0;
            foreach (var programHeader in ElfLoader.ProgramHeaders)
            {
                if (RelocOutput != null) RelocOutput.WriteLine("Program Header: %d".Sprintf(relocProgramIndex++));
                switch (programHeader.Type)
                {
                    case Elf.ProgramHeader.TypeEnum.Reloc1:
                        _logger.Warning("SKIPPING Elf.ProgramHeader.TypeEnum.Reloc1!");
                        break;
                    case Elf.ProgramHeader.TypeEnum.Reloc2:
                        throw(new NotImplementedException());
                }
            }

            var relocSectionIndex = 0;
            foreach (var sectionHeader in ElfLoader.SectionHeaders)
            {
                //RelocOutput.WriteLine("Section Header: %d : %s".Sprintf(RelocSectionIndex++, SectionHeader.ToString()));
                RelocOutput?.WriteLine("Section Header: %d".Sprintf(relocSectionIndex++));

                switch (sectionHeader.Type)
                {
                    case Elf.SectionHeader.TypeEnum.Relocation:
                        Console.Error.WriteLine("Not implemented Elf.SectionHeader.TypeEnum.Relocation");
                        //throw (new NotImplementedException("Not implemented Elf.SectionHeader.TypeEnum.Relocation"));
                        //break;
                        /*
                        RelocateRelocs(
                            ElfLoader.SectionHeaderFileStream(SectionHeader).ReadStructVectorUntilTheEndOfStream<Elf.Reloc>()
                        );
                        */
                        break;

                    case Elf.SectionHeader.TypeEnum.PrxRelocation:
                        Console.WriteLine("PrxRelocation : {0}", sectionHeader);
                        RelocateRelocs(
                            ElfLoader.SectionHeaderFileStream(sectionHeader)
                                .ReadStructVectorUntilTheEndOfStream<Elf.Reloc>()
                        );
                        break;
                    case Elf.SectionHeader.TypeEnum.PrxRelocation_FW5:
                        throw (new Exception("Not implemented ElfSectionHeader.Type.PrxRelocation_FW5"));
                }
            }

            if (RelocOutput != null)
            {
                RelocOutput.Flush();
                _relocOutputStream.Flush();
                RelocOutput.Close();
                _relocOutputStream.Close();

                _relocOutput = null;
                _relocOutputStream = null;
            }
        }

        /// <summary>
        /// This function relocates all the instructions and pointers of the loading executable.
        /// </summary>
        /// <param name="relocs"></param>
        protected void RelocateRelocs(IEnumerable<Elf.Reloc> relocs)
        {
            var instructionReader = new InstructionStreamReader(ElfLoader.MemoryStream);

            /*
            Func<uint, Action<ref Instruction>> UpdateInstruction = (Address) =>
            {
            };
            */

            //var Hi16List = new List<uint>();

            ushort hiValue = 0;
            var deferredHi16 =
                new LinkedList<uint>(); // We'll use this to relocate R_MIPS_HI16 when we get a R_MIPS_LO16

            var index = 0;
            foreach (var reloc in relocs)
            {
                //Console.WriteLine(Reloc.ToStringDefault());
                //Console.WriteLine("   {0:X}", RelocatedAddress);

                // Check if R_TYPE is 0xFF (break code) and break the loop
                // immediately in order to avoid fetching non existent program headers.

                // Some games (e.g.: "Final Fantasy: Dissidia") use this kind of relocation
                // suggesting that the PSP's ELF Loader is capable of recognizing it and stop.
                if (reloc.Type == Elf.Reloc.TypeEnum.StopRelocation)
                {
                    break;
                }

                var pointerBaseOffset = ElfLoader.ProgramHeaders[reloc.PointerSectionHeaderBase].VirtualAddress;
                var pointeeBaseOffset = ElfLoader.ProgramHeaders[reloc.PointeeSectionHeaderBase].VirtualAddress;

                // Address of data to relocate
                var relocatedPointerAddress = BaseAddress + reloc.PointerAddress + pointerBaseOffset;

                // Value of data to relocate
                var instruction = instructionReader[relocatedPointerAddress];
                var instructionBefore = instruction;

                var s = BaseAddress + pointeeBaseOffset;
                var gpAddr = (int) (BaseAddress + reloc.PointerAddress);
                var gpOffset = gpAddr - ((int) BaseAddress & 0xFFFF0000);

                //Console.WriteLine(Reloc.Type);

                var debugReloc = (relocatedPointerAddress >= 0x08809320 &&
                                  relocatedPointerAddress <= 0x08809320 + 0x100);
                //bool DebugReloc = false;

                if (debugReloc)
                {
                    Console.WriteLine("{0:X8}[{1:X8}]: {2}", relocatedPointerAddress, instruction.Value, reloc);
                }

                switch (reloc.Type)
                {
                    // Tested on PSP: R_MIPS_NONE just returns 0.
                    case Elf.Reloc.TypeEnum.None: // 0
                    {
                    }
                        break;
                    /*
                case Elf.Reloc.TypeEnum.Mips16: // 1
                    {
                        Instruction.IMMU += S;
                    }
                    break;
                    */
                    case Elf.Reloc.TypeEnum.Mips32: // 2
                    {
                        instruction.Value += s;
                    }
                        break;
                    case Elf.Reloc.TypeEnum.MipsRel32: // 3;
                    {
                        throw new NotImplementedException();
                    }
                    case Elf.Reloc.TypeEnum.Mips26: // 4
                    {
                        instruction.JumpReal = instruction.JumpReal + s;
                    }
                        break;
                    case Elf.Reloc.TypeEnum.MipsHi16: // 5
                    {
                        hiValue = (ushort) instruction.Immu;
                        deferredHi16.AddLast(relocatedPointerAddress);
                    }
                        break;
                    case Elf.Reloc.TypeEnum.MipsLo16: // 6
                    {
                        var a = instruction.Immu;

                        instruction.Immu = ((uint) (hiValue << 16) | a & 0x0000FFFF) + s;

                        // Process deferred R_MIPS_HI16
                        foreach (var dataAddr2 in deferredHi16)
                        {
                            var data2 = instructionReader[dataAddr2];
                            var result = ((data2.Value & 0x0000FFFF) << 16) + a + s;
                            // The low order 16 bits are always treated as a signed
                            // value. Therefore, a negative value in the low order bits
                            // requires an adjustment in the high order bits. We need
                            // to make this adjustment in two ways: once for the bits we
                            // took from the data, and once for the bits we are putting
                            // back in to the data.
                            if ((a & 0x8000) != 0)
                            {
                                result -= 0x10000;
                            }
                            if ((result & 0x8000) != 0)
                            {
                                result += 0x10000;
                            }
                            data2.Immu = (result >> 16);
                            instructionReader[dataAddr2] = data2;
                        }
                        deferredHi16.Clear();
                    }
                        break;
                    case Elf.Reloc.TypeEnum.MipsGpRel16: // 7
                    {
                        /*
                        int A = Instruction.IMM;
                        int result;
                        if (A == 0)
                        {
                            result = (int)S - (int)GP_ADDR;
                        }
                        else
                        {
                            result = (int)S + (int)GP_OFFSET + (int)(((A & 0x00008000) != 0) ? (((A & 0x00003FFF) + 0x4000) | 0xFFFF0000) : A) - (int)GP_ADDR;
                        }
                        if ((result < -32768) || (result > 32768))
                        {
                            Console.Error.WriteLine("Relocation overflow (R_MIPS_GPREL16) : '" + result + "'");
                        }
                        Instruction.IMMU = (uint)result;
                        */
                    }
                        break;
                    default:
                        throw new NotImplementedException($"Handling {reloc.Type} not implemented");
                }

                RelocOutput?.WriteLine(
                    "RELOC %06d : 0x%08X : 0x%08X -> 0x%08X".Sprintf(
                        index,
                        relocatedPointerAddress, instructionBefore.Value, instruction.Value
                    )
                );

                if (debugReloc)
                {
                    Console.WriteLine("   -> {0:X8}", instruction.Value);
                }

                /*
                log.error(String.format(
                    "RELOC %06d : 0x%08X : 0x%08X -> 0x%08X\n",
                    i, data_addr, data_prev, data
                ));
                */
                instructionReader[relocatedPointerAddress] = instruction;
                index++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void UpdateModuleImports()
        {
            ConsoleUtils.SaveRestoreConsoleState(() =>
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                _UpdateModuleImports();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        protected void UpdateModuleExports()
        {
            ConsoleUtils.SaveRestoreConsoleState(() =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                _UpdateModuleExports();
            });
        }

        /// <summary>
        /// The export section is organized as as sequence of:
        /// - 32-bit NID * functionCount
        /// - 32-bit NID * variableCount
        /// - 32-bit export address * functionCount
        /// - 32-bit variable address * variableCount
        ///   (each variable address references another structure, depending on its NID)
        /// </summary>
        private void _UpdateModuleExports()
        {
            //var BaseMemoryStream = ElfLoader.MemoryStream.SliceWithLength(BaseAddress);
            var exportsStream = ElfLoader.MemoryStream.SliceWithBounds(HleModuleGuest.ModuleInfo.ExportsStart,
                HleModuleGuest.ModuleInfo.ExportsEnd);
            var moduleExports = exportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleExport>();

            Console.WriteLine("Exports:");

            foreach (var moduleExport in moduleExports)
            {
                var moduleExportName = (moduleExport.Name > 0)
                    ? ElfLoader.MemoryStream.ReadStringzAt(moduleExport.Name)
                    : "";

                Console.WriteLine("  * Export: '{0}'", moduleExportName);

                var hleModuleExports = new HleModuleExports
                {
                    Name = moduleExportName,
                    Flags = moduleExport.Flags,
                    Version = moduleExport.Version
                };

                var exportsExportsStream = ElfLoader.MemoryStream.SliceWithLength(
                    moduleExport.Exports,
                    moduleExport.FunctionCount * sizeof(uint) * 2 + moduleExport.VariableCount * sizeof(uint) * 2
                );

                var functionNidReader =
                    new BinaryReader(exportsExportsStream.ReadStream(moduleExport.FunctionCount * sizeof(uint)));
                var variableNidReader =
                    new BinaryReader(exportsExportsStream.ReadStream(moduleExport.VariableCount * sizeof(uint)));
                var functionAddressReader =
                    new BinaryReader(exportsExportsStream.ReadStream(moduleExport.FunctionCount * sizeof(uint)));
                var variableAddressReader =
                    new BinaryReader(exportsExportsStream.ReadStream(moduleExport.VariableCount * sizeof(uint)));

                for (var n = 0; n < moduleExport.FunctionCount; n++)
                {
                    var nid = functionNidReader.ReadUInt32();
                    var callAddress = functionAddressReader.ReadUInt32();
                    hleModuleExports.Functions[nid] = new HleModuleImportsExports.Entry() {Address = callAddress};

                    Console.WriteLine("  |  - FUNC: {0:X} : {1:X} : {2}", nid, callAddress,
                        Enum.GetName(typeof(SpecialFunctionNids), nid));
                }

                for (var n = 0; n < moduleExport.VariableCount; n++)
                {
                    var nid = variableNidReader.ReadUInt32();
                    var callAddress = variableAddressReader.ReadUInt32();
                    hleModuleExports.Variables[nid] = new HleModuleImportsExports.Entry() {Address = callAddress};

                    Console.WriteLine("  |  - VAR: {0:X} : {1:X} : {2}", nid, callAddress,
                        Enum.GetName(typeof(SpecialVariableNids), nid));
                }

                HleModuleGuest.ModulesExports.Add(hleModuleExports);
            }

            HleModuleGuest.ExportModules();
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SpecialFunctionNids : uint
        {
            ModuleStart = 0xD632ACDB,
            ModuleStop = 0xCEE8593C,
            ModuleRebootBefore = 0x2F064FA6,
            ModuleRebootPhase = 0xADF12745,
            ModuleBootstart = 0xD3744BE0,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SpecialVariableNids : uint
        {
            ModuleInfo = 0xF01D73A7,
            ModuleStartThreadParameter = 0x0F7C276C,
            ModuleStopThreadParameter = 0xCF0CC697,
            ModuleRebootBeforeThreadParameter = 0xF4F4299D,
            ModuleSdkVersion = 0x11B97506,
        }

        /// <summary>
        /// 
        /// </summary>
        private void _UpdateModuleImports()
        {
            //var BaseMemoryStream = ElfLoader.MemoryStream.SliceWithLength(BaseAddress);
            var importsStream = ElfLoader.MemoryStream.SliceWithBounds(HleModuleGuest.ModuleInfo.ImportsStart,
                HleModuleGuest.ModuleInfo.ImportsEnd);
            //Console.WriteLine("ImportsStream.Length: {0}", ImportsStream.Length);
            var moduleImports = importsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();

            Console.WriteLine("BASE ADDRESS: 0x{0:X}", BaseAddress);

            Console.WriteLine("Imports ({0:X8}-{1:X8}):", HleModuleGuest.ModuleInfo.ImportsStart,
                HleModuleGuest.ModuleInfo.ImportsEnd);

            foreach (var moduleImport in moduleImports)
            {
                var moduleImportName = "INVALID";
                try
                {
                    moduleImportName = ElfLoader.MemoryStream.ReadStringzAt(moduleImport.Name);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                var hleModuleImports = new HleModuleImports
                {
                    Name = moduleImportName,
                    Flags = moduleImport.Flags,
                    Version = moduleImport.Version
                };

                var nidStreamReader = new BinaryReader(ElfLoader.MemoryStream.SliceWithLength(moduleImport.NidAddress,
                    moduleImport.FunctionCount * sizeof(uint)));

                for (var n = 0; n < moduleImport.FunctionCount; n++)
                {
                    var nid = nidStreamReader.ReadUInt32();
                    var callAddress = (uint) (moduleImport.CallAddress + n * 8);

                    hleModuleImports.Functions[nid] = new HleModuleImportsExports.Entry() {Address = callAddress};
                }

                HleModuleGuest.ModulesImports.Add(hleModuleImports);
            }

            HleModuleGuest.ImportModules();

            //Console.ReadKey();
        }
    }
}