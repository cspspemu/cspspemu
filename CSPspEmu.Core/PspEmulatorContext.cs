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
			object Output = null;

			if (!ObjectsByType.TryGetValue(typeof(TType), out Output))
			{
				Output = Activator.CreateInstance(typeof(TType), this);
				//Output = new TType();
			}

			return (TType)Output;
		}
	}
}
