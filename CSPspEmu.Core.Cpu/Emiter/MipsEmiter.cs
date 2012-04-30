using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	public class MipsEmiter : PspEmulatorComponent
	{
		static private ulong UniqueCounter = 0;
		internal AssemblyBuilder AssemblyBuilder;
		internal ModuleBuilder ModuleBuilder;

		public override void InitializeComponent()
		{
			UniqueCounter++;
			var CurrentAppDomain = AppDomain.CurrentDomain;
			AssemblyBuilder = CurrentAppDomain.DefineDynamicAssembly(new AssemblyName("assembly" + UniqueCounter), AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule("module" + UniqueCounter);
		}
	}
}
