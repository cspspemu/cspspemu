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

		private void _branch_pre_vv(Action Action)
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadGPR(RS);
				MipsMethodEmiter.LoadGPR(RT);
				Action();
			});
		}

		private void _branch_pre_v0(Action Action)
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadGPR(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4_0);
				Action();
			});
		}

		// Branch on EQuals (Likely).
		public void beq() {
			_branch_pre_vv(() => {
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ceq);
			});
		}
		public void beql() {
			beq();
		}

		// Branch on Greater Equal Zero (And Link) (Likely).
		public void bgez() { throw (new NotImplementedException()); }
		public void bgezl() { throw (new NotImplementedException()); }
		public void bgezal() { throw (new NotImplementedException()); }
		public void bgezall() { throw (new NotImplementedException()); }

		// Branch on Less Than Zero (And Link) (Likely).
		public void bltz() { throw (new NotImplementedException()); }
		public void bltzl() { throw (new NotImplementedException()); }
		public void bltzal() { throw (new NotImplementedException()); }
		public void bltzall() { throw (new NotImplementedException()); }

		// Branch on Less Or Equals than Zero (Likely).
		public void blez() { throw (new NotImplementedException()); }
		public void blezl() { throw (new NotImplementedException()); }

		// Branch on Great Than Zero (Likely).
		public void bgtz() { throw (new NotImplementedException()); }
		public void bgtzl() { throw (new NotImplementedException()); }

		// Branch on Not Equals (Likely).
		public void bne()
		{
			_branch_pre_vv(() =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ceq);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4_0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ceq);
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Not);
			});
		}
		public void bnel()
		{
			bne();
		}

		// Jump (And Link) (Register).
		public void j() { throw (new NotImplementedException()); }
		public void jr() { throw (new NotImplementedException()); }
		public void jalr() { throw (new NotImplementedException()); }
		public void jal() { throw (new NotImplementedException()); }

		// Branch on C1 False/True (Likely).
		public void bc1f() { throw (new NotImplementedException()); }
		public void bc1t() { throw (new NotImplementedException()); }
		public void bc1fl() { throw (new NotImplementedException()); }
		public void bc1tl() { throw (new NotImplementedException()); }
	}
}
