using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using System.Reflection;
using CSPspEmu.Core;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Managers
{
	public class HleModuleManager : PspEmulatorComponent
	{
		public struct DelegateInfo
		{
			public int CallIndex;
			public uint PC;
			public uint RA;
			public string ModuleImportName;
			public HleModuleHost.FunctionEntry FunctionEntry;
			public Action<CpuThreadState> Action;
			public HleThread Thread;

			public override string ToString()
			{
				try
				{
					return String.Format("{0}: PC=0x{3:X}, RA=0x{4:X} => '{5}' : {1}::{2}", CallIndex, ModuleImportName, FunctionEntry.Name, PC, RA, Thread.Name);
				}
				catch (Exception Exception)
				{
					return String.Format("Invalid DelegateInfo");
				}
				//return this.ToStringDefault();
			}
		}

		protected Dictionary<Type, HleModuleHost> HleModules = new Dictionary<Type, HleModuleHost>();
		public uint DelegateLastId = 0;
		public Dictionary<uint, DelegateInfo> DelegateTable = new Dictionary<uint, DelegateInfo>();
		public Queue<DelegateInfo> LastCalledCallbacks = new Queue<DelegateInfo>();
		protected HleThreadManager HleThreadManager;
		protected CpuProcessor CpuProcessor;
		protected PspConfig PspConfig;

		static public IEnumerable<Type> GetAllHleModules(Assembly ModulesAssembly)
		{
			var FindType = typeof(HleModuleHost);
			return ModulesAssembly.GetTypes().Where(Type => FindType.IsAssignableFrom(Type));
		}

		public Dictionary<String, Type> HleModuleTypes;

		protected int LastCallIndex = 0;

		public override void InitializeComponent()
		{
			HleModuleTypes = GetAllHleModules(PspEmulatorContext.PspConfig.HleModulesDll).ToDictionary(Type => Type.Name);
			HleThreadManager = PspEmulatorContext.GetInstance<HleThreadManager>();
			Console.WriteLine("HleModuleTypes: {0}", HleModuleTypes.Count);
			CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			PspConfig = CpuProcessor.PspConfig;

			PspEmulatorContext.GetInstance<CpuProcessor>().RegisterNativeSyscall(FunctionGenerator.NativeCallSyscallCode, (Code, CpuThreadState) =>
			{
				uint Info = CpuThreadState.CpuProcessor.Memory.Read4(CpuThreadState.PC + 4);
				var DelegateInfo = DelegateTable[Info];
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
					}
					if (LastCalledCallbacks.Count > 10)
					{
						LastCalledCallbacks.Dequeue();
					}
				}
				DelegateInfo.Action(CpuThreadState);
				CpuThreadState.PC = CpuThreadState.RA;
			});
		}


		public TType GetModule<TType>() where TType : HleModuleHost
		{
			return (TType)GetModuleByType(typeof(TType));
		}

		public HleModuleHost GetModuleByType(Type Type)
		{
			if (!HleModules.ContainsKey(Type))
			{
				var HleModule = HleModules[Type] = (HleModuleHost)Activator.CreateInstance(Type);
				HleModule.Initialize(PspEmulatorContext.GetInstance<HleState>());
			}

			return (HleModuleHost)HleModules[Type];
		}

		public HleModuleHost GetModuleByName(String ModuleNameToFind)
		{
			//Console.WriteLine("GetModuleByName('{0}')", ModuleNameToFind);
			if (!HleModuleTypes.ContainsKey(ModuleNameToFind))
			{
				throw (new KeyNotFoundException("Can't find module '" + ModuleNameToFind + "'"));
			}
			return GetModuleByType(HleModuleTypes[ModuleNameToFind]);
		}

		public Action<CpuThreadState> GetModuleDelegate<TType>(String FunctionName) where TType : HleModuleHost
		{
			var Module = GetModule<TType>();
			var DelegatesByName = Module.DelegatesByName;
			if (!DelegatesByName.ContainsKey(FunctionName))
			{
				throw (new KeyNotFoundException(
					String.Format(
						"Can't find method '{0}' on module '{1}'",
						FunctionName,
						Module.GetType().Name
					)
				));
			}
			return DelegatesByName[FunctionName];
		}

		public uint AllocDelegateSlot(Action<CpuThreadState> Action, string ModuleImportName, HleModuleHost.FunctionEntry FunctionEntry)
		{
			uint DelegateId = DelegateLastId++;
			if (Action == null)
			{
				Action = (CpuThreadState) =>
				{
					throw (new NotImplementedException("Not Implemented Syscall '" + ModuleImportName + ":" + FunctionEntry + "'"));
				};
			}
			DelegateTable[DelegateId] = new DelegateInfo()
			{
				Action = Action,
				ModuleImportName = ModuleImportName,
				FunctionEntry = FunctionEntry,
			};
			return DelegateId;
		}

		public override void Dispose()
		{
			foreach (var HleModule in HleModules)
			{
				HleModule.Value.Dispose();
			}
			HleModules = new Dictionary<Type, HleModuleHost>();
		}
	}
}
