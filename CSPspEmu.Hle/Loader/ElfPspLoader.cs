using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Formats;
using CSharpUtils.Extensions;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Core.Cpu;
using CSharpUtils;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Loader
{
	unsafe public class ElfPspLoader : PspEmulatorComponent
	{
		public struct InitInfoStruct
		{
			public uint PC;
			public uint GP;
		}

		public override void InitializeComponent()
		{
		}

		protected uint BaseAddress;
		public ElfPsp.ModuleInfo ModuleInfo { get; protected set; }
		public InitInfoStruct InitInfo;
		public ElfLoader ElfLoader;
		public HleModuleManager ModuleManager;

		public void Load(Stream FileStream, Stream MemoryStream, MemoryPartition MemoryPartition, HleModuleManager ModuleManager, String GameTitle)
		{
			this.ElfLoader = new ElfLoader();
			this.ModuleManager = ModuleManager;

			this.ElfLoader.Load(FileStream);

			PspEmulatorContext.PspConfig.InfoExeHasRelocation = this.ElfLoader.NeedsRelocation;

			if (this.ElfLoader.NeedsRelocation)
			{
				var DummyPartition = MemoryPartition.Allocate(
					0x4000,
					Name: "Dummy"
				);
				BaseAddress = MemoryPartition.ChildPartitions.OrderByDescending(Partition => Partition.Size).First().Low;
			}
			else
			{
				BaseAddress = 0;
			}

			PspEmulatorContext.PspConfig.RelocatedBaseAddress = BaseAddress;
			PspEmulatorContext.PspConfig.GameTitle = GameTitle;

			this.ElfLoader.AllocateAndWrite(MemoryStream, MemoryPartition, BaseAddress);

			if (this.ElfLoader.NeedsRelocation)
			{
				RelocateFromHeaders();
			}

			this.ModuleInfo = ElfLoader.SectionHeaderFileStream(ElfLoader.SectionHeadersByName[".rodata.sceModuleInfo"]).ReadStruct<ElfPsp.ModuleInfo>(); ;

			//Console.WriteLine(this.ModuleInfo.ToStringDefault());

			this.InitInfo.PC = ElfLoader.Header.EntryPoint + BaseAddress;
			this.InitInfo.GP = this.ModuleInfo.GP + BaseAddress;

			UpdateModuleImports();
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
				RelocOutput.WriteLine("Program Header: %d".Sprintf(RelocProgramIndex++));
				switch (ProgramHeader.Type)
				{
					case Elf.ProgramHeader.TypeEnum.Reloc1:
						Console.Error.WriteLine("NOT IMPLEMENTED Elf.ProgramHeader.TypeEnum.Reloc1!");
						//throw (new NotImplementedException());
						RelocateRelocs(ElfLoader.ProgramHeaderFileStream(ProgramHeader).ReadStructVectorUntilTheEndOfStream<Elf.Reloc>());
						break;
						//throw (new NotImplementedException());
						/*
						int RelCount = (int)phdr.getP_filesz() / Elf32Relocate.sizeof();
						f.position((int)(elfOffset + phdr.getP_offset()));
						relocateFromBuffer(f, module, baseAddress, elf, RelCount);
						*/
					case Elf.ProgramHeader.TypeEnum.Reloc2:
						throw(new NotImplementedException());
				}
			}

			int RelocSectionIndex = 0;
			foreach (var SectionHeader in ElfLoader.SectionHeaders)
			{
				//RelocOutput.WriteLine("Section Header: %d : %s".Sprintf(RelocSectionIndex++, SectionHeader.ToString()));
				RelocOutput.WriteLine("Section Header: %d".Sprintf(RelocSectionIndex++));

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

			RelocOutput.Flush();
			_RelocOutputStream.Flush();
			RelocOutput.Close();
			_RelocOutputStream.Close();
		}

		Stream _RelocOutputStream;
		StreamWriter _RelocOutput;

		StreamWriter RelocOutput {
			get {
				if (_RelocOutput == null)
				{
					_RelocOutputStream = File.Open("reloc.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
					_RelocOutput = new StreamWriter(_RelocOutputStream);
					_RelocOutput.AutoFlush = true;
				}
				return _RelocOutput;
			}
		}


		/// <summary>
		/// This function relocates all the instructions and pointers of the loading executable.
		/// </summary>
		/// <param name="Relocs"></param>
		protected void RelocateRelocs(IEnumerable<Elf.Reloc> Relocs)
		{
			var InstructionReader = new InstructionReader(ElfLoader.MemoryStream);

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

				RelocOutput.WriteLine(
					"RELOC %06d : 0x%08X : 0x%08X -> 0x%08X".Sprintf(
						Index,
						RelocatedPointerAddress, InstructionBefore.Value, Instruction.Value
					)
				);

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

		protected void UpdateModuleImports()
		{
			ConsoleUtils.SaveRestoreConsoleState(() =>
			{
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				_UpdateModuleImports();
			});
		}

		private void _UpdateModuleImports()
		{
			var BaseMemoryStream = ElfLoader.MemoryStream.SliceWithLength(BaseAddress);

			var ImportsStream = BaseMemoryStream.SliceWithBounds(ModuleInfo.ImportsStart, ModuleInfo.ImportsEnd);
			var ExportsStream = BaseMemoryStream.SliceWithBounds(ModuleInfo.ExportsStart, ModuleInfo.ExportsEnd);

			var ModuleImports = ImportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();
			var ModuleExports = ExportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();

			foreach (var SectionHeader in ElfLoader.SectionHeaders)
			{
				//Console.WriteLine("{0}: {1}", NameAt(SectionHeader.Name), SectionHeader);
			}

			foreach (var ModuleImport in ModuleImports)
			{
				//Console.WriteLine(ModuleImport.ToStringDefault());

				try
				{
					String ModuleImportName = "INVALID";
					ModuleImportName = ElfLoader.MemoryStream.ReadStringzAt(ModuleImport.Name);
					/*
					try
					{
						
					}
					catch (Exception Exception)
					{
						Console.Error.WriteLine(Exception);
					}
					*/
					var NidStreamReader = new BinaryReader(ElfLoader.MemoryStream.SliceWithLength(ModuleImport.NidAddress, ModuleImport.FunctionCount * sizeof(uint)));
					var CallStreamWriter = new BinaryWriter(ElfLoader.MemoryStream.SliceWithLength(ModuleImport.CallAddress, ModuleImport.FunctionCount * sizeof(uint) * 2));

					HleModuleHost Module = null;
					try
					{
						Module = ModuleManager.GetModuleByName(ModuleImportName);
					}
					catch (Exception Exception)
					{
						Console.WriteLine(Exception);
						throw (new Exception(Exception.Message, Exception));
					}

					Console.WriteLine("{0:X}:'{1}'", ModuleImport.Name, ModuleImportName);
					for (int n = 0; n < ModuleImport.FunctionCount; n++)
					{
						uint NID = NidStreamReader.ReadUInt32();
						var DefaultEntry = new FunctionEntry()
						{
							NID = 0x00000000,
							Name = String.Format("__<unknown:0x{0:X}>", NID),
							Description = "Unknown",
						};
						var FunctionEntry = (Module != null) ? Module.EntriesByNID.GetOrDefault(NID, DefaultEntry) : DefaultEntry;
						//var Delegate = Module.DelegatesByNID.GetOrDefault(NID, null);
						CallStreamWriter.Write(FunctionGenerator.NativeCallSyscallOpCode); // syscall NativeCallSyscallCode
						CallStreamWriter.Write(
							(uint)ModuleManager.AllocDelegateSlot(
								CreateDelegate(ModuleManager, Module, NID, ModuleImportName, FunctionEntry.Name),
								ModuleImportName, FunctionEntry
							)
						);

						Console.WriteLine(
							"    CODE_ADDR({0:X})  :  NID(0x{1,8:X}) : {2} - {3}",
							ModuleImport.CallAddress + n * 8, NID, FunctionEntry.Name, FunctionEntry.Description
						);
					}
				}
				/*
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
				*/
				finally
				{
				}
			}

			//Console.ReadKey();
		}

		protected Action<CpuThreadState> CreateDelegate(HleModuleManager ModuleManager, HleModuleHost Module, uint NID, string ModuleImportName, string NIDName)
		{
			Action<CpuThreadState> Callback = null;
			if (Module != null)
			{
				Callback = Module.DelegatesByNID.GetOrDefault(NID, null);
			}

			return (CpuThreadState) =>
			{
				if (Callback == null)
				{
					if (CpuThreadState.CpuProcessor.PspConfig.DebugSyscalls)
					{
						Console.WriteLine(
							"Thread({0}:'{1}'):{2}:{3}",
							PspEmulatorContext.GetInstance<HleThreadManager>().Current.Id,
							PspEmulatorContext.GetInstance<HleThreadManager>().Current.Name,
							ModuleImportName, NIDName
						);
					}
					throw (new NotImplementedException("Not Implemented '" + String.Format("{0}:{1}", ModuleImportName, NIDName) + "'"));
				}
				else
				{
					Callback(CpuThreadState);
				}
			};
		}
	}
}
