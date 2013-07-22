using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Cpu;
using CSharpUtils;
using CSPspEmu.Core;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Loader
{
	public struct InitInfoStruct
	{
		public uint PC;
		public uint GP;
	}

    public class ElfPspLoader
	{
		static Logger Logger = Logger.GetLogger("Loader");

		[Inject]
		protected ElfLoader ElfLoader;

		[Inject]
		protected HleModuleManager ModuleManager;

		[Inject]
		protected ElfConfig ElfConfig;

		[Inject]
		protected InjectContext InjectContext;

		private ElfPspLoader()
		{
		}

		protected uint BaseAddress;
		
		protected HleModuleGuest HleModuleGuest;

		Stream _RelocOutputStream;
		StreamWriter _RelocOutput;

		StreamWriter RelocOutput
		{
			get
			{
				if (_RelocOutput == null)
				{
//#if !DEBUG
#if true
					//_RelocOutput = new StreamWriter(_RelocOutputStream = new MemoryStream());
					_RelocOutput = null;
					_RelocOutputStream = null;
#else
					_RelocOutputStream = File.Open("reloc.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
					_RelocOutput = new StreamWriter(_RelocOutputStream);
					_RelocOutput.AutoFlush = true;
#endif
				}
				return _RelocOutput;
			}
		}

		public HleModuleGuest LoadModule(Stream FileStream, Stream MemoryStream, MemoryPartition MemoryPartition, HleModuleManager ModuleManager, String GameTitle, string ModuleName, bool IsMainModule)
		{
			this.HleModuleGuest = InjectContext.NewInstance<HleModuleGuest>();

			this.ElfLoader = new ElfLoader();
			this.ModuleManager = ModuleManager;

			var Magic = FileStream.SliceWithLength(0, 4).ReadString(4);
			Logger.Info("Magic: '{0}'", Magic);
			if (Magic == "~PSP")
			{
				try
				{
					var DecryptedData = new EncryptedPrx().Decrypt(FileStream.ReadAll(), true);
					File.WriteAllBytes("last_decoded_prx.bin", DecryptedData);
					FileStream = new MemoryStream(DecryptedData);
				}
				catch (Exception Exception)
				{
					Logger.Error(Exception);
					throw;
				}
			}

			this.ElfLoader.Load(FileStream, ModuleName);

			ElfConfig.InfoExeHasRelocation = this.ElfLoader.NeedsRelocation;

			if (this.ElfLoader.NeedsRelocation)
			{
				var DummyPartition = MemoryPartition.Allocate(
					0x4000,
					Name: "Dummy"
				);
				BaseAddress = MemoryPartition.ChildPartitions.OrderByDescending(Partition => Partition.Size).First().Low;
				Logger.Info("BASE ADDRESS (Try    ): 0x{0:X}", BaseAddress);
				BaseAddress = MathUtils.NextAligned(BaseAddress, 0x1000);
				Logger.Info("BASE ADDRESS (Aligned): 0x{0:X}", BaseAddress);
			}
			else
			{
				BaseAddress = 0;
			}

			ElfConfig.RelocatedBaseAddress = BaseAddress;
			ElfConfig.GameTitle = GameTitle;

			this.ElfLoader.AllocateAndWrite(MemoryStream, MemoryPartition, BaseAddress);

			LoadModuleInfo();

			if (this.ElfLoader.NeedsRelocation)
			{
				RelocateFromHeaders();
			}

			LoadModuleInfo();

			//Console.WriteLine(this.ModuleInfo.ToStringDefault());

			HleModuleGuest.InitInfo = new InitInfoStruct()
			{
				PC = ElfLoader.Header.EntryPoint + BaseAddress,
				GP = HleModuleGuest.ModuleInfo.GP,
			};

			UpdateModuleImports();
			UpdateModuleExports();

			ModuleManager.LoadedGuestModules.Add(HleModuleGuest);

			return HleModuleGuest;
		}

		protected void LoadModuleInfo()
		{
			var SectionHeaderName = ".rodata.sceModuleInfo";
			var ProgramHeader = ElfLoader.ProgramHeaders.FirstOrDefault();
			Stream Stream = null;
			if (ElfLoader.SectionHeadersByName.ContainsKey(SectionHeaderName))
			{
				var SectionHeader = ElfLoader.SectionHeadersByName[".rodata.sceModuleInfo"];
				Stream = ElfLoader.SectionHeaderMemoryStream(SectionHeader);
				Console.WriteLine("LoadModuleInfo: .rodata.sceModuleInfo 0x{0:X8}[{1}]", BaseAddress + SectionHeader.Address, SectionHeader.Size);
			}
			else
			{
				uint ModuleInfoAddress = (uint)(BaseAddress + (ProgramHeader.PsysicalAddress & 0x7FFFFFFFL) - ProgramHeader.Offset);
				int Size = Marshal.SizeOf(typeof(ElfPsp.ModuleInfo));
				Stream = ElfLoader.MemoryStream.SliceWithLength(ModuleInfoAddress, Size);
				Console.WriteLine("LoadModuleInfo: 0x{0:X8}[{1}]", ModuleInfoAddress, Size);
			}

			HleModuleGuest.ModuleInfo = Stream.ReadStruct<ElfPsp.ModuleInfo>();
			Console.WriteLine("{0}", HleModuleGuest.ModuleInfo.ToStringDefault());
		}

		protected void RelocateFromHeaders()
		{
			if ((BaseAddress & 0xFFFF) != 0)
			{
				//throw(new NotImplementedException("Can't relocate with the BaseAddress.LO16 != 0"));
			}

			// Relocate from program headers
			int RelocProgramIndex = 0;
			foreach (var ProgramHeader in ElfLoader.ProgramHeaders)
			{
				if (RelocOutput != null) RelocOutput.WriteLine("Program Header: %d".Sprintf(RelocProgramIndex++));
				switch (ProgramHeader.Type)
				{
					case Elf.ProgramHeader.TypeEnum.Reloc1:
						Logger.Warning("SKIPPING Elf.ProgramHeader.TypeEnum.Reloc1!");
						break;
					case Elf.ProgramHeader.TypeEnum.Reloc2:
						throw(new NotImplementedException());
				}
			}

			int RelocSectionIndex = 0;
			foreach (var SectionHeader in ElfLoader.SectionHeaders)
			{
				//RelocOutput.WriteLine("Section Header: %d : %s".Sprintf(RelocSectionIndex++, SectionHeader.ToString()));
				if (RelocOutput != null) RelocOutput.WriteLine("Section Header: %d".Sprintf(RelocSectionIndex++));

				switch (SectionHeader.Type)
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
						Console.WriteLine("PrxRelocation : {0}", SectionHeader);
						RelocateRelocs(
							ElfLoader.SectionHeaderFileStream(SectionHeader).ReadStructVectorUntilTheEndOfStream<Elf.Reloc>()
						);
						break;
					case Elf.SectionHeader.TypeEnum.PrxRelocation_FW5:
						throw (new Exception("Not implemented ElfSectionHeader.Type.PrxRelocation_FW5"));
				}
			}

			if (RelocOutput != null)
			{
				RelocOutput.Flush();
				_RelocOutputStream.Flush();
				RelocOutput.Close();
				_RelocOutputStream.Close();
				
				_RelocOutput = null;
				_RelocOutputStream = null;
			}
		}

		/// <summary>
		/// This function relocates all the instructions and pointers of the loading executable.
		/// </summary>
		/// <param name="Relocs"></param>
		protected void RelocateRelocs(IEnumerable<Elf.Reloc> Relocs)
		{
			var InstructionReader = new InstructionStreamReader(ElfLoader.MemoryStream);

			/*
			Func<uint, Action<ref Instruction>> UpdateInstruction = (Address) =>
			{
			};
			*/

			//var Hi16List = new List<uint>();

			ushort HiValue = 0;
			var DeferredHi16 = new LinkedList<uint>(); // We'll use this to relocate R_MIPS_HI16 when we get a R_MIPS_LO16

			int Index = 0;
			foreach (var Reloc in Relocs)
			{
				//Console.WriteLine(Reloc.ToStringDefault());
				//Console.WriteLine("   {0:X}", RelocatedAddress);

				// Check if R_TYPE is 0xFF (break code) and break the loop
				// immediately in order to avoid fetching non existent program headers.

				// Some games (e.g.: "Final Fantasy: Dissidia") use this kind of relocation
				// suggesting that the PSP's ELF Loader is capable of recognizing it and stop.
				if (Reloc.Type == Elf.Reloc.TypeEnum.StopRelocation)
				{
					break;
				}

				var PointerBaseOffset = (uint)ElfLoader.ProgramHeaders[Reloc.PointerSectionHeaderBase].VirtualAddress;
				var PointeeBaseOffset = (uint)ElfLoader.ProgramHeaders[Reloc.PointeeSectionHeaderBase].VirtualAddress;

				// Address of data to relocate
				var RelocatedPointerAddress = (uint)(BaseAddress + Reloc.PointerAddress + PointerBaseOffset);

				// Value of data to relocate
				var Instruction = InstructionReader[RelocatedPointerAddress];
				var InstructionBefore = Instruction;

				var S = (uint)BaseAddress + PointeeBaseOffset;
				var GP_ADDR = (int)(BaseAddress + Reloc.PointerAddress);
				var GP_OFFSET = (int)GP_ADDR - ((int)BaseAddress & 0xFFFF0000);

				//Console.WriteLine(Reloc.Type);

				bool DebugReloc = (RelocatedPointerAddress >= 0x08809320 && RelocatedPointerAddress <= 0x08809320 + 0x100);
				//bool DebugReloc = false;

				if (DebugReloc)
				{
					Console.WriteLine("{0:X8}[{1:X8}]: {2}", RelocatedPointerAddress, Instruction.Value, Reloc);
				}

				switch (Reloc.Type)
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
							Instruction.Value += S;
						}
						break;
					case Elf.Reloc.TypeEnum.MipsRel32: // 3;
						{
							throw (new NotImplementedException());
						}
					case Elf.Reloc.TypeEnum.Mips26: // 4
						{
							Instruction.JUMP_Real = Instruction.JUMP_Real + S;
						}
						break;
					case Elf.Reloc.TypeEnum.MipsHi16: // 5
						{
							HiValue = (ushort)Instruction.IMMU;
							DeferredHi16.AddLast(RelocatedPointerAddress);
						}
						break;
					case Elf.Reloc.TypeEnum.MipsLo16: // 6
						{
							uint A = Instruction.IMMU;

							Instruction.IMMU = ((uint)(HiValue << 16) | (uint)(A & 0x0000FFFF)) + S;

							// Process deferred R_MIPS_HI16
							foreach (var data_addr2 in DeferredHi16)
							{
								var data2 = InstructionReader[data_addr2];
								uint result = ((data2.Value & 0x0000FFFF) << 16) + A + S;
								// The low order 16 bits are always treated as a signed
								// value. Therefore, a negative value in the low order bits
								// requires an adjustment in the high order bits. We need
								// to make this adjustment in two ways: once for the bits we
								// took from the data, and once for the bits we are putting
								// back in to the data.
								if ((A & 0x8000) != 0) {
									result -= 0x10000;
								}
								if ((result & 0x8000) != 0) {
									 result += 0x10000;
								}
								data2.IMMU = (result >> 16);
								InstructionReader[data_addr2] = data2;
							}
							DeferredHi16.Clear();
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
						throw(new NotImplementedException("Handling " + Reloc.Type + " not implemented"));
				}

				if (RelocOutput != null) RelocOutput.WriteLine(
					"RELOC %06d : 0x%08X : 0x%08X -> 0x%08X".Sprintf(
						Index,
						RelocatedPointerAddress, InstructionBefore.Value, Instruction.Value
					)
				);

				if (DebugReloc)
				{
					Console.WriteLine("   -> {0:X8}", Instruction.Value);
				}

				/*
				log.error(String.format(
					"RELOC %06d : 0x%08X : 0x%08X -> 0x%08X\n",
					i, data_addr, data_prev, data
				));
				*/
				InstructionReader[RelocatedPointerAddress] = Instruction;
				Index++;
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
			var ExportsStream = ElfLoader.MemoryStream.SliceWithBounds(HleModuleGuest.ModuleInfo.ExportsStart, HleModuleGuest.ModuleInfo.ExportsEnd);
			var ModuleExports = ExportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleExport>();

			Console.WriteLine("Exports:");

			foreach (var ModuleExport in ModuleExports)
			{
				String ModuleExportName = "";

				try { ModuleExportName = ElfLoader.MemoryStream.ReadStringzAt(ModuleExport.Name); } catch { }

				Console.WriteLine("  * Export: '{0}'", ModuleExportName);

				var HleModuleExports = new HleModuleExports();
				HleModuleExports.Name = ModuleExportName;
				HleModuleExports.Flags = ModuleExport.Flags;
				HleModuleExports.Version = ModuleExport.Version;

				var ExportsExportsStream = ElfLoader.MemoryStream.SliceWithLength(
					ModuleExport.Exports,
					ModuleExport.FunctionCount * sizeof(uint) * 2 + ModuleExport.VariableCount * sizeof(uint) * 2
				);

				var FunctionNIDReader = new BinaryReader(ExportsExportsStream.ReadStream(ModuleExport.FunctionCount * sizeof(uint)));
				var VariableNIDReader = new BinaryReader(ExportsExportsStream.ReadStream(ModuleExport.VariableCount * sizeof(uint)));
				var FunctionAddressReader = new BinaryReader(ExportsExportsStream.ReadStream(ModuleExport.FunctionCount * sizeof(uint)));
				var VariableAddressReader = new BinaryReader(ExportsExportsStream.ReadStream(ModuleExport.VariableCount * sizeof(uint)));

				for (int n = 0; n < ModuleExport.FunctionCount; n++)
				{
					uint NID = FunctionNIDReader.ReadUInt32();
					uint CallAddress = FunctionAddressReader.ReadUInt32();
					HleModuleExports.Functions[NID] = new HleModuleImportsExports.Entry() { Address = CallAddress };

					Console.WriteLine("  |  - FUNC: {0:X} : {1:X} : {2}", NID, CallAddress, Enum.GetName(typeof(SpecialFunctionNids), NID));
				}

				for (int n = 0; n < ModuleExport.VariableCount; n++)
				{
					uint NID = VariableNIDReader.ReadUInt32();
					uint CallAddress = VariableAddressReader.ReadUInt32();
					HleModuleExports.Variables[NID] = new HleModuleImportsExports.Entry() { Address = CallAddress };

					Console.WriteLine("  |  - VAR: {0:X} : {1:X} : {2}", NID, CallAddress, Enum.GetName(typeof(SpecialVariableNids), NID));
				}

				HleModuleGuest.ModulesExports.Add(HleModuleExports);
			}

			HleModuleGuest.ExportModules();
		}

		/// <summary>
		/// 
		/// </summary>
		public enum SpecialFunctionNids : uint
		{
			module_start = 0xD632ACDB,
			module_stop = 0xCEE8593C,
			module_reboot_before = 0x2F064FA6,
			module_reboot_phase = 0xADF12745,
			module_bootstart = 0xD3744BE0,
		}

		/// <summary>
		/// 
		/// </summary>
		public enum SpecialVariableNids : uint
		{
			module_info = 0xF01D73A7,
			module_start_thread_parameter = 0x0F7C276C,
			module_stop_thread_parameter = 0xCF0CC697,
			module_reboot_before_thread_parameter = 0xF4F4299D,
			module_sdk_version = 0x11B97506,
		}

		/// <summary>
		/// 
		/// </summary>
		private void _UpdateModuleImports()
		{
			//var BaseMemoryStream = ElfLoader.MemoryStream.SliceWithLength(BaseAddress);
			var ImportsStream = ElfLoader.MemoryStream.SliceWithBounds(HleModuleGuest.ModuleInfo.ImportsStart, HleModuleGuest.ModuleInfo.ImportsEnd);
			//Console.WriteLine("ImportsStream.Length: {0}", ImportsStream.Length);
			var ModuleImports = ImportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();

			Console.WriteLine("BASE ADDRESS: 0x{0:X}", BaseAddress);

			Console.WriteLine("Imports ({0:X8}-{1:X8}):", HleModuleGuest.ModuleInfo.ImportsStart, HleModuleGuest.ModuleInfo.ImportsEnd);

			foreach (var ModuleImport in ModuleImports)
			{
				String ModuleImportName = "INVALID";
				try { ModuleImportName = ElfLoader.MemoryStream.ReadStringzAt(ModuleImport.Name); } catch { }

				var HleModuleImports = new HleModuleImports();
				HleModuleImports.Name = ModuleImportName;
				HleModuleImports.Flags = ModuleImport.Flags;
				HleModuleImports.Version = ModuleImport.Version;

				var NidStreamReader = new BinaryReader(ElfLoader.MemoryStream.SliceWithLength(ModuleImport.NidAddress, ModuleImport.FunctionCount * sizeof(uint)));

				for (int n = 0; n < ModuleImport.FunctionCount; n++)
				{
					var NID = NidStreamReader.ReadUInt32();
					var CallAddress = (uint)(ModuleImport.CallAddress + n * 8);

					HleModuleImports.Functions[NID] = new HleModuleImportsExports.Entry() { Address = CallAddress };
				}

				HleModuleGuest.ModulesImports.Add(HleModuleImports);
			}

			HleModuleGuest.ImportModules();

			//Console.ReadKey();
		}

		
	}
}
