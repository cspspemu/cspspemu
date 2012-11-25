#define DEBUG_GENERATE_IL
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

		//MethodBody GetMethodBody()
		//{
		//	return DynamicMethod.GetMethodBody();
		//}

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

		public void SaveField<TType>(FieldInfo FieldInfo, Action Action)
		{
			LoadFieldPtr(FieldInfo);
			Action();
			SafeILGenerator.StoreIndirect<TType>();
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
	}
}
