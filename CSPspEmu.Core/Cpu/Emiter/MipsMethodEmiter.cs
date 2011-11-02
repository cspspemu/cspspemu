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
		protected ILGenerator ILGenerator;
		protected String MethodName;
		static protected FieldInfo Field_GPR_Ptr = typeof(Processor).GetField("GPR_Ptr");
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
			//typeof(MipsEmiter).
			ILGenerator = MethodBuilder.GetILGenerator();


			//var TypeBuilder = TypeBuilder
			//MethodBuilder = new MethodBuilder();
			/*
			DynamicMethod = new DynamicMethod(
				"",
				//MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.UnmanagedExport,
				//CallingConventions.Standard,
				typeof(void),
				new Type[] { typeof(Processor) }
				//typeof(MipsEmiter).Module,
				//true
			);
			ILGenerator = DynamicMethod.GetILGenerator();
			*/
		}

		/*
IL_0001: ldarg.0
IL_0002: ldfld uint32* CSPspEmu.Core.Cpu.Processor::GPR_Ptr
IL_0007: ldc.i4.4
IL_0008: conv.i
IL_0009: add

IL_000a: ldarg.0
IL_000b: ldfld uint32* CSPspEmu.Core.Cpu.Processor::GPR_Ptr
IL_0010: ldc.i4.8
IL_0011: conv.i
IL_0012: add
IL_0013: ldind.u4

IL_0014: ldarg.0
IL_0015: ldfld uint32* CSPspEmu.Core.Cpu.Processor::GPR_Ptr
IL_001a: ldc.i4.8
IL_001b: conv.i
IL_001c: add
IL_001d: ldind.u4

IL_001e: add
IL_001f: stind.i4
		*/

		protected void LoadGPRPtr(int R)
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			ILGenerator.Emit(OpCodes.Ldfld, Field_GPR_Ptr);
			ILGenerator.Emit(OpCodes.Ldc_I4, R * 4);
			ILGenerator.Emit(OpCodes.Conv_I);
			ILGenerator.Emit(OpCodes.Add);
		}

		protected void LoadGPR(int R)
		{
			if (R == 0)
			{
				ILGenerator.Emit(OpCodes.Ldc_I4_0);
			}
			else
			{
				LoadGPRPtr(R);
				ILGenerator.Emit(OpCodes.Ldind_U4);
			}
		}

		protected void SavePtr()
		{
			ILGenerator.Emit(OpCodes.Stind_I4);
		}

		protected void _3REG_OP(int RD, int RS, int RT, OpCode OpCode)
		{
			if (RD == 0) return;
			LoadGPRPtr(RD);
			LoadGPR(RS);
			LoadGPR(RT);
			ILGenerator.Emit(OpCode);
			SavePtr();
		}

		protected void _2REG_PLUS_IMM_OP(int RT, int RS, short Immediate, OpCode OpCode)
		{
			if (RT == 0) return;
			LoadGPRPtr(RT);
			LoadGPR(RS);
			ILGenerator.Emit(OpCodes.Ldc_I4, (int)Immediate);
			ILGenerator.Emit(OpCode);
			SavePtr();
		}

		public void ADDI(int RT, int RS, short Immediate)
		{
			_2REG_PLUS_IMM_OP(RT, RS, Immediate, OpCodes.Add);
		}

		public void ADD(int RD, int RS, int RT)
		{
			_3REG_OP(RD, RS, RT, OpCodes.Add);
		}

		public void SUB(int RD, int RS, int RT)
		{
			_3REG_OP(RD, RS, RT, OpCodes.Sub);
		}

		public void XOR(int RD, int RS, int RT)
		{
			_3REG_OP(RD, RS, RT, OpCodes.Xor);
		}

		public void OR(int RD, int RS, int RT)
		{
			_3REG_OP(RD, RS, RT, OpCodes.Or);
		}

		public void AND(int RD, int RS, int RT)
		{
			_3REG_OP(RD, RS, RT, OpCodes.And);
		}

		/*
		public void Sub(int RD, int RS, int RT)
		{
			if (RD == 0) return;
			LoadGPRPtr(RD);
			LoadGPR(RS); LoadGPR(RT); ILGenerator.Emit(OpCodes.Sub); SavePtr();
		}
		*/

		public Action<Processor> CreateDelegate()
		{
			ILGenerator.Emit(OpCodes.Ret);

			var Type = TypeBuilder.CreateType();
			return (Action<Processor>)Delegate.CreateDelegate(typeof(Action<Processor>), Type.GetMethod(MethodName));
			//return (Action<Processor>)DynamicMethod.CreateDelegate(typeof(Action<Processor>));
		}
	}
}
