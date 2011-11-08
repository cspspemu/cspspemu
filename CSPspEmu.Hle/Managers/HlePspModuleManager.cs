using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public class HlePspModuleManager
	{
		protected Dictionary<Type, HlePspHleModule> HleModules = new Dictionary<Type, HlePspHleModule>();
		public HleState HleState;

		public HlePspModuleManager(HleState HleState)
		{
			this.HleState = HleState;
		}

		public TType GetModule<TType>() where TType : HlePspHleModule
		{
			if (!HleModules.ContainsKey(typeof(TType)))
			{
				var HleModule = HleModules[typeof(TType)] = Activator.CreateInstance<TType>();
				HleModule.Initialize(HleState);
			}

			return (TType)HleModules[typeof(TType)];
		}
	}
}
