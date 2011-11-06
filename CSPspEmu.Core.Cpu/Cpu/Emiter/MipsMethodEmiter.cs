using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Reflection;

namespace CSPspEmu.Core.Cpu.Emiter
{
	/// <summary>
	/// <see cref="http://www.mrc.uidaho.edu/mrc/people/jff/digital/MIPSir.html"/>
	/// </summary>
	unsafe public class MipsMethodEmiter
	{
		protected TypeBuilder TypeBuilder;
		protected MethodBuilder MethodBuilder;
		//protected DynamicMethod DynamicMethod;
		public ILGenerator ILGenerator;
		protected String MethodName;
		public Processor Processor;
		//static protected FieldInfo Field_GPR_Ptr = typeof(Processor).GetField("GPR_Ptr");
		//static protected FieldInfo Field_FPR_Ptr = typeof(Processor).GetField("FPR_Ptr");
		static protected FieldInfo Field_BranchFlag = typeof(Processor).GetField("BranchFlag");
		static public MethodInfo Method_Syscall = typeof(Processor).GetMethod("Syscall");
		static private ulong UniqueCounter = 0;

		static protected bool InitializedOnce = false;
		static protected FieldInfo[] Field_GPRList;
		static protected FieldInfo[] Field_FPRList;

		/*
		static public MipsMethodEmiter()
		{
		}
		*/

		public MipsMethodEmiter(MipsEmiter MipsEmiter, Processor Processor)
		{
			this.Processor = Processor;

			if (!InitializedOnce)
			{
				Field_GPRList = new FieldInfo[32];
				Field_FPRList = new FieldInfo[32];
				for (int n = 0; n < 32; n++)
				{
					Field_GPRList[n] = typeof(Processor).GetField("GPR" + n);
					Field_FPRList[n] = typeof(Processor).GetField("FPR" + n);
				}

				InitializedOnce = true;
			}

			UniqueCounter++;
			TypeBuilder = MipsEmiter.ModuleBuilder.DefineType("type" + UniqueCounter, TypeAttributes.Sealed | TypeAttributes.Public);
			MethodBuilder = TypeBuilder.DefineMethod(
				name           : MethodName = "method" + UniqueCounter,
				attributes     : MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.UnmanagedExport | MethodAttributes.Final,
				returnType     : typeof(void),
				parameterTypes : new Type[] { typeof(Processor) }
			);
			ILGenerator = MethodBuilder.GetILGenerator();
		}

		protected void LoadGPRPtr(int R)
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			//ILGenerator.Emit(OpCodes.Ldfld, Field_GPR_Ptr);
			//ILGenerator.Emit(OpCodes.Ldc_I4, R * 4);
			//ILGenerator.Emit(OpCodes.Add);
			ILGenerator.Emit(OpCodes.Ldflda, Field_GPRList[R]);
		}

		protected void LoadFPRPtr(int R)
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			//ILGenerator.Emit(OpCodes.Ldfld, Field_FPR_Ptr);
			//ILGenerator.Emit(OpCodes.Ldc_I4, R * 4);
			//ILGenerator.Emit(OpCodes.Add);
			ILGenerator.Emit(OpCodes.Ldflda, Field_FPRList[R]);
		}

		public void LoadBranchFlag()
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			ILGenerator.Emit(OpCodes.Ldfld, Field_BranchFlag);
		}

		public void StoreBranchFlag(Action Action)
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			Action();
			ILGenerator.Emit(OpCodes.Stfld, Field_BranchFlag);
		}

		public void SaveGPR(int R, Action Action)
		{
			LoadGPRPtr(R);
			{
				Action();
			}
			ILGenerator.Emit(OpCodes.Stind_I4);
		}

		public void SaveFPR(int R, Action Action)
		{
			LoadFPRPtr(R);
			{
				Action();
			}
			ILGenerator.Emit(OpCodes.Stind_R4);
		}

		public void LoadGPR(int R)
		{
			if (R == 0)
			//if (false)
			{
				ILGenerator.Emit(OpCodes.Ldc_I4_0);
			}
			else
			{
				LoadGPRPtr(R);
				ILGenerator.Emit(OpCodes.Ldind_U4);
			}
		}

		public void LoadFPR(int R)
		{
			LoadFPRPtr(R);
			ILGenerator.Emit(OpCodes.Ldind_R4);
		}

		/*
		public void SavePtr()
		{
			ILGenerator.Emit(OpCodes.Stind_I4);
		}
		*/

		public void OP_3REG(int RD, int RS, int RT, OpCode OpCode)
		{
			if (RD == 0) return;
			SaveGPR(RD, () =>
			{
				LoadGPR(RS);
				LoadGPR(RT);
				ILGenerator.Emit(OpCode);
			});
		}

		public void OP_3REG_F(int FD, int FS, int FT, OpCode OpCode)
		{
			SaveFPR(FD, () =>
			{
				LoadFPR(FS);
				LoadFPR(FT);
				ILGenerator.Emit(OpCode);
			});
		}

		public void OP_2REG_IMM_F(int FD, int FS, float Immediate, OpCode OpCode)
		{
			SaveFPR(FD, () =>
			{
				LoadFPR(FS);
				ILGenerator.Emit(OpCodes.Ldc_R4, Immediate);
				ILGenerator.Emit(OpCode);
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

		public void OP_2REG_IMM(int RT, int RS, short Immediate, OpCode OpCode)
		{
			if (RT == 0) return;
			SaveGPR(RT, () =>
			{
				LoadGPR(RS);
				ILGenerator.Emit(OpCodes.Ldc_I4, (int)Immediate);
				ILGenerator.Emit(OpCode);
			});
		}

		public void OP_2REG_IMMU(int RT, int RS, uint Immediate, OpCode OpCode)
		{
			if (RT == 0) return;
			SaveGPR(RT, () =>
			{
				LoadGPR(RS);
				ILGenerator.Emit(OpCodes.Ldc_I4, (uint)Immediate);
				ILGenerator.Emit(OpCodes.Conv_U4);
				ILGenerator.Emit(OpCode);
			});
		}

		public void SET(int RT, uint Value)
		{
			if (RT == 0) return;
			SaveGPR(RT, () =>
			{
				ILGenerator.Emit(OpCodes.Ldc_I4, Value);
			});
		}


		public Action<Processor> CreateDelegate()
		{
			ILGenerator.Emit(OpCodes.Ret);

			var Type = TypeBuilder.CreateType();
			return (Action<Processor>)Delegate.CreateDelegate(typeof(Action<Processor>), Type.GetMethod(MethodName));
			//return (Action<Processor>)DynamicMethod.CreateDelegate(typeof(Action<Processor>));
		}
	}
}
