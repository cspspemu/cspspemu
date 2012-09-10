using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emitter;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public unsafe partial class HleModuleHost : HleModule
	{
		[Inject]
		internal HleThreadManager ThreadManager;

		[Inject]
		internal CpuProcessor CpuProcessor;

		private Action<CpuThreadState> CreateDelegateForMethodInfo(MethodInfo MethodInfo, HlePspFunctionAttribute HlePspFunctionAttribute)
		{
			var MipsMethodEmiter = new MipsMethodEmitter(CpuProcessor, 0);
			int GprIndex = 4;
			int FprIndex = 0;

			var NotImplementedAttribute = (HlePspNotImplementedAttribute)MethodInfo.GetCustomAttributes(typeof(HlePspNotImplementedAttribute), true).FirstOrDefault();
			bool NotImplementedFunc = (NotImplementedAttribute != null) && NotImplementedAttribute.Notice;
			bool SkipLog = HlePspFunctionAttribute.SkipLog;
			var SafeILGenerator = MipsMethodEmiter.SafeILGenerator;
			SafeILGenerator.Comment("HleModuleHost.CreateDelegateForMethodInfo(" + MethodInfo + ", " + HlePspFunctionAttribute + ")");

			var ParamInfoList = new List<ParamInfo>();

			Action CallAction = () =>
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				SafeILGenerator.LoadField(typeof(CpuThreadState).GetField("ModuleObject"));
				SafeILGenerator.CastClass(this.GetType());
				foreach (var ParameterInfo in MethodInfo.GetParameters())
				{
					var ParameterType = ParameterInfo.ParameterType;

					// The CpuThreadState
					if (ParameterType == typeof(CpuThreadState))
					{
						SafeILGenerator.LoadArgument0CpuThreadState();
					}
					// A stringz
					else if (ParameterType == typeof(string))
					{
						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
							RegisterIndex = GprIndex,
							ParameterType = ParameterType,
						});
						SafeILGenerator.LoadArgument0CpuThreadState();
						MipsMethodEmiter.LoadGPR_Unsigned(GprIndex);
						SafeILGenerator.Call(typeof(HleModuleHost).GetMethod("StringFromAddress"));
						GprIndex++;
					}
					// A pointer or ref/out
					else if (ParameterType.IsPointer || ParameterType.IsByRef)
					{
						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
							RegisterIndex = GprIndex,
							ParameterType = typeof(uint),
						});
						MipsMethodEmiter._getmemptr(() =>
						{
							MipsMethodEmiter.LoadGPR_Unsigned(GprIndex);
						}, Safe: true, ErrorDescription: "Invalid Pointer for Argument '" + ParameterType.Name + " " + ParameterInfo.Name + "'");
						GprIndex++;
					}
					/*
					// An array
					else if (ParameterType.IsArray)
					{
						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
							RegisterIndex = GprIndex,
							ParameterType = typeof(uint),
						});
						// Pointer
						MipsMethodEmiter._getmemptr(() =>
						{
							MipsMethodEmiter.LoadGPR_Unsigned(GprIndex);
						}, Safe: true, ErrorDescription: "Invalid Pointer for Argument '" + ParameterType.Name + " " + ParameterInfo.Name + "'");
						GprIndex++;
						// Array
						MipsMethodEmiter.LoadGPR_Unsigned(GprIndex);
						GprIndex++;
						MipsMethodEmiter.CallMethod(HleModuleHost.PointerLengthToArrat);
					}
					*/
					// A long type
					else if (ParameterType == typeof(long) || ParameterType == typeof(ulong))
					{
						while (GprIndex % 2 != 0) GprIndex++;

						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
							RegisterIndex = GprIndex,
							ParameterType = ParameterType,
						});


						MipsMethodEmiter.LoadGPRLong_Signed(GprIndex + 0);
						/*
						MipsMethodEmiter.LoadGPR_Unsigned(GprIndex + 0);
						MipsMethodEmiter.LoadGPR_Unsigned(GprIndex + 1);
						SafeILGenerator.Push((int)32);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shl);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
						*/
						GprIndex += 2;
					}
					// A float register.
					else if (ParameterType == typeof(float))
					{
						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Fpr,
							RegisterIndex = FprIndex,
							ParameterType = ParameterType,
						});

						MipsMethodEmiter.LoadFPR(FprIndex);
						FprIndex++;
					}
					// Test
					else if (ParameterType == typeof(PspPointer))
					{
						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
							RegisterIndex = GprIndex,
							ParameterType = ParameterType,
						});

						MipsMethodEmiter.LoadGPR_Unsigned(GprIndex);
						MipsMethodEmiter.CallMethod(typeof(PspPointer).GetMethod("op_Implicit", new[] { typeof(uint) }));
						GprIndex++;
					}
					// An integer register
					else
					{
						ParamInfoList.Add(new ParamInfo()
						{
							ParameterName = ParameterInfo.Name,
							RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
							RegisterIndex = GprIndex,
							ParameterType = ParameterType,
						});

						MipsMethodEmiter.LoadGPR_Unsigned(GprIndex);
						GprIndex++;
					}
					//MipsMethodEmiter.ILGenerator.Emit(OpCodes.ld
				}
				SafeILGenerator.Call(MethodInfo);
			};

			if (MethodInfo.ReturnType == typeof(void))
			{
				CallAction();
			}
			else if (MethodInfo.ReturnType == typeof(long))
			{
				MipsMethodEmiter.SaveGPRLong(2, CallAction);
			}
			else if (MethodInfo.ReturnType == typeof(float))
			{
				MipsMethodEmiter.SaveFPR(0, CallAction);
			}
			else
			{
				MipsMethodEmiter.SaveGPR(2, CallAction);
			}

			var Delegate = MipsMethodEmiter.CreateDelegate();
			return (CpuThreadState) =>
			{
				bool Trace = (!SkipLog && CpuThreadState.CpuProcessor.PspConfig.DebugSyscalls);
				bool NotImplemented = NotImplementedFunc && CpuThreadState.CpuProcessor.PspConfig.DebugNotImplemented;

				if (Trace && (MethodInfo.DeclaringType.Name == "Kernel_Library")) Trace = false;

				if (NotImplemented)
				{
					Trace = true;
					ConsoleUtils.SaveRestoreConsoleState(() =>
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine(
							"Not implemented {0}.{1}",
							MethodInfo.DeclaringType.Name, MethodInfo.Name
						);
					});
				}

				var Out = Console.Out;
				if (NotImplemented)
				{
					Out = Console.Error;
				}

				if (Trace)
				{
					if (ThreadManager.Current != null)
					{
						Out.Write(
							"Thread({0}:'{1}') : RA(0x{2:X})",
							ThreadManager.Current.Id,
							ThreadManager.Current.Name,
							ThreadManager.Current.CpuThreadState.RA
						);
					}
					else
					{
						Out.Write("NoThread:");
					}
					Out.Write(" : {0}.{1}", MethodInfo.DeclaringType.Name, MethodInfo.Name);
					Out.Write("(");
					int Count = 0;
					foreach (var ParamInfo in ParamInfoList)
					{
						if (Count > 0) Out.Write(", ");
						Out.Write("{0}:", ParamInfo.ParameterName);
						switch (ParamInfo.RegisterType)
						{
							case HleModuleHost.ParamInfo.RegisterTypeEnum.Fpr:
							case HleModuleHost.ParamInfo.RegisterTypeEnum.Gpr:
								uint Int4 = (uint)CpuThreadState.GPR[ParamInfo.RegisterIndex];
								uint Float4 = (uint)CpuThreadState.FPR[ParamInfo.RegisterIndex];
								Out.Write("{0}", ToNormalizedTypeString(ParamInfo.ParameterType, CpuThreadState, Int4, Float4));
								break;
							default:
								throw (new NotImplementedException());
						}
						Count++;
					}
					Out.Write(")");
					//Console.WriteLine("");
				}

				CpuThreadState.ModuleObject = this;
				try
				{
					Delegate(CpuThreadState);
				}
				catch (MemoryPartitionNoMemoryException)
				{
					CpuThreadState.GPR[2] = (int)SceKernelErrors.ERROR_ERRNO_NO_MEMORY;
				}
				catch (SceKernelException SceKernelException)
				{
					CpuThreadState.GPR[2] = (int)SceKernelException.SceKernelError;
				}
				catch (SceKernelSelfStopUnloadModuleException SceKernelSelfStopUnloadModuleException)
				{
					throw (SceKernelSelfStopUnloadModuleException);
				}
#if !DO_NOT_PROPAGATE_EXCEPTIONS
				catch (Exception Exception)
				{
					throw (new Exception(
						String.Format("ERROR calling {0}.{1}!", MethodInfo.DeclaringType.Name, MethodInfo.Name),
						Exception
					));
				}
#endif
				finally
				{
					if (Trace)
					{
						Out.WriteLine(" : {0}", ToNormalizedTypeString(MethodInfo.ReturnType, CpuThreadState, (uint)CpuThreadState.GPR[2], (float)CpuThreadState.FPR[0]));
						Out.WriteLine("");
					}
				}
			};
		}

		public static string ToNormalizedTypeString(Type ParameterType, CpuThreadState CpuThreadState, uint Int4, float Float4)
		{
			if (ParameterType == typeof(void))
			{
				return "void";
			}

			if (ParameterType == typeof(string))
			{
				return String.Format("'{0}'", StringFromAddress(CpuThreadState, Int4));
			}

			if (ParameterType == typeof(int))
			{
				return String.Format("{0}", (int)Int4);
			}

			if (ParameterType.IsEnum)
			{
				var Name = ParameterType.GetEnumName(Int4);
				if (string.IsNullOrEmpty(Name)) Name = Int4.ToString();
				return Name;
			}

			if (ParameterType.IsPointer)
			{
				try
				{
					return "0x%08X".Sprintf(CpuThreadState.CpuProcessor.Memory.PointerToPspAddressUnsafe((void*)Int4));
				}
				catch (Exception)
				{
					return String.Format("0x{0:X}", CpuThreadState.CpuProcessor.Memory.PointerToPspAddressUnsafe((void*)Int4));
				}
			}

			if (ParameterType == typeof(float))
			{
				return String.Format("{0}", Float4);
			}

			try
			{
				return "0x%08X".Sprintf(Int4);
			}
			catch (Exception)
			{
				return String.Format("0x{0:X}", Int4);
			}
		}
	}
}
