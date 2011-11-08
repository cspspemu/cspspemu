using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle
{
	unsafe public class HlePspHleModule
	{
		public HleState HleState;
		public Dictionary<uint, Action<CpuThreadState>> DelegatesByNID = new Dictionary<uint, Action<CpuThreadState>>();
		public Dictionary<string, Action<CpuThreadState>> DelegatesByName = new Dictionary<string, Action<CpuThreadState>>();

		public HlePspHleModule()
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
					}
				}
			}
		}

		static public string StringFromAddress(CpuThreadState CpuThreadState, uint Address)
		{
			if (Address == 0) return null;
			var Bytes = new List<byte>();
			for (var Ptr = (byte*)CpuThreadState.GetMemoryPtr(Address); *Ptr != 0; Ptr++)
			{
				Bytes.Add(*Ptr);
			}
			return Encoding.UTF8.GetString(Bytes.ToArray());
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
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(HlePspHleModule).GetMethod("StringFromAddress"));
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

			/*
			int GprIndex = 4;
			var ParameterTypes = MethodInfo.GetParameters().Select(ParameterInfo => ParameterInfo.ParameterType).ToArray();
			int ParametersCount = ParameterTypes.Length;
			object[] Parameters = new object[ParametersCount];
			int[] ParameterIndex = new int[ParametersCount];
			for (int n = 0; n < ParametersCount; n++)
			{
				ParameterIndex[n] = GprIndex;
				GprIndex++;
			}

			return (CpuThreadState) => 
			{
				for (int n = 0; n < ParametersCount; n++)
				{
					Parameters[n] = CpuThreadState.GPR[ParameterIndex[n]];
					if (ParameterTypes[n] == typeof(int)) {
						Parameters[n] = (int)Parameters[n];
					} else if (ParameterTypes[n] == typeof(uint)) {
						Parameters[n] = (uint)Parameters[n];
					}
				}
				var ReturnValue = MethodInfo.Invoke(this, Parameters);
				if (MethodInfo.ReturnType != typeof(void))
				{
					if (MethodInfo.ReturnType == typeof(long))
					{
						CpuThreadState.GPR[2] = (int)(((uint)ReturnValue >> 0) & 0xFFFFFFFF);
						CpuThreadState.GPR[3] = (int)(((uint)ReturnValue >> 32) & 0xFFFFFFFF);
					}
					else
					{
						CpuThreadState.GPR[2] = (int)ReturnValue;
					}
				}
			};
			*/
		}

		/*
		public Action<CpuThreadState> CreateMethodForNid(uint NID)
		{
			throw (new NotImplementedException());
		}

		public Action<CpuThreadState> CreateMethodForMethodName(string MethodName)
		{
			return CreateMethodForMethodInfo(this.GetType().GetMethod(MethodName));
		}

		public Action<CpuThreadState> CreateMethodForMethodInfo(MethodInfo MethodInfo)
		{
			if (MethodInfo == null)
			{
				throw(new ArgumentNullException());
			}
			throw (new NotImplementedException());
		}
		*/
	}
}
