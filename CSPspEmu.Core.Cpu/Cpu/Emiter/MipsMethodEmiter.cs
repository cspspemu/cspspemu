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
		static protected FieldInfo Field_GPR_Ptr = typeof(Processor).GetField("GPR_Ptr");
		static protected FieldInfo Field_BranchFlag = typeof(Processor).GetField("BranchFlag");
		static private ulong UniqueCounter = 0;

		public MipsMethodEmiter(MipsEmiter MipsEmiter)
		{
			UniqueCounter++;
			TypeBuilder = MipsEmiter.ModuleBuilder.DefineType("type" + UniqueCounter);
			MethodBuilder = TypeBuilder.DefineMethod(
				name           : MethodName = "method" + UniqueCounter,
				attributes     : MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.UnmanagedExport,
				returnType     : typeof(void),
				parameterTypes : new Type[] { typeof(Processor) }
			);
			ILGenerator = MethodBuilder.GetILGenerator();
		}

		public void LoadGPRPtr(int R)
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			ILGenerator.Emit(OpCodes.Ldfld, Field_GPR_Ptr);
			ILGenerator.Emit(OpCodes.Ldc_I4, R * 4);
			ILGenerator.Emit(OpCodes.Conv_I);
			ILGenerator.Emit(OpCodes.Add);
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

		public void SavePtr()
		{
			ILGenerator.Emit(OpCodes.Stind_I4);
		}

		public void OP_3REG(int RD, int RS, int RT, OpCode OpCode)
		{
			if (RD == 0) return;
			LoadGPRPtr(RD);
			LoadGPR(RS);
			LoadGPR(RT);
			ILGenerator.Emit(OpCode);
			SavePtr();
		}

		public void OP_2REG_IMM(int RT, int RS, short Immediate, OpCode OpCode)
		{
			if (RT == 0) return;
			LoadGPRPtr(RT);
			LoadGPR(RS);
			ILGenerator.Emit(OpCodes.Ldc_I4, (int)Immediate);
			ILGenerator.Emit(OpCode);
			SavePtr();
		}

		public void OP_2REG_IMMU(int RT, int RS, uint Immediate, OpCode OpCode)
		{
			if (RT == 0) return;
			LoadGPRPtr(RT);
			LoadGPR(RS);
			ILGenerator.Emit(OpCodes.Ldc_I4, (uint)Immediate);
			ILGenerator.Emit(OpCodes.Conv_U4);
			ILGenerator.Emit(OpCode);
			SavePtr();
		}

		public void SET(int RT, uint Value)
		{
			if (RT == 0) return;
			LoadGPRPtr(RT);
			ILGenerator.Emit(OpCodes.Ldc_I4, Value);
			SavePtr();
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
