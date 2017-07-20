using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu;
using System.Reflection;
using CSharpUtils;
using SafeILGenerator.Utils;

namespace CSPspEmu.Hle.Managers
{
    public sealed class HleModuleManager : IInjectInitialize, IDisposable
    {
        private readonly Dictionary<Type, HleModuleHost> HleModules = new Dictionary<Type, HleModuleHost>();
        public readonly List<HleModuleGuest> LoadedGuestModules = new List<HleModuleGuest>();
        private readonly Dictionary<uint, DelegateInfo> DelegateTable = new Dictionary<uint, DelegateInfo>();
        public readonly Queue<DelegateInfo> LastCalledCallbacks = new Queue<DelegateInfo>();
        private uint DelegateLastId = 0;

        [Inject] HleThreadManager HleThreadManager;

        [Inject] CpuProcessor CpuProcessor;

        [Inject] InjectContext InjectContext;

        [Inject] HleConfig HleConfig;

        public static IEnumerable<Type> GetAllHleModules(Assembly ModulesAssembly)
        {
            var FindType = typeof(HleModuleHost);
            //foreach (var Type in ModulesAssembly.GetTypes()) Console.WriteLine(Type);
            return ModulesAssembly.GetTypes().Where(Type => FindType.IsAssignableFrom(Type));
        }

        public Dictionary<string, Type> HleModuleTypes;

        protected int LastCallIndex = 0;

        void IInjectInitialize.Initialize()
        {
            if (HleConfig.HleModulesDll == null)
            {
                throw (new ArgumentNullException("PspEmulatorContext.PspConfig.HleModulesDll Can't be null"));
            }

            HleModuleTypes = GetAllHleModules(HleConfig.HleModulesDll).ToDictionary(Type => Type.Name);
            Console.WriteLine("HleModuleTypes: {0}", HleModuleTypes.Count);

            if (HleModuleTypes.Count < 10)
            {
                ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
                {
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!");
                    Console.WriteLine("Can't find HLE modules!!");
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!");
                });
            }

#if false
			CpuProcessor.RegisterNativeSyscall(SyscallInfo.NativeCallSyscallCode, (CpuThreadState, Code) =>
			{
				uint Info = CpuThreadState.CpuProcessor.Memory.ReadSafe<uint>(CpuThreadState.PC + 4);
				DelegateInfo DelegateInfo;
				if (!DelegateTable.TryGetValue(Info, out DelegateInfo))
				{
					throw (new Exception(String.Format("Can't find DelegateInfo for PC={0:X8}, Info=0x{1:X8}", CpuThreadState.PC, Info)));
				}
				if (PspConfig.TraceLastSyscalls)
				{
					//Console.WriteLine("{0:X}", CpuThreadState.RA);
					DelegateInfo.CallIndex = LastCallIndex++;
					DelegateInfo.PC = CpuThreadState.PC;
					DelegateInfo.RA = CpuThreadState.RA;
					DelegateInfo.Thread = HleThreadManager.Current;

#if false
//#if true
					Console.Error.WriteLine("HleModuleManager: " + DelegateInfo);
#endif

					if (DelegateInfo.ModuleImportName != "Kernel_Library")
					{
						LastCalledCallbacks.Enqueue(DelegateInfo);
						if (HleThreadManager != null && HleThreadManager.Current != null)
						{
							HleThreadManager.Current.LastCalledHleFunction = DelegateInfo;
						}
					}
					if (LastCalledCallbacks.Count > 10)
					{
						LastCalledCallbacks.Dequeue();
					}
				}
				DelegateInfo.Action(CpuThreadState);
				CpuThreadState.PC = CpuThreadState.RA;
			});
#endif
        }

        public HleModuleHost GetModuleByType(Type Type)
        {
            if (!HleModules.ContainsKey(Type))
            {
                var HleModule = HleModules[Type] = (HleModuleHost) InjectContext.GetInstance(Type);
                InjectContext.InjectDependencesTo(HleModule);
            }

            return (HleModuleHost) HleModules[Type];
        }

        public HleModuleHost GetModuleByName(string ModuleNameToFind)
        {
            //Console.WriteLine("GetModuleByName('{0}')", ModuleNameToFind);
            if (!HleModuleTypes.ContainsKey(ModuleNameToFind))
            {
                throw (new KeyNotFoundException("Can't find module '" + ModuleNameToFind + "'"));
                //return new HleModuleHost();
            }
            return GetModuleByType(HleModuleTypes[ModuleNameToFind]);
        }

        public TType GetModule<TType>() where TType : HleModuleHost
        {
            return (TType) GetModuleByType(typeof(TType));
        }

        public Action<CpuThreadState> GetModuleDelegate<TType>(string FunctionName) where TType : HleModuleHost
        {
            var Module = GetModule<TType>();
            var EntriesByName = Module.EntriesByName;
            if (!EntriesByName.ContainsKey(FunctionName))
            {
                throw (new KeyNotFoundException(
                    string.Format(
                        "Can't find method '{0}' on module '{1}'",
                        FunctionName,
                        Module.GetType().Name
                    )
                ));
            }
            return EntriesByName[FunctionName].Delegate;
        }

        public uint AllocDelegateSlot(Action<CpuThreadState> Action, string ModuleImportName,
            HleFunctionEntry FunctionEntry)
        {
            uint DelegateId = DelegateLastId++;
            if (Action == null)
            {
                Action = (CpuThreadState) =>
                {
                    throw (new NotImplementedException("Not Implemented Syscall '" + ModuleImportName + ":" +
                                                       FunctionEntry + "'"));
                };
            }
            CpuProcessor.RegisteredNativeSyscallMethods[DelegateId] = new NativeSyscallInfo()
            {
                ModuleImportName = ModuleImportName,
                FunctionEntryName = FunctionEntry.Name,
                Nid = FunctionEntry.NID,
                PoolItem = IlInstanceHolder.TAlloc<Action<CpuThreadState>>(FunctionEntry.Delegate),
            };
            DelegateTable[DelegateId] = new DelegateInfo()
            {
                Action = Action,
                ModuleImportName = ModuleImportName,
                FunctionEntry = FunctionEntry,
            };
            return DelegateId;
        }

        public void Dispose()
        {
            foreach (var HleModule in HleModules)
            {
                HleModule.Value.Dispose();
            }
            HleModules.Clear();
        }
    }
}