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

		public ElfPspLoader(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
		}

		protected uint BaseAddress;
		public ElfPsp.ModuleInfo ModuleInfo { get; protected set; }
		public InitInfoStruct InitInfo;
		public ElfLoader ElfLoader;
		public HleModuleManager ModuleManager;

		public void Load(Stream FileStream, Stream MemoryStream, MemoryPartition MemoryPartition, HleModuleManager ModuleManager)
		{
			this.ElfLoader = new ElfLoader();
			this.ModuleManager = ModuleManager;

			this.ElfLoader.Load(FileStream);

			BaseAddress = (uint)(this.ElfLoader.NeedsRelocation ? 0x08900000 : 0);

			this.ElfLoader.AllocateAndWrite(MemoryStream, MemoryPartition, BaseAddress);

			if (this.ElfLoader.NeedsRelocation)
			{
				Relocate();
			}

			this.ModuleInfo = ElfLoader.SectionHeaderFileStream(ElfLoader.SectionHeadersByName[".rodata.sceModuleInfo"]).ReadStruct<ElfPsp.ModuleInfo>(); ;

			//Console.WriteLine(this.ModuleInfo.ToStringDefault());

			this.InitInfo.PC = ElfLoader.Header.EntryPoint + BaseAddress;
			this.InitInfo.GP = this.ModuleInfo.GP;

			UpdateModuleImports();
		}

		protected void Relocate()
		{
			if ((BaseAddress & 0xFFFF) != 0)
			{
				throw(new NotImplementedException("Can't relocate with the BaseAddress.LO16 != 0"));
			}

			// Relocate from program headers
			foreach (var ProgramHeader in ElfLoader.ProgramHeaders)
			{
				switch (ProgramHeader.Type)
				{
					case Elf.ProgramHeader.TypeEnum.Reloc1:
						throw (new NotImplementedException());
						/*
						int RelCount = (int)phdr.getP_filesz() / Elf32Relocate.sizeof();
						f.position((int)(elfOffset + phdr.getP_offset()));
						relocateFromBuffer(f, module, baseAddress, elf, RelCount);
						*/
					case Elf.ProgramHeader.TypeEnum.Reloc2:
						throw(new NotImplementedException());
				}
			}

			foreach (var SectionHeader in ElfLoader.SectionHeaders)
			{
				switch (SectionHeader.Type)
				{
					case Elf.SectionHeader.TypeEnum.Relocation:
						throw(new NotImplementedException());
					case Elf.SectionHeader.TypeEnum.PrxRelocation:
						RelocateRelocs(
							ElfLoader.SectionHeaderFileStream(SectionHeader).ReadStructVectorUntilTheEndOfStream<Elf.Reloc>()
						);
						break;
				}
			}
		}

		protected void RelocateRelocs(IEnumerable<Elf.Reloc> Relocs)
		{
			var InstructionReader = new InstructionReader(ElfLoader.MemoryStream);

			/*
			Func<uint, Action<ref Instruction>> UpdateInstruction = (Address) =>
			{
			};
			*/

			//var Hi16List = new List<uint>();

			foreach (var Reloc in Relocs)
			{
				var RelocatedAddress = Reloc.GetRelocatedAddress(BaseAddress);

				//Console.WriteLine(Reloc.ToStringDefault());
				//Console.WriteLine("   {0:X}", RelocatedAddress);

				var Instruction = InstructionReader[RelocatedAddress];

				switch (Reloc.Type)
				{
					case Elf.Reloc.TypeEnum.MipsHi16:
						{
							Instruction.IMMU = Instruction.IMMU + (RelocatedAddress >> 16);
						}

						{
							//Hi16List.Add(RelocatedAddress);
						}
						break;
					case Elf.Reloc.TypeEnum.MipsLo16:
						{
							//foreach (var Hi16 in Hi16List) { } Hi16List.Clear();
						}
						break;
					case Elf.Reloc.TypeEnum.Mips26:
						{
							uint PointedAddress = BaseAddress + Instruction.JUMP * 4;
							Instruction.JUMP = PointedAddress / 4;
						}
						break;
					case Elf.Reloc.TypeEnum.Mips32:
						{
							Instruction.Value = BaseAddress + Instruction.Value;
						}
						break;
					case Elf.Reloc.TypeEnum.MipsGpRel16:
						{
						}
						break;
					//case Elf.Reloc.TypeEnum.MipsLo16:
					default:
						throw(new NotImplementedException("Handling " + Reloc.Type + " not implemented"));
				}

				InstructionReader[RelocatedAddress] = Instruction;
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
			var ImportsStream = ElfLoader.MemoryStream.SliceWithBounds(ModuleInfo.ImportsStart + BaseAddress, ModuleInfo.ImportsEnd + BaseAddress);
			var ExportsStream = ElfLoader.MemoryStream.SliceWithBounds(ModuleInfo.ExportsStart + BaseAddress, ModuleInfo.ExportsEnd + BaseAddress);

			var ModuleImports = ImportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();
			var ModuleExports = ExportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();

			foreach (var SectionHeader in ElfLoader.SectionHeaders)
			{
				//Console.WriteLine("{0}: {1}", NameAt(SectionHeader.Name), SectionHeader);
			}

			foreach (var ModuleImport in ModuleImports)
			{
				//Console.WriteLine(ModuleImport.ToStringDefault());

				var ModuleImportName = ElfLoader.MemoryStream.ReadStringzAt(ModuleImport.Name);
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
					throw(new Exception(Exception.Message, Exception));
				}

				Console.WriteLine("{0:X}:'{1}'", ModuleImport.Name, ModuleImportName);
				for (int n = 0; n < ModuleImport.FunctionCount; n++)
				{
					uint NID = NidStreamReader.ReadUInt32();
					var DefaultName = String.Format("<unknown:{0:X}>", NID);
					var NIDName = (Module != null) ? Module.NamesByNID.GetOrDefault(NID, DefaultName) : DefaultName;
					//var Delegate = Module.DelegatesByNID.GetOrDefault(NID, null);
					CallStreamWriter.Write((uint)(0x0000000C | (FunctionGenerator.NativeCallSyscallCode << 6))); // syscall 0x2307
					CallStreamWriter.Write(
						(uint)ModuleManager.AllocDelegateSlot(
							CreateDelegate(ModuleManager, Module, NID, ModuleImportName, NIDName),
							String.Format("{0}:{1}", ModuleImportName, NIDName)
						)
					);

					Console.WriteLine(
						"    CODE_ADDR({0:X})  :  NID(0x{1,8:X}) : {2}",
						ModuleImport.CallAddress + n * 8, NID, NIDName
					);
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
