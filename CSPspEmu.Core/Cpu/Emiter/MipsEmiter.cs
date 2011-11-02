using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	public class MipsEmiter
	{
		static private ulong UniqueCounter = 0;
		internal ModuleBuilder ModuleBuilder;

		public MipsEmiter()
		{
			UniqueCounter++;
			var CurrentAppDomain = AppDomain.CurrentDomain;
			var AssemblyBuilder = CurrentAppDomain.DefineDynamicAssembly(new AssemblyName("assembly" + UniqueCounter), AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule("module" + UniqueCounter);
		}
	}
}
