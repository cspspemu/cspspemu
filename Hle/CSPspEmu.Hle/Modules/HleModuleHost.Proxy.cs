﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using SafeILGenerator.Ast.Optimizers;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using SafeILGenerator.Utils;

namespace CSPspEmu.Hle
{
	public unsafe partial class HleModuleHost : HleModule
	{
		[Inject]
		internal HleThreadManager ThreadManager;

		[Inject]
		internal CpuProcessor CpuProcessor;

		[Inject]
		internal HleConfig HleConfig;

		private ILInstanceHolderPoolItem _ThisILInstanceHolder = null;
		private ILInstanceHolderPoolItem ThisILInstanceHolder
		{
			get
			{
				if (_ThisILInstanceHolder == null)
				{
					this._ThisILInstanceHolder = ILInstanceHolder.Alloc(this.GetType(), this);
				}
				return _ThisILInstanceHolder;
			}
		}

		static private readonly AstMipsGenerator ast = AstMipsGenerator.Instance;

		static public TType GetObjectFromPoolHelper<TType>(CpuThreadState CpuThreadState, int Index)
		{
			//Console.Error.WriteLine("GetObjectFromPoolHelper");
			return (TType)CpuThreadState.CpuProcessor.InjectContext.GetInstance<HleUidPoolManager>().Get(typeof(TType), Index);
		}

		static public object GetObjectFromPoolHelper(CpuThreadState CpuThreadState, Type Type, int Index)
		{
			//Console.Error.WriteLine("GetObjectFromPoolHelper");
			return CpuThreadState.CpuProcessor.InjectContext.GetInstance<HleUidPoolManager>().Get(Type, Index);
		}

		static public uint GetOrAllocIndexFromPoolHelper(CpuThreadState CpuThreadState, Type Type, IHleUidPoolClass Item)
		{
			//Console.Error.WriteLine("AllocIndexFromPoolHelper");
			return (uint)CpuThreadState.CpuProcessor.InjectContext.GetInstance<HleUidPoolManager>().GetOrAllocIndex(Type, Item);
		}

		private AstNodeStmContainer CreateDelegateForMethodInfoPriv(MethodInfo MethodInfo, HlePspFunctionAttribute HlePspFunctionAttribute, out List<ParamInfo> OutParamInfoList)
		{
			int GprIndex = 4;
			int FprIndex = 0;

			//var SafeILGenerator = MipsMethodEmiter.SafeILGenerator;

			var AstNodes = new AstNodeStmContainer();

			AstNodes.AddStatement(ast.Comment("HleModuleHost.CreateDelegateForMethodInfo(" + MethodInfo + ", " + HlePspFunctionAttribute + ")"));

			var ParamInfoList = new List<ParamInfo>();
			OutParamInfoList = ParamInfoList;

			AstNodeExprCall AstMethodCall;
			{
				//var ModuleObject = this.Cast(this.GetType(), this.FieldAccess(this.Argument<CpuThreadState>(0, "CpuThreadState"), "ModuleObject"));
				//SafeILGenerator.LoadArgument0CpuThreadState();
				//SafeILGenerator.LoadField(typeof(CpuThreadState).GetField("ModuleObject"));
				//SafeILGenerator.CastClass(this.GetType());

				var AstParameters = new List<AstNodeExpr>();

				Action<ParameterInfo, Type> AddGprIndex = (ParameterInfo ParameterInfo, Type ParameterType) =>
				{
					if (ParameterType == null) ParameterType = ParameterInfo.ParameterType;
					ParamInfoList.Add(new ParamInfo()
					{
						ParameterName = ParameterInfo.Name,
						RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
						RegisterIndex = GprIndex,
						ParameterType = ParameterType,
					});
				};

				foreach (var ParameterInfo in MethodInfo.GetParameters())
				{
					var ParameterType = ParameterInfo.ParameterType;

					// The CpuThreadState
					if (ParameterType == typeof(CpuThreadState))
					{
						AstParameters.Add(ast.CpuThreadState);
					}
					// A stringz
					else if (ParameterType == typeof(string))
					{
						AddGprIndex(ParameterInfo, null);

						AstParameters.Add(
							ast.CallStatic(
								(Func<CpuThreadState, uint, string>)HleModuleHost.StringFromAddress,
								ast.CpuThreadState,
								ast.GPR_u(GprIndex)
							)
						);

						GprIndex++;
					}
					// A pointer or ref/out
					else if (ParameterType.IsPointer || ParameterType.IsByRef)
					{
						AddGprIndex(ParameterInfo, typeof(uint));

						var HleInvalidAsNullAttribute = ParameterInfo.GetCustomAttribute<HleInvalidAsNullAttribute>();
						var HleInvalidAsInvalidPointerAttribute = ParameterInfo.GetCustomAttribute<HleInvalidAsInvalidPointerAttribute>();
						InvalidAddressAsEnum InvalidAddressAsEnum = InvalidAddressAsEnum.Exception;
						if (HleInvalidAsNullAttribute != null) InvalidAddressAsEnum = InvalidAddressAsEnum.Null;
						if (HleInvalidAsInvalidPointerAttribute != null) InvalidAddressAsEnum = InvalidAddressAsEnum.InvalidAddress;

						//Console.Error.WriteLine(ParameterInfo.CustomAttributes.Count());

						AstParameters.Add(
							ast.Cast(
								ParameterType,
								ast.MemoryGetPointer(
									CpuProcessor.Memory,
									ast.GPR_u(GprIndex),
									Safe: true,
									ErrorDescription: "Invalid Pointer for Argument '" + ParameterType.Name + " " + ParameterInfo.Name + "'",
									InvalidAddress: InvalidAddressAsEnum
								)
							)
						);

						GprIndex++;
					}
					// A long type
					else if (ParameterType == typeof(long) || ParameterType == typeof(ulong))
					{
						while (GprIndex % 2 != 0) GprIndex++;

						AddGprIndex(ParameterInfo, null);

						if (ParameterType == typeof(ulong))
						{
							AstParameters.Add(ast.GPR_ul(GprIndex + 0));
						}
						else
						{
							AstParameters.Add(ast.GPR_sl(GprIndex + 0));
						}

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

						AstParameters.Add(ast.FPR(FprIndex));

						FprIndex++;
					}
					// PspPointer
					else if (ParameterType == typeof(PspPointer))
					{
						AddGprIndex(ParameterInfo, null);

						AstParameters.Add(ast.CallStatic(
							typeof(PspPointer).GetMethod("op_Implicit", new[] { typeof(uint) }),
							ast.GPR_u(GprIndex)
						));

						GprIndex++;
					}
					// A class
					else if (ParameterType.IsClass)
					{
						AddGprIndex(ParameterInfo, null);

						if (!ParameterType.Implements(typeof(IHleUidPoolClass)))
						{
							throw (new InvalidCastException(String.Format("Can't use a class '{0}' not implementing IHleUidPoolClass as parameter", ParameterType)));
						}

						AstParameters.Add(ast.Cast(ParameterType, ast.CallStatic(
							(Func<CpuThreadState, Type, int, object>)GetObjectFromPoolHelper,
							ast.CpuThreadState,
							ast.Immediate(ParameterType),
							ast.GPR_s(GprIndex)
						)));

						GprIndex++;
					}
					// An integer register
					else
					{
						AddGprIndex(ParameterInfo, null);

						if (ParameterType == typeof(uint))
						{
							AstParameters.Add(ast.Cast(ParameterType, ast.GPR_u(GprIndex)));
						}
						else
						{
							AstParameters.Add(ast.Cast(ParameterType, ast.GPR_s(GprIndex)));
						}

						GprIndex++;
					}
				}

				AstMethodCall = ast.CallInstance(
					ThisILInstanceHolder.GetAstFieldAccess(),
					MethodInfo,
					AstParameters.ToArray()
				);
			}

			if (AstMethodCall.Type == typeof(void)) AstNodes.AddStatement(ast.Statement(AstMethodCall));
			else if (AstMethodCall.Type == typeof(long)) AstNodes.AddStatement(ast.Assign(ast.GPR_l(2), ast.Cast<long>(AstMethodCall)));
			else if (AstMethodCall.Type == typeof(float)) AstNodes.AddStatement(ast.Assign(ast.FPR(0), ast.Cast<float>(AstMethodCall)));
			else if (AstMethodCall.Type.IsClass)
			{
				if (!AstMethodCall.Type.Implements(typeof(IHleUidPoolClass)))
				{
					throw (new InvalidCastException(String.Format("Can't use a class '{0}' not implementing IHleUidPoolClass as return value", AstMethodCall.Type)));
				}
				AstNodes.AddStatement(ast.Assign(
					ast.GPR(2),
					ast.CallStatic(
						(Func<CpuThreadState, Type, IHleUidPoolClass, uint>)GetOrAllocIndexFromPoolHelper,
						ast.CpuThreadState,
						ast.Immediate(AstMethodCall.Type),
						ast.Cast<IHleUidPoolClass>(AstMethodCall)
					)
				));
			}
			else AstNodes.AddStatement(ast.Assign(ast.GPR(2), ast.Cast<uint>(AstMethodCall)));

			return AstNodes;
		}

		private Action<CpuThreadState> CreateDelegateForMethodInfo(MethodInfo MethodInfo, HlePspFunctionAttribute HlePspFunctionAttribute)
		{
			if (!MethodInfo.DeclaringType.IsAssignableFrom(this.GetType()))
			{
				throw (new Exception(String.Format("Invalid {0} != {1}", MethodInfo.DeclaringType, this.GetType())));
			}

			bool SkipLog = HlePspFunctionAttribute.SkipLog;
			var NotImplementedAttribute = (HlePspNotImplementedAttribute)MethodInfo.GetCustomAttributes(typeof(HlePspNotImplementedAttribute), true).FirstOrDefault();
			bool NotImplementedFunc = (NotImplementedAttribute != null) && NotImplementedAttribute.Notice;

			List<ParamInfo> ParamInfoList;
			var AstNodes = AstOptimizerPsp.GlobalOptimize(
				CpuProcessor,
				ast.Statements(
					// Do stuff before
					CreateDelegateForMethodInfoPriv(MethodInfo, HlePspFunctionAttribute, out ParamInfoList)
					// Do stuff after
				)
			);

			var Delegate = AstNodeExtensions._GeneratorILPsp.GenerateDelegate<Action<CpuThreadState>>(
				String.Format("Proxy_{0}_{1}", this.GetType().Name, MethodInfo.Name),
				AstNodes
			);

			//return Delegate;

			return (CpuThreadState) =>
			{
				bool Trace = (!SkipLog && CpuThreadState.CpuProcessor.CpuConfig.DebugSyscalls);
				bool NotImplemented = NotImplementedFunc && HleConfig.DebugNotImplemented;

				if (Trace && (MethodInfo.DeclaringType.Name == "Kernel_Library")) Trace = false;

				//Console.WriteLine("aaaaaaaaaaaaa");

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

				try
				{
					CpuThreadState.PC = CpuThreadState.RA;
					Delegate(CpuThreadState);
				}
				catch (InvalidProgramException)
				{
					Console.WriteLine("CALLING: {0}", MethodInfo);
					Console.WriteLine("{0}", (new GeneratorCSharp()).GenerateRoot(AstNodes).ToString());

					foreach (var Line in AstNodeExtensions._GeneratorILPsp.GenerateToStringList(MethodInfo, AstNodes))
					{
						Console.WriteLine(Line);
					}

					throw;
				}
				catch (MemoryPartitionNoMemoryException)
				{
					CpuThreadState.GPR[2] = (int)SceKernelErrors.ERROR_ERRNO_NO_MEMORY;
				}
				catch (SceKernelException SceKernelException)
				{
					CpuThreadState.GPR[2] = (int)SceKernelException.SceKernelError;
				}
				catch (SceKernelSelfStopUnloadModuleException)
				{
					throw;
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