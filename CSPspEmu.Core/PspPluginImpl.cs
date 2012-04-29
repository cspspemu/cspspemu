using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	abstract public class PspPluginImpl : PspEmulatorComponent
	{
		/// <summary>
		/// 
		/// </summary>
		abstract public PluginInfo PluginInfo { get; }

		/// <summary>
		/// 
		/// </summary>
		abstract public bool IsWorking { get; }

		static public void SelectWorkingPlugin<TType>(PspEmulatorContext PspEmulatorContext, params Type[] AvailablePluginImplementations) where TType : PspPluginImpl
		{
			foreach (var ImplementationType in AvailablePluginImplementations)
			{
				if (((PspPluginImpl)Activator.CreateInstance(ImplementationType)).IsWorking)
				{
					// Found a working implementation
					PspEmulatorContext.SetInstanceType<TType>(ImplementationType);
					break;
				}
			}
		}
	}
}
