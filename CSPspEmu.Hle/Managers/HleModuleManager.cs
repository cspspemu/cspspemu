using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using System.Reflection;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Managers
{
	public class HleModuleManager : PspEmulatorComponent
	{
		protected Dictionary<Type, HleModuleHost> HleModules = new Dictionary<Type, HleModuleHost>();
		public uint DelegateLastId = 0;
		public Dictionary<uint, Action<CpuThreadState>> DelegateTable = new Dictionary<uint, Action<CpuThreadState>>();

		static public IEnumerable<Type> GetAllHleModules(Assembly ModulesAssembly)
		{
			var FindType = typeof(HleModuleHost);
			return ModulesAssembly.GetTypes().Where(Type => FindType.IsAssignableFrom(Type));
		}

		public Dictionary<String, Type> HleModuleTypes;

		public override void InitializeComponent()
		{
			HleModuleTypes = GetAllHleModules(PspEmulatorContext.PspConfig.HleModulesDll).ToDictionary(Type => Type.Name);
			Console.WriteLine("HleModuleTypes: {0}", HleModuleTypes.Count);

			PspEmulatorContext.GetInstance<CpuProcessor>().RegisterNativeSyscall(FunctionGenerator.NativeCallSyscallCode, (Code, CpuThreadState) =>
			{
				uint Info = CpuThreadState.CpuProcessor.Memory.Read4(CpuThreadState.PC + 4);
				{
					//Console.WriteLine("{0:X}", CpuThreadState.RA);
					DelegateTable[Info](CpuThreadState);
				}
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

		public uint AllocDelegateSlot(Action<CpuThreadState> Action, string Info)
		{
			uint DelegateId = DelegateLastId++;
			if (Action == null)
			{
				Action = (CpuThreadState) =>
				{
					throw (new NotImplementedException("Not Implemented Syscall '" + Info + "'"));
				};
			}
			DelegateTable[DelegateId] = Action;
			return DelegateId;
		}
	}
}
