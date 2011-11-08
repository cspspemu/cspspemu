using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Managers
{
	public class HleModuleManager
	{
		protected Dictionary<Type, HleModuleHost> HleModules = new Dictionary<Type, HleModuleHost>();
		public HleState HleState;

		public HleModuleManager(HleState HleState)
		{
			this.HleState = HleState;
		}

		public TType GetModule<TType>() where TType : HleModuleHost
		{
			if (!HleModules.ContainsKey(typeof(TType)))
			{
				var HleModule = HleModules[typeof(TType)] = Activator.CreateInstance<TType>();
				HleModule.Initialize(HleState);
			}

			return (TType)HleModules[typeof(TType)];
		}

		public Action<CpuThreadState> GetModuleDelegate<TType>(String FunctionName) where TType : HleModuleHost
		{
			return GetModule<TType>().DelegatesByName[FunctionName];
		}
	}
}
