//#define DEBUG_GENERATE_IL
#define USE_DYNAMIC_METHOD

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using CSPspEmu.Core.Memory;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Optimizers;

namespace CSPspEmu.Core.Cpu.Emitter
{
	/// <summary>
	/// <see cref="http://www.mrc.uidaho.edu/mrc/people/jff/digital/MIPSir.html"/>
	/// </summary>
	public unsafe class MipsMethodEmitter : IAstGenerator
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
		
		//protected static FieldInfo Field_GPR_Ptr = typeof(Processor).GetField("GPR_Ptr");
		//protected static FieldInfo Field_FPR_Ptr = typeof(Processor).GetField("FPR_Ptr");
		protected static FieldInfo Field_BranchFlag = typeof(CpuThreadState).GetField("BranchFlag");
		protected static FieldInfo Field_PC = typeof(CpuThreadState).GetField("PC");
		protected static FieldInfo Field_LO = typeof(CpuThreadState).GetField("LO");
		protected static FieldInfo Field_HI = typeof(CpuThreadState).GetField("HI");
		protected static FieldInfo Field_StepInstructionCount = typeof(CpuThreadState).GetField("StepInstructionCount");
		private static ulong UniqueCounter = 0;

		protected static bool InitializedOnce = false;
		protected static FieldInfo[] Field_GPRList;
		protected static FieldInfo[] Field_C0RList;
		protected static FieldInfo[] Field_FPRList;

		public readonly Dictionary<string, uint> InstructionStats = new Dictionary<string, uint>();

		/*
		public static MipsMethodEmiter()
		{
		}
		*/

		public void _savetoaddress<TType>(Action ActionAddress, Action ActionValue, bool Safe = false, String ErrorDescription = "", bool CanBeNull = true)
		{
			if (Safe || !(Processor.Memory is FastPspMemory))
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				ActionAddress();
				ActionValue();
				switch (Marshal.SizeOf(typeof(TType)))
				{
					case 1: SafeILGenerator.Call((Action<uint, byte>)CpuThreadState.Methods.Write1); break;
					case 2: SafeILGenerator.Call((Action<uint, ushort>)CpuThreadState.Methods.Write2); break;
					case 4: SafeILGenerator.Call((Action<uint, uint>)CpuThreadState.Methods.Write4); break;
					default: throw (new NotImplementedException());
				}
			}
			else
			{
				_getmemptr(ActionAddress, Safe, ErrorDescription, CanBeNull);
				ActionValue();
				SafeILGenerator.StoreIndirect<TType>();
			}
		}

		public void _loadfromaddress<TType>(Action ActionAddress, bool Safe = false, String ErrorDescription = "", bool CanBeNull = true)
		{
			if (Safe || !(Processor.Memory is FastPspMemory))
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				ActionAddress();
				switch (Marshal.SizeOf(typeof(TType)))
				{
					case 1: SafeILGenerator.Call((Func<uint, byte>)CpuThreadState.Methods.Read1); break;
					case 2: SafeILGenerator.Call((Func<uint, ushort>)CpuThreadState.Methods.Read2); break;
					case 4: SafeILGenerator.Call((Func<uint, uint>)CpuThreadState.Methods.Read4); break;
					default: throw(new NotImplementedException());
				}
			}
			else
			{
				_getmemptr(ActionAddress, Safe, ErrorDescription, CanBeNull);
				SafeILGenerator.LoadIndirect<TType>();
			}
		}

		public void _getmemptr(Action Action, bool Safe = false, String ErrorDescription = "", bool CanBeNull = true)
		{
			if (Safe || !(Processor.Memory is FastPspMemory))
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				{
					Action();
				}
				//ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("GetMemoryPtrSafe"));

				if (Safe)
				{
					SafeILGenerator.Push((string)ErrorDescription);
					SafeILGenerator.Push((int)(CanBeNull ? 1 : 0));
					SafeILGenerator.Call((GetMemoryPtrSafeWithErrorDelegate)CpuThreadState.Methods.GetMemoryPtrSafeWithError);
				}
				else
				{
					//ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("GetMemoryPtr"));
					SafeILGenerator.Call((GetMemoryPtrNotNullDelegate)CpuThreadState.Methods.GetMemoryPtrNotNull);
				}
			}
			else
			{
				var Base = ((FastPspMemory)Processor.Memory).Base;

				if (Platform.Is32Bit)
				{
					SafeILGenerator.Push((int)Base);
				}
				else
				{
					SafeILGenerator.Push((long)Base);
				}
				{
					Action();
				}
				SafeILGenerator.Push((int)PspMemory.MemoryMask);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
				if (!Platform.Is32Bit) SafeILGenerator.ConvertTo<long>();
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			}
		}

		public MipsMethodEmitter(CpuProcessor Processor, uint PC, bool DoDebug = false, bool DoLog = false)
		{
			this.Processor = Processor;

			if (!InitializedOnce)
			{
				Field_GPRList = new FieldInfo[32];
				Field_C0RList = new FieldInfo[32];
				Field_FPRList = new FieldInfo[32];
				for (int n = 0; n < 32; n++)
				{
					Field_GPRList[n] = typeof(CpuThreadState).GetField("GPR" + n);
					Field_C0RList[n] = typeof(CpuThreadState).GetField("C0R" + n);
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
				DoDebug: false,
				DoLog: true
#else
				DoDebug: DoDebug,
				DoLog: DoLog
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
			SafeILGenerator.Return(typeof(void));

			try
			{
#if USE_DYNAMIC_METHOD
				var Method = (Action<CpuThreadState>)DynamicMethod.CreateDelegate(typeof(Action<CpuThreadState>));
#else
				var Type = TypeBuilder.CreateType();
				var Method = (Action<CpuThreadState>)Delegate.CreateDelegate(typeof(Action<CpuThreadState>), Type.GetMethod(MethodName));
#endif
				//Console.WriteLine(Method.Method.);
				if (Platform.IsMono)
				{
					Marshal.Prelink(Method.Method);
				}

				return Method;
			}
			catch (InvalidProgramException InvalidProgramException)
			{
#if LOG_TRACE
				Console.WriteLine("Invalid Delegate:");
				foreach (var Line in SafeILGenerator.GetEmittedInstructions())
				{
					if (Line.Substr(0, 1) == ":")
					{
						Console.WriteLine("{0}", Line);
					}
					else
					{
						Console.WriteLine("    {0}", Line);
					}
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
		protected void LoadC0R_Ptr(int R) { LoadFieldPtr(Field_C0RList[R]); }
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

		AstOptimizer AstOptimizer = new AstOptimizer();
		GeneratorCSharp GeneratorCSharp = new GeneratorCSharp();

		public void GenerateIL(AstNodeStm AstNodeStm)
		{
			// Optimize
			AstNodeStm = (AstNodeStm)AstOptimizer.Optimize(AstNodeStm);

#if DEBUG_GENERATE_IL
			Console.WriteLine("{0}", GeneratorCSharp.Reset().Generate(AstNodeStm).ToString());
#endif

			new GeneratorIL(DynamicMethod, SafeILGenerator.__ILGenerator).Generate(AstNodeStm);
		}

		public void SaveGPR_F(int R, Action Action) { if (R != 0) SaveField<float>(Field_GPRList[R], Action); }
		public void SaveGPR(int R, Action Action) { if (R != 0) SaveField<int>(Field_GPRList[R], Action); }
		public void SaveC0R(int R, Action Action) { SaveField<int>(Field_C0RList[R], Action); }
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

		public static bool _LoadFcr31CC(CpuThreadState CpuThreadState)
		{
			return CpuThreadState.Fcr31.CC;
		}

		public static void _SaveFcr31CC(CpuThreadState CpuThreadState, bool Value)
		{
			CpuThreadState.Fcr31.CC = Value;
		}

		public void LoadFCR31_CC()
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Call((Func<CpuThreadState, bool>)MipsMethodEmitter._LoadFcr31CC);
		}

		public void SaveFCR31_CC(Action Action)
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			Action();
			SafeILGenerator.Call((Action<CpuThreadState, bool>)MipsMethodEmitter._SaveFcr31CC);
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


		public void LoadC0R(int R)
		{
			LoadC0R_Ptr(R);
			SafeILGenerator.LoadIndirect<int>();
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
			SaveGPR(RT, () => SafeILGenerator.Push((int)Value));
		}

		public void SET_REG(int RT, int RS)
		{
			SaveGPR(RT, () => LoadGPR_Unsigned(RS));
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
