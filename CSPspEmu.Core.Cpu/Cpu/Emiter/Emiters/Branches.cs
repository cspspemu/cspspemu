using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

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
				MipsMethodEmiter.LoadGPR(RS);
				MipsMethodEmiter.LoadGPR(RT);
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
				MipsMethodEmiter.LoadGPR(RS);
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
		public void bltzall() { bltzall(); }

		// Branch on Less Or Equals than Zero (Likely).
		public void blez() { _branch_pre_v0(OpCodes.Cgt, OpCodes.Ldc_I4_0, OpCodes.Ceq); }
		public void blezl() { blezl(); }

		// Branch on Great Than Zero (Likely).
		public void bgtz() { _branch_pre_v0(OpCodes.Cgt); }
		public void bgtzl() { bgtz(); }

		// Branch on Greater Equal Zero (And Link) (Likely).
		public void bgez() { _branch_pre_v0(OpCodes.Clt, OpCodes.Ldc_I4_0, OpCodes.Ceq); }
		public void bgezl() { bgez(); }
		public void bgezal() { throw (new NotImplementedException()); }
		public void bgezall() { bgezal(); }

		// Jump (And Link) (Register).
		public void j() { throw (new NotImplementedException()); }
		public void jr() { throw (new NotImplementedException()); }
		public void jalr() { throw (new NotImplementedException()); }
		public void jal() { throw (new NotImplementedException()); }

		// Branch on C1 False/True (Likely).
		public void bc1f() { throw (new NotImplementedException()); }
		public void bc1t() { throw (new NotImplementedException()); }
		public void bc1fl() { bc1f(); }
		public void bc1tl() { bc1t(); }
	}
}
