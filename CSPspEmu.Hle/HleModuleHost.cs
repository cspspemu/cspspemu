using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CSPspEmu.Core.Cpu;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public unsafe partial class HleModuleHost : HleModule
	{
		public string ModuleLocation;
		public Dictionary<uint, HleFunctionEntry> EntriesByNID = new Dictionary<uint, HleFunctionEntry>();
		public Dictionary<uint, Action<CpuThreadState>> DelegatesByNID = new Dictionary<uint, Action<CpuThreadState>>();
		public Dictionary<string, Action<CpuThreadState>> DelegatesByName = new Dictionary<string, Action<CpuThreadState>>();
		public string Name { get { return this.GetType().Name; } }

		[Inject]
		protected PspMemory PspMemory;

		public HleModuleHost()
		{
		}

		public void Initialize(PspEmulatorContext PspEmulatorContext)
		{
			//this.PspEmulatorContext = PspEmulatorContext;
			//PspEmulatorContext.InjectDependencesTo(this);

			this.ModuleLocation = "flash0:/kd/" + this.GetType().Namespace.Split('.').Last() + ".prx";
			//Console.WriteLine(this.ModuleLocation);
			//Console.ReadKey();

			//try
			{
				foreach (
					var MethodInfo in
					new MethodInfo[0]
					.Concat(this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
					//.Concat(this.GetType().GetMethods(BindingFlags.NonPublic))
					//.Concat(this.GetType().GetMethods(BindingFlags.Public))
				)
				{
					var Attributes = MethodInfo.GetCustomAttributes(typeof(HlePspFunctionAttribute), true).Cast<HlePspFunctionAttribute>();
					if (Attributes.Count() > 0)
					{
						if (!MethodInfo.IsPublic)
						{
							throw(new InvalidProgramException("Method " + MethodInfo + " is not public"));
						}
						var Delegate = CreateDelegateForMethodInfo(MethodInfo, Attributes.First());
						DelegatesByName[MethodInfo.Name] = Delegate;
						foreach (var Attribute in Attributes)
						{
							//Console.WriteLine("HleModuleHost: {0}, {1}", "0x%08X".Sprintf(Attribute.NID), MethodInfo.Name);
							DelegatesByNID[Attribute.NID] = Delegate;
							EntriesByNID[Attribute.NID] = new HleFunctionEntry()
							{
								NID = Attribute.NID,
								Name = MethodInfo.Name,
								Description = "",
								Module = this,
								ModuleName = this.Name,
							};
						}
					}
					else
					{
						//Console.WriteLine("HleModuleHost: NO: {0}", MethodInfo.Name);
					}
				}
			}
			//catch (Exception Exception)
			//{
			//	Console.WriteLine(Exception);
			//	throw (Exception);
			//}
		}

		public static string StringFromAddress(CpuThreadState CpuThreadState, uint Address)
		{
			if (Address == 0) return null;
			return PointerUtils.PtrToString((byte*)CpuThreadState.GetMemoryPtr(Address), Encoding.UTF8);
		}

		private struct ParamInfo
		{
			public enum RegisterTypeEnum
			{
				Gpr, Fpr,
			}
			public RegisterTypeEnum RegisterType;
			public int RegisterIndex;
			public Type ParameterType;
			public String ParameterName;
		}
	}
}
