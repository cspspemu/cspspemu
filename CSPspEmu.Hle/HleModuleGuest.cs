using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Loader;
using CSPspEmu.Core.Memory;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Cpu.Emiter;

namespace CSPspEmu.Hle
{
	unsafe public struct SceModule
	{
		/// <summary>
		/// 
		/// </summary>
		public PspPointer Next; // SceModule*

		/// <summary>
		/// 
		/// </summary>
		public ElfPsp.ModuleInfo.AtributesEnum Attributes;

		/// <summary>
		/// 
		/// </summary>
		public ushort Version;

		/// <summary>
		/// 
		/// </summary>
		public fixed byte ModuleNameRaw[28];

		/// <summary>
		/// 
		/// </summary>
		public string ModuleName
		{
			get
			{
				fixed (byte* Ptr = ModuleNameRaw) return PointerUtils.PtrToStringUtf8(Ptr);
			}
			set
			{
				fixed (byte* Ptr = ModuleNameRaw) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, Ptr, 28);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public uint Unknown1;

		/// <summary>
		/// 
		/// </summary>
		public uint Unknown2;

		/// <summary>
		/// 
		/// </summary>
		public SceUID ModuleId;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint Unknown3[4];

		/// <summary>
		/// 
		/// </summary>
		public PspPointer ent_top; // void*

		/// <summary>
		/// 
		/// </summary>
		public uint ent_size;

		/// <summary>
		/// 
		/// </summary>
		public PspPointer stub_top; // void*

		/// <summary>
		/// 
		/// </summary>
		public uint stub_size;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint unknown4[4];

		/// <summary>
		/// 
		/// </summary>
		public uint EntryAddress;

		/// <summary>
		/// 
		/// </summary>
		public uint GP;

		/// <summary>
		/// 
		/// </summary>
		public uint TextAddress;

		/// <summary>
		/// 
		/// </summary>
		public uint TextSize;

		/// <summary>
		/// 
		/// </summary>
		public uint DataSize;

		/// <summary>
		/// 
		/// </summary>
		public uint BssSize;

		/// <summary>
		/// 
		/// </summary>
		public uint NumberOfSegments;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint SegmentAddress[4];

		/// <summary>
		/// 
		/// </summary>
		public fixed uint SegmentSize[4];
	}

	public class HleModuleImportsExports
	{
		public string Name;
		public ushort Version;
		public ushort Flags;

		public class Entry
		{
			public bool Linked;
			public uint Address;
		}

		public Dictionary<uint, Entry> Functions = new Dictionary<uint, Entry>();
		public Dictionary<uint, Entry> Variables = new Dictionary<uint, Entry>();
	}

	public class HleModuleImports : HleModuleImportsExports
	{
	}

	public class HleModuleExports : HleModuleImportsExports
	{
	}

	unsafe public class HleModuleGuest : HleModule
	{
		public string Name { get { return ModuleInfo.Name; } }
		public HleState HleState;
		public bool Loaded;
		public ElfPsp.ModuleInfo ModuleInfo;
		public InitInfoStruct InitInfo;
		public MemoryPartition SceModuleStructPartition;
		public List<HleModuleImports> ModulesImports = new List<HleModuleImports>();
		public List<HleModuleExports> ModulesExports = new List<HleModuleExports>();

		public HleModuleGuest(HleState HleState)
		{
			this.HleState = HleState;
		}

		public void ExportModules()
		{
			foreach (var ExportModule in ModulesExports)
			{
				var ExportModuleName = ExportModule.Name;

				foreach (var Module in HleState.ModuleManager.LoadedGuestModules)
				{
					//Console.WriteLine("{0} - {1}", ExportModuleName, Module.Name);
					var ImportModule = Module.ModulesImports.Find(Item => Item.Name == ExportModuleName);
					if (ImportModule != null)
					{
						foreach (var ImportFunction in ImportModule.Functions)
						{
							var ExportFunctionEntry = ExportModule.Functions[ImportFunction.Key];
							var ImportFunctionEntry = ImportFunction.Value;

							if (!ImportFunctionEntry.Linked)
							{
								ImportFunctionEntry.Linked = true;
								var FunctionAddress = ExportFunctionEntry.Address;
								var CallAddress = ImportFunctionEntry.Address;

								// J
								//0000 10ii iiii iiii iiii iiii iiii iiii
								var Instruction = default(Instruction);
								Instruction.OP1 = 2;
								Instruction.JUMP_Real = FunctionAddress;

								HleState.CpuProcessor.Memory.Write4(CallAddress + 0, Instruction); // J
								HleState.CpuProcessor.Memory.Write4(CallAddress + 4, 0x00000000); // NOP
							}
						}
					}
				}
			}
		}

		public void ImportModules()
		{
			var ModuleManager = HleState.ModuleManager;

			foreach (var ModuleImports in ModulesImports)
			{
				HleModuleHost Module = null;
				try
				{
					Module = ModuleManager.GetModuleByName(ModuleImports.Name);
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
				}

				Console.WriteLine("'{0}' - {1}", ModuleImports.Name, (Module != null) ? Module.ModuleLocation : "?");
				foreach (var Function in ModuleImports.Functions)
				{
					var NID = Function.Key;
					var CallAddress = Function.Value.Address;

					var DefaultEntry = new FunctionEntry()
					{
						NID = 0x00000000,
						Name = CStringFormater.Sprintf("__<unknown:0x%08X>", (int)NID),
						Description = "Unknown",
					};
					var FunctionEntry = (Module != null) ? Module.EntriesByNID.GetOrDefault(NID, DefaultEntry) : DefaultEntry;
					//var Delegate = Module.DelegatesByNID.GetOrDefault(NID, null);

					HleState.CpuProcessor.Memory.Write4(CallAddress + 0, FunctionGenerator.NativeCallSyscallOpCode);  // syscall NativeCallSyscallCode
					HleState.CpuProcessor.Memory.Write4(CallAddress + 4, (uint)ModuleManager.AllocDelegateSlot(
						CreateDelegate(ModuleManager, Module, NID, ModuleImports.Name, FunctionEntry.Name),
						ModuleImports.Name, FunctionEntry
					));

					Console.WriteLine(
						"    CODE_ADDR({0:X})  :  NID(0x{1,8:X}) : {2} - {3}",
						CallAddress, NID, FunctionEntry.Name, FunctionEntry.Description
					);
				}
			}
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
							HleState.PspConfig.PspEmulatorContext.GetInstance<HleThreadManager>().Current.Id,
							HleState.PspConfig.PspEmulatorContext.GetInstance<HleThreadManager>().Current.Name,
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
