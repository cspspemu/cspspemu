using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	public class PspEmulatorContext
	{
		public PspConfig PspConfig;

		public event Action ApplicationExit;

		public PspEmulatorContext(PspConfig PspConfig)
		{
			this.PspConfig = PspConfig;
		}

		Dictionary<Type, object> ObjectsByType = new Dictionary<Type, object>();

		public TType GetInstance<TType>() where TType : PspEmulatorComponent
		{
			if (!ObjectsByType.ContainsKey(typeof(TType)))
			{
				SetInstance<TType>(Activator.CreateInstance(typeof(TType), this));
			}

			return (TType)ObjectsByType[typeof(TType)];
		}

		public TType SetInstance<TType>(object Instance)
		{
			ObjectsByType[typeof(TType)] = Instance;
			return (TType)Instance;
		}
	}
}
