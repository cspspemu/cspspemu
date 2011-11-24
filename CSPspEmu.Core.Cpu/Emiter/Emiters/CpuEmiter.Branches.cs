using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// Code executed after the delayed slot.
		public void _branch_post(Label Label)
		{
			MipsMethodEmiter.LoadBranchFlag();
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Brtrue, Label);
		}

		public void _branch_likely(Action Action)
		{
			var NullifyDelayedLabel = MipsMethodEmiter.ILGenerator.DefineLabel();
			MipsMethodEmiter.LoadBranchFlag();
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Brfalse, NullifyDelayedLabel);
			{
				Action();
			}
			MipsMethodEmiter.ILGenerator.MarkLabel(NullifyDelayedLabel);
		}

		private void _branch_pre_vv(params OpCode[] OpCodeList)
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RS);
				MipsMethodEmiter.LoadGPR_Signed(RT);
				foreach (var OpCode in OpCodeList)
				{
					MipsMethodEmiter.ILGenerator.Emit(OpCode);
				}
			});
		}

		private void _branch_pre_v0(params OpCode[] OpCodeList)
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4_0);
				foreach (var OpCode in OpCodeList)
				{
					MipsMethodEmiter.ILGenerator.Emit(OpCode);
				}
			});
		}

		// Branch on EQuals (Likely).
		public void beq() { _branch_pre_vv(OpCodes.Ceq); }
		public void beql() { beq(); }

		// Branch on Not Equals (Likely).
		public void bne() { _branch_pre_vv(OpCodes.Ceq, OpCodes.Ldc_I4_0, OpCodes.Ceq); }
		public void bnel() { bne(); }

		// Branch on Less Than Zero (And Link) (Likely).
		public void bltz() { _branch_pre_v0(OpCodes.Clt); }
		public void bltzl() { bltz(); }
		public void bltzal() { throw (new NotImplementedException()); }
		public void bltzall() { bltzal(); }

		// Branch on Less Or Equals than Zero (Likely).
		public void blez() { _branch_pre_v0(OpCodes.Cgt, OpCodes.Ldc_I4_0, OpCodes.Ceq); }
		public void blezl() { blez(); }

		// Branch on Great Than Zero (Likely).
		public void bgtz() { _branch_pre_v0(OpCodes.Cgt); }
		public void bgtzl() { bgtz(); }

		// Branch on Greater Equal Zero (And Link) (Likely).
		public void bgez() { _branch_pre_v0(OpCodes.Clt, OpCodes.Ldc_I4_0, OpCodes.Ceq); }
		public void bgezl() { bgez(); }
		public void bgezal() { throw (new NotImplementedException()); }
		public void bgezall() { bgezal(); }

		private uint GetJumpAddress()
		{
			//Console.WriteLine("Instruction.JUMP: {0:X}", Instruction.JUMP);
			return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);
		}

		private void _link()
		{
			//Console.WriteLine("LINK: {0:X}", PC);
			MipsMethodEmiter.SaveGPR(31, () =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, PC + 8);
			});
		}

		// Jump (And Link) (Register).
		public void j()
		{
			//Console.WriteLine("JUMP_ADDR: {0:X}", GetJumpAddress());
			MipsMethodEmiter.SavePC(() =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, GetJumpAddress());
			});

			if (CpuProcessor.PspConfig.TraceJal)
			{
				MipsMethodEmiter.ILGenerator.EmitWriteLine(String.Format("{0:X} : JAL 0x{0:X}", PC, GetJumpAddress()));
			}

			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Jmp);
			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldobj, (object)CpuProcessor.CreateAndCacheDelegateForPC(MemoryStream, GetJumpAddress()));
			//var FieldBuilder = MipsMethodEmiter.TypeBuilder.DefineField("testField", typeof(int), FieldAttributes.Static);
			//FieldBuilder.SetValue(null, CpuProcessor.CreateAndCacheDelegateForPC(MemoryStream, GetJumpAddress()));
			//FieldBuilder.SetValue(null, 1);
			
			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Callvirt);


			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ret);
		}
		public void jr() 
		{
			// RETURN
			if (RS == 31)
			{
			}

			MipsMethodEmiter.SavePC(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
			});
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ret);
		}

		public void jalr()
		{
			_link();
			jr();
		}

		public void jal()
		{
			_link();
			j();
		}

		// Branch on C1 False/True (Likely).
		public void bc1f()
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadFCR31_CC();
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4_0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ceq);
			});
		}
		public void bc1t() {
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadFCR31_CC();
			});
		}
		public void bc1fl() { bc1f(); }
		public void bc1tl() { bc1t(); }
	}
}
