#define USE_DYNAMIC_METHOD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Reflection;
using CSPspEmu.Core.Memory;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emiter
{
	/// <summary>
	/// <see cref="http://www.mrc.uidaho.edu/mrc/people/jff/digital/MIPSir.html"/>
	/// </summary>
	unsafe public class MipsMethodEmiter
	{
#if USE_DYNAMIC_METHOD
		//protected DynamicMethod DynamicMethod;
		public DynamicMethod DynamicMethod;
#else
		public TypeBuilder TypeBuilder;
		protected MethodBuilder MethodBuilder;
#endif
		public SafeILGeneratorEx SafeILGenerator;
		protected String MethodName;
		//public CpuThreadState CpuThreadState;
		public CpuProcessor Processor;
		
		//static protected FieldInfo Field_GPR_Ptr = typeof(Processor).GetField("GPR_Ptr");
		//static protected FieldInfo Field_FPR_Ptr = typeof(Processor).GetField("FPR_Ptr");
		static protected FieldInfo Field_BranchFlag = typeof(CpuThreadState).GetField("BranchFlag");
		static protected FieldInfo Field_PC = typeof(CpuThreadState).GetField("PC");
		static protected FieldInfo Field_LO = typeof(CpuThreadState).GetField("LO");
		static protected FieldInfo Field_HI = typeof(CpuThreadState).GetField("HI");
		static protected FieldInfo Field_StepInstructionCount = typeof(CpuThreadState).GetField("StepInstructionCount");
		static private ulong UniqueCounter = 0;

		static protected bool InitializedOnce = false;
		static protected FieldInfo[] Field_GPRList;
		static protected FieldInfo[] Field_FPRList;

		readonly public Dictionary<string, uint> InstructionStats = new Dictionary<string, uint>();

		/*
		static public MipsMethodEmiter()
		{
		}
		*/

		public void _getmemptr(Action Action, bool Safe = false, String ErrorDescription = "", bool CanBeNull = true)
		{
			if (Safe)
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				{
					Action();
				}
				//ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("GetMemoryPtrSafe"));

				SafeILGenerator.Push((string)ErrorDescription);
				SafeILGenerator.Push((int)(CanBeNull ? 1: 0));
				SafeILGenerator.Call((GetMemoryPtrSafeWithErrorDelegate)CpuThreadState.Methods.GetMemoryPtrSafeWithError);
			}
			else if (Processor.Memory is FastPspMemory)
			{
				SafeILGenerator.Push((int)((FastPspMemory)Processor.Memory).Base);
				{
					Action();
				}
				SafeILGenerator.Push((int)PspMemory.MemoryMask);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			}
			else
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				{
					Action();
				}
#if true
				//ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("GetMemoryPtr"));
				SafeILGenerator.Call((GetMemoryPtrNotNullDelegate)CpuThreadState.Methods.GetMemoryPtrNotNull);
#else
				ILGenerator.Emit(OpCodes.Ldstr, ErrorDescription);
				ILGenerator.Emit(OpCodes.Ldc_I4, CanBeNull ? 1 : 0);
				ILGenerator.Emit(OpCodes.Call, CpuThreadState.GetMemoryPtrSafeWithError);
#endif
			}
		}

		public MipsMethodEmiter(MipsEmiter MipsEmiter, CpuProcessor Processor, uint PC)
		{
			this.Processor = Processor;

			if (!InitializedOnce)
			{
				Field_GPRList = new FieldInfo[32];
				Field_FPRList = new FieldInfo[32];
				for (int n = 0; n < 32; n++)
				{
					Field_GPRList[n] = typeof(CpuThreadState).GetField("GPR" + n);
					Field_FPRList[n] = typeof(CpuThreadState).GetField("FPR" + n);
				}

				InitializedOnce = true;
			}

			UniqueCounter++;

#if USE_DYNAMIC_METHOD
			DynamicMethod = new DynamicMethod(
				String.Format("DynamicMethod_0x{0:X}", PC),
				typeof(void),
				new Type[] { typeof(CpuThreadState) },
				Assembly.GetExecutingAssembly().ManifestModule
			);
			SafeILGenerator = new SafeILGeneratorEx(
				DynamicMethod.GetILGenerator(),
				CheckTypes: false,
#if LOG_TRACE
				DoDebug: true,
				DoLog: true
#else
				DoDebug: false,
				DoLog: false
#endif
			);
#else
			TypeBuilder = MipsEmiter.ModuleBuilder.DefineType("type" + UniqueCounter, TypeAttributes.Sealed | TypeAttributes.Public);
			MethodBuilder = TypeBuilder.DefineMethod(
				name: MethodName = "method" + UniqueCounter,
				attributes: MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.UnmanagedExport | MethodAttributes.Final,
				returnType: typeof(void),
				parameterTypes: new Type[] { typeof(CpuThreadState) }
			);
			ILGenerator = MethodBuilder.GetILGenerator();
#endif
		}

		public Action<CpuThreadState> CreateDelegate()
		{
			SafeILGenerator.Return();

			try
			{
#if USE_DYNAMIC_METHOD
				return (Action<CpuThreadState>)DynamicMethod.CreateDelegate(typeof(Action<CpuThreadState>));
#else
			var Type = TypeBuilder.CreateType();
			return (Action<CpuThreadState>)Delegate.CreateDelegate(typeof(Action<CpuThreadState>), Type.GetMethod(MethodName));
#endif
			}
			catch (InvalidProgramException InvalidProgramException)
			{
#if LOG_TRACE
				Console.WriteLine("Invalid Delegate...");
				foreach (var Line in SafeILGenerator.GetEmittedInstructions())
				{
					Console.WriteLine("  ## {0}", Line);
				}
#endif
				throw (InvalidProgramException);
			}
		}

		MethodBody GetMethodBody()
		{
			return DynamicMethod.GetMethodBody();
		}

		public void LoadFieldPtr(FieldInfo FieldInfo)
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			if (FieldInfo == null) throw (new InvalidCastException("FieldInfo == null"));
			if (FieldInfo.DeclaringType != typeof(CpuThreadState)) throw (new InvalidCastException("FieldInfo.DeclaringType != CpuThreadState"));
			SafeILGenerator.LoadFieldAddress(FieldInfo);
		}

		protected void LoadGPR_Ptr(int R) { LoadFieldPtr(Field_GPRList[R]); }
		protected void LoadFPR_Ptr(int R) { LoadFieldPtr(Field_FPRList[R]); }
		protected void LoadPC_Ptr() { LoadFieldPtr(Field_PC); }
		protected void LoadHI_Ptr() { LoadFieldPtr(Field_HI); }
		protected void LoadLO_Ptr() { LoadFieldPtr(Field_LO); }
		protected void LoadStepInstructionCountPtr() {
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.LoadFieldAddress(Field_StepInstructionCount);
		}

		public void LoadStepInstructionCount()
		{
			LoadStepInstructionCountPtr();
			SafeILGenerator.LoadIndirect<int>();
		}

		public void SaveStepInstructionCount(Action Action)
		{
			LoadStepInstructionCountPtr();
			{
				Action();
			}
			SafeILGenerator.StoreIndirect<int>();
		}

		public void LoadBranchFlag()
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.LoadField(Field_BranchFlag);
		}

		public void StoreBranchFlag(Action Action)
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			Action();
			SafeILGenerator.StoreField(Field_BranchFlag);
		}

		public void SaveField<TType>(FieldInfo FieldInfo, Action Action)
		{
			LoadFieldPtr(FieldInfo);
			Action();
			SafeILGenerator.StoreIndirect<TType>();
		}

		public void SavePC(Action Action) { SaveField<int>(Field_PC, Action); }
		public void SavePC(uint PC) { SaveField<int>(Field_PC, () => { SafeILGenerator.Push((int)PC); }); }
		public void SaveLO(Action Action) { SaveField<int>(Field_LO, Action); }
		public void SaveHI(Action Action) { SaveField<int>(Field_HI, Action); }

		public void SaveHI_LO(Action Action) {
			LoadFieldPtr(Field_LO);
			Action();
			SafeILGenerator.StoreIndirect<long>();
		}

		public void LoadHI_LO()
		{
			LoadFieldPtr(Field_LO);
			SafeILGenerator.LoadIndirect<long>();
		}

		public void SaveGPR_F(int R, Action Action) { if (R != 0) SaveField<float>(Field_GPRList[R], Action); }
		public void SaveGPR(int R, Action Action) { if (R != 0) SaveField<int>(Field_GPRList[R], Action); }
		public void SaveGPRLong(int R, Action Action) { if (R != 0) SaveField<long>(Field_GPRList[R], Action); }
		public void SaveFPR(int R, Action Action)
		{
			LoadFPR_Ptr(R);
			{
				Action();
			}
			SafeILGenerator.StoreIndirect<float>();
		}
		public void SaveFPR_I(int R, Action Action)
		{
			LoadFPR_Ptr(R);
			{
				Action();
			}
			SafeILGenerator.StoreIndirect<int>();
		}

		static public bool _LoadFcr31CC(CpuThreadState CpuThreadState)
		{
			return CpuThreadState.Fcr31.CC;
		}

		static public void _SaveFcr31CC(CpuThreadState CpuThreadState, bool Value)
		{
			CpuThreadState.Fcr31.CC = Value;
		}

		public void LoadFCR31_CC()
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Call((Func<CpuThreadState, bool>)MipsMethodEmiter._LoadFcr31CC);
		}

		public void SaveFCR31_CC(Action Action)
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			Action();
			SafeILGenerator.Call((Action<CpuThreadState, bool>)MipsMethodEmiter._SaveFcr31CC);
		}

		public void LoadGPR_Signed(int R)
		{
			if (R == 0)
			{
				SafeILGenerator.Push((int)0);
			}
			else
			{
				LoadGPR_Ptr(R);
				SafeILGenerator.LoadIndirect<int>();
			}
		}

		public void LoadGPR_Unsigned(int R)
		{
			if (R == 0)
			{
				SafeILGenerator.Push((int)0);
				SafeILGenerator.ConvertTo<uint>();
			}
			else
			{
				LoadGPR_Ptr(R);
				SafeILGenerator.LoadIndirect<uint>();
			}
		}

		public void LoadGPRLong_Signed(int R) { LoadGPR_Ptr(R); SafeILGenerator.LoadIndirect<long>(); }
		public void LoadFPR(int R) { LoadFPR_Ptr(R); SafeILGenerator.LoadIndirect<float>(); }
		public void LoadFPR_I(int R) { LoadFPR_Ptr(R); SafeILGenerator.LoadIndirect<uint>(); }
		public void LoadPC() { LoadPC_Ptr(); SafeILGenerator.LoadIndirect<uint>(); }
		public void LoadLO() { LoadLO_Ptr(); SafeILGenerator.LoadIndirect<uint>(); }
		public void LoadHI() { LoadHI_Ptr(); SafeILGenerator.LoadIndirect<uint>(); }

		public void OP_3REG_Unsigned(int RD, int RS, int RT, Action Action)
		{
			SaveGPR(RD, () =>
			{
				LoadGPR_Unsigned(RS);
				LoadGPR_Unsigned(RT);
				Action();
			});
		}

		public void OP_3REG_Signed(int RD, int RS, int RT, Action Action)
		{
			SaveGPR(RD, () =>
			{
				LoadGPR_Signed(RS);
				LoadGPR_Signed(RT);
				Action();
			});
		}

		public void OP_3REG_F(int FD, int FS, int FT, Action Action)
		{
			SaveFPR(FD, () =>
			{
				LoadFPR(FS);
				LoadFPR(FT);
				Action();
			});
		}

		public void OP_2REG_IMM_F(int FD, int FS, float Immediate, Action Action)
		{
			SaveFPR(FD, () =>
			{
				LoadFPR(FS);
				SafeILGenerator.Push((float)Immediate);
				Action();
			});
		}

		public void OP_2REG_F(int FD, int FS, Action Action)
		{
			SaveFPR(FD, () =>
			{
				LoadFPR(FS);
				Action();
			});
		}

		public void OP_2REG_IMM_Signed(int RT, int RS, short Immediate, Action Action)
		{
			SaveGPR(RT, () =>
			{
				LoadGPR_Unsigned(RS);
				SafeILGenerator.ConvertTo<int>();
				SafeILGenerator.Push((int)Immediate);
				SafeILGenerator.ConvertTo<int>();
				Action();
			});
		}

		public void OP_2REG_IMM_Unsigned(int RT, int RS, uint Immediate, Action Action)
		{
			SaveGPR(RT, () =>
			{
				LoadGPR_Unsigned(RS);
				SafeILGenerator.ConvertTo<uint>();
				SafeILGenerator.Push((int)Immediate);
				SafeILGenerator.ConvertTo<uint>();
				Action();
			});
		}

		public void SET(int RT, uint Value)
		{
			SaveGPR(RT, () =>
			{
				SafeILGenerator.Push((int)Value);
			});
		}

		public void SET_REG(int RT, int RS)
		{
			SaveGPR(RT, () => { LoadGPR_Unsigned(RS); });
		}

		public void CallMethod(MethodInfo MethodInfo)
		{
			SafeILGenerator.Call(MethodInfo);
		}

		public void CallMethod(Delegate Delegate)
		{
			CallMethod(Delegate.Method);
		}

		private void CallMethod(Type Class, String MethodName)
		{
			var MethodInfo = Class.GetMethod(MethodName);
			if (MethodInfo == null) throw (new KeyNotFoundException(String.Format("Can't find {0}::{1}", Class, MethodName)));
			CallMethod(MethodInfo);
		}

		public void CallMethodWithCpuThreadStateAsFirstArgument(Type Class, String MethodName)
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			CallMethod(Class, MethodName);
		}
	}
}
