using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		static public void _vcmp_end(CpuThreadState CpuThreadState, int VectorSize)
		{
			CpuThreadState.VFR_CC_4 = false;
			CpuThreadState.VFR_CC_5 = true;
			if (VectorSize >= 1) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_0; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_0; }
			if (VectorSize >= 2) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_1; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_1; }
			if (VectorSize >= 3) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_2; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_2; }
			if (VectorSize >= 4) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_3; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_3; }
			/*
			Console.Error.WriteLine(
				"{0}, {1}, {2}, {3}, {4}, {5}",
				CpuThreadState.VFR_CC_0,
				CpuThreadState.VFR_CC_1,
				CpuThreadState.VFR_CC_2,
				CpuThreadState.VFR_CC_3,
				CpuThreadState.VFR_CC_4,
				CpuThreadState.VFR_CC_5
			);
			*/
		}

		public void vcmp() {
			var VectorSize = Instruction.ONE_TWO;
			var Cond = Instruction.IMM4;
			bool NormalFlag = (Cond & 8) == 0;
			bool NotFlag = (Cond & 4) != 0;
			uint TypeFlag = Cond & 3;
			foreach (var Index in XRange(VectorSize))
			{
				Save_VCC(Index, () =>
				{
					if (NormalFlag)
					{
						switch (TypeFlag)
						{
							// True/False
							case 0: MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0); break;
							// Equality
							case 1: Load_VS_VT(Index); MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ceq); break;
							// Less
							case 2: Load_VS_VT(Index); MipsMethodEmiter.ILGenerator.Emit(OpCodes.Clt); break;
							// Less than equals
							case 3: Load_VS_VT(Index); MipsMethodEmiter.ILGenerator.Emit(OpCodes.Cgt); MipsMethodEmiter.ILGenerator.Emit(OpCodes.Not); break;
							default: throw (new InvalidOperationException());
						}
					}
					else
					{
						if (TypeFlag == 0)
						{
							Load_VS(Index);
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ceq);
						}
						else
						{
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
							if ((Cond & 1) != 0)
							{
								Load_VS(Index);
								MipsMethodEmiter.CallMethod(typeof(MathFloat), "IsNan");
								MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
							}
							if ((Cond & 2) != 0)
							{
								Load_VS(Index);
								MipsMethodEmiter.CallMethod(typeof(MathFloat), "IsInfinity");
								MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
							}

						}
					}

					if (NotFlag) MipsMethodEmiter.ILGenerator.Emit(OpCodes.Not);
				});
			}
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, VectorSize);
			MipsMethodEmiter.CallMethod(typeof(CpuEmiter), "_vcmp_end");
		}
		public void vslt() { throw (new NotImplementedException("")); }
		public void vsge() { throw (new NotImplementedException("")); }
		public void vscmp() { throw (new NotImplementedException("")); }

		/*
		static public void _vcmovtf_test(CpuThreadState CpuThreadState, int Register, int VectorSize)
		{
			Console.Error.WriteLine("_vcmovtf({0}, {1}) : {2}", Register, VectorSize, CpuThreadState.VFR_CC(Register));
		}

		static public void _vcmovtf_set(CpuThreadState CpuThreadState, int Register, int VectorSize)
		{
			Console.Error.WriteLine("SET! _vcmovtf({0}, {1}) : {2}", Register, VectorSize, CpuThreadState.VFR_CC(Register));
		}
		*/

		public void _vcmovtf(bool True)
		{
			var Register = Instruction.IMM3;
			var VectorSize = Instruction.ONE_TWO;

			if (Register < 6)
			{
				/*
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Register);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, VectorSize);
				MipsMethodEmiter.CallMethod(typeof(CpuEmiter), "_vcmovtf_test");
				*/

				var SkipSetLabel = MipsMethodEmiter.ILGenerator.DefineLabel();
				{
					Load_VCC(Register);
					MipsMethodEmiter.ILGenerator.Emit(True ? OpCodes.Brfalse : OpCodes.Brtrue, SkipSetLabel);
					//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Br, SkipSetLabel);
					{
						VectorOperationSaveVd((Index) =>
						{
							Load_VS(Index);
						});

						/*
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Register);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, VectorSize);
						MipsMethodEmiter.CallMethod(typeof(CpuEmiter), "_vcmovtf_set");
						*/

						//MipsMethodEmiter.ILGenerator.EmitWriteLine("SET!");
						//_call_debug_vfpu();
					}
				}
				MipsMethodEmiter.ILGenerator.MarkLabel(SkipSetLabel);
			}
			else if (Register == 6)
			{
				throw(new NotImplementedException());
			}
			else if (Register == 7)
			{
			}
			else
			{
				throw(new InvalidOperationException());
			}
		}

		public void vcmovf() { _vcmovtf(false); }
		public void vcmovt() { _vcmovtf(true); }

		public void bvf() { throw (new NotImplementedException()); }
		public void bvfl() { bvf(); }
		public void bvt() { throw (new NotImplementedException()); }
		public void bvtl() { bvt(); }
	}
}
