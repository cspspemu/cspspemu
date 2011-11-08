using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using CSharpUtils;

namespace CSPspEmu.Hle
{
	unsafe public class HleModuleHost : HleModule
	{
		public HleState HleState;
		public Dictionary<uint, String> NamesByNID = new Dictionary<uint, String>();
		public Dictionary<uint, Action<CpuThreadState>> DelegatesByNID = new Dictionary<uint, Action<CpuThreadState>>();
		public Dictionary<string, Action<CpuThreadState>> DelegatesByName = new Dictionary<string, Action<CpuThreadState>>();

		public HleModuleHost()
		{
		}

		public void Initialize(HleState HleState)
		{
			this.HleState = HleState;

			foreach (var MethodInfo in this.GetType().GetMethods())
			{
				var Attributes = MethodInfo.GetCustomAttributes(typeof(HlePspFunctionAttribute), true).Cast<HlePspFunctionAttribute>();
				if (Attributes.Count() > 0)
				{
					var Delegate = CreateDelegateForMethodInfo(MethodInfo);
					DelegatesByName[MethodInfo.Name] = Delegate;
					foreach (var Attribute in Attributes)
					{
						//Console.WriteLine(Attribute.NID);
						DelegatesByNID[Attribute.NID] = Delegate;
						NamesByNID[Attribute.NID] = MethodInfo.Name;
					}
				}
			}
		}

		static public string StringFromAddress(CpuThreadState CpuThreadState, uint Address)
		{
			if (Address == 0) return null;
			return PointerUtils.PtrToString((byte*)CpuThreadState.GetMemoryPtr(Address), Encoding.UTF8);
		}

		private Action<CpuThreadState> CreateDelegateForMethodInfo(MethodInfo MethodInfo)
		{
			var MipsMethodEmiter = new MipsMethodEmiter(HleState.MipsEmiter, HleState.Processor);
			int GprIndex = 4;

			Action CallAction = () =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldfld, typeof(CpuThreadState).GetField("ModuleObject"));
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Castclass, this.GetType());
				foreach (var ParameterInfo in MethodInfo.GetParameters())
				{
					var ParameterType = ParameterInfo.ParameterType;
					if (ParameterType == typeof(CpuThreadState))
					{
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
					}
					else if (ParameterType == typeof(string))
					{
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
						MipsMethodEmiter.LoadGPR(GprIndex);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(HleModuleHost).GetMethod("StringFromAddress"));
						GprIndex++;
					}
					else if (ParameterType.IsPointer)
					{
						MipsMethodEmiter._getmemptr(() =>
						{
							MipsMethodEmiter.LoadGPR(GprIndex);
						});
						GprIndex++;
					}
					else
					{
						MipsMethodEmiter.LoadGPR(GprIndex);
						GprIndex++;
					}
					//MipsMethodEmiter.ILGenerator.Emit(OpCodes.ld
				}
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, MethodInfo);
			};

			if (MethodInfo.ReturnType == typeof(void))
			{
				CallAction();
			}
			else if (MethodInfo.ReturnType == typeof(long))
			{
				throw(new NotImplementedException());
			}
			else
			{
				MipsMethodEmiter.SaveGPR(2, CallAction);
			}

			var Delegate = MipsMethodEmiter.CreateDelegate();
			return (CpuThreadState) =>
			{
				CpuThreadState.ModuleObject = this;
				Delegate(CpuThreadState);
			};
		}
	}
}
