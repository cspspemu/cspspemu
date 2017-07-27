using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Loader;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Utils;

namespace CSPspEmu.Hle
{
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

    public class HleModuleGuest : HleModule
    {
        public int ID;

        public string Name => ModuleInfo.Name;

        public bool Loaded;
        public ElfPsp.ModuleInfo ModuleInfo;
        public InitInfoStruct InitInfo;
        public MemoryPartition SceModuleStructPartition;
        public List<HleModuleImports> ModulesImports = new List<HleModuleImports>();
        public List<HleModuleExports> ModulesExports = new List<HleModuleExports>();

        [Inject] HleModuleManager ModuleManager;

        [Inject] CpuProcessor CpuProcessor;

        [Inject] HleThreadManager HleThreadManager;

        private HleModuleGuest()
        {
        }

        public void LinkFunction(uint CallAddress, uint FunctionAddress)
        {
            // J
            //0000 10ii iiii iiii iiii iiii iiii iiii
            var Instruction = default(Instruction);
            Instruction.Op1 = 2;
            Instruction.JumpReal = FunctionAddress;

            CpuProcessor.Memory.WriteSafe(CallAddress + 0, Instruction); // J
            CpuProcessor.Memory.WriteSafe(CallAddress + 4, 0x00000000); // NOP
        }

        public void LinkFunction(uint CallAddress, HleFunctionEntry NativeFunction)
        {
            //Console.WriteLine(NativeFunction);

            CpuProcessor.Memory.WriteSafe(CallAddress + 0,
                SyscallInfo.NativeCallSyscallOpCode); // syscall NativeCallSyscallCode
            CpuProcessor.Memory.WriteSafe(CallAddress + 4, (uint) ModuleManager.AllocDelegateSlot(
                Action: CreateDelegate(
                    ModuleManager: ModuleManager,
                    Module: NativeFunction.Module,
                    NID: NativeFunction.NID,
                    ModuleImportName: NativeFunction.ModuleName,
                    NIDName: NativeFunction.Name
                ),
                ModuleImportName: NativeFunction.ModuleName,
                FunctionEntry: NativeFunction
            ));

            Console.WriteLine(
                "    CODE_ADDR({0:X})  :  NID(0x{1:X8}) : {2} - {3}",
                CallAddress, NativeFunction.NID, NativeFunction.Name, NativeFunction.Description
            );
        }

        public void ExportModules()
        {
            foreach (var Module in ModuleManager.LoadedGuestModules)
            {
                ExportModules(Module);
            }
        }

        public void ExportModules(HleModuleGuest Module)
        {
            foreach (var ExportModule in ModulesExports)
            {
                var ExportModuleName = ExportModule.Name;

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
                            var CallAddress = ImportFunctionEntry.Address;
                            var FunctionAddress = ExportFunctionEntry.Address;

                            LinkFunction(CallAddress, FunctionAddress);
                        }
                    }
                }
            }
        }


        public void ImportModules()
        {
            foreach (var ModuleImports in ModulesImports)
            {
                HleModuleHost HleModuleHost = null;
                try
                {
                    HleModuleHost = ModuleManager.GetModuleByName(ModuleImports.Name);
                }
                catch (Exception Exception)
                {
                    Console.WriteLine(Exception);
                }

                // Can't use a host module. Try to use a Guest module.
                if (HleModuleHost == null)
                {
                    var HleModuleGuest =
                        ModuleManager.LoadedGuestModules.FirstOrDefault(ModuleExports =>
                            (ModuleExports.Name == ModuleImports.Name));
                    if (HleModuleGuest != null)
                    {
                        HleModuleGuest.ExportModules(this);
                        continue;
                    }
                }

                Console.WriteLine("'{0}' - {1}", ModuleImports.Name,
                    (HleModuleHost != null) ? HleModuleHost.ModuleLocation : "?");
                foreach (var Function in ModuleImports.Functions)
                {
                    var NID = Function.Key;
                    var CallAddress = Function.Value.Address;

                    var DefaultEntry = new HleFunctionEntry()
                    {
                        NID = 0x00000000,
                        Name = CStringFormater.Sprintf("__<unknown:0x%08X>", (int) NID),
                        Description = "Unknown",
                        Module = null,
                        ModuleName = ModuleImports.Name,
                    };
                    var FunctionEntry = HleModuleHost?.EntriesByNID.GetOrDefault(NID, DefaultEntry) ?? DefaultEntry;
                    FunctionEntry.NID = NID;
                    //var Delegate = Module.DelegatesByNID.GetOrDefault(NID, null);

                    LinkFunction(CallAddress, FunctionEntry);
                }
            }
        }

        protected Action<CpuThreadState> CreateDelegate(HleModuleManager ModuleManager, HleModuleHost Module, uint NID,
            string ModuleImportName, string NIDName)
        {
            Action<CpuThreadState> Callback = null;
            if (Module != null)
            {
                if (Module.EntriesByNID.ContainsKey(NID))
                {
                    Callback = Module.EntriesByNID[NID].Delegate;
                }
                else
                {
                    Callback = null;
                }
            }

            return (CpuThreadState) =>
            {
                if (Callback == null)
                {
                    if (CpuThreadState.CpuProcessor.CpuConfig.DebugSyscalls)
                    {
                        Console.WriteLine(
                            "Thread({0}:'{1}'):{2}:{3}",
                            HleThreadManager.Current.Id,
                            HleThreadManager.Current.Name,
                            ModuleImportName, NIDName
                        );
                    }
                    throw (new NotImplementedException("Not Implemented '" +
                                                       $"{ModuleImportName}:{NIDName}" + "'"));
                }
                else
                {
                    Callback(CpuThreadState);
                }
            };
        }
    }
}