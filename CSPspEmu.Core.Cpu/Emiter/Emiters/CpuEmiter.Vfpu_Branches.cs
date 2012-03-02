using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CSharpUtils;
using Codegen;

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
							case 0: SafeILGenerator.Push((int)0); break;
							// Equality
							case 1: Load_VS_VT(Index); SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals); break;
							// Less
							case 2: Load_VS_VT(Index); SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned); break;
							// Less than equals
							case 3: Load_VS_VT(Index); SafeILGenerator.CompareBinary(SafeBinaryComparison.LessOrEqualSigned); break;
							default: throw (new InvalidOperationException());
						}
					}
					else
					{
						if (TypeFlag == 0)
						{
							Load_VS(Index);
							SafeILGenerator.Push((float)0.0f);
							SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals); 
						}
						else
						{
							SafeILGenerator.Push((int)0);
							if ((Cond & 1) != 0)
							{
								Load_VS(Index);
								MipsMethodEmiter.CallMethod((Func<float, bool>)MathFloat.IsNan);
								SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); 
							}
							if ((Cond & 2) != 0)
							{
								Load_VS(Index);
								MipsMethodEmiter.CallMethod((Func<float, bool>)MathFloat.IsInfinity);
								SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or);
							}

						}
					}

					if (NotFlag)
					{
						SafeILGenerator.UnaryOperation(SafeUnaryOperator.Not);
					}
				});
			}
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((int)VectorSize);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, int>)CpuEmiter._vcmp_end);
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
				SafeILGenerator.LoadArgument0CpuThreadState()
				SafeILGenerator.Push((int)Register);
				SafeILGenerator.Push((int)VectorSize);
				MipsMethodEmiter.CallMethod(typeof(CpuEmiter), "_vcmovtf_test");
				*/

				var SkipSetLabel = SafeILGenerator.DefineLabel("SkipSetLabel");
				{
					Load_VCC(Register);
					SafeILGenerator.BranchUnaryComparison(True ? SafeUnaryComparison.False : SafeUnaryComparison.True, SkipSetLabel);
					//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Br, SkipSetLabel);
					{
						VectorOperationSaveVd((Index) =>
						{
							Load_VS(Index);
						});

						/*
						SafeILGenerator.LoadArgument0CpuThreadState()
						SafeILGenerator.Push((int)Register);
						SafeILGenerator.Push((int)VectorSize);
						MipsMethodEmiter.CallMethod(typeof(CpuEmiter), "_vcmovtf_set");
						*/

						//MipsMethodEmiter.ILGenerator.EmitWriteLine("SET!");
						//_call_debug_vfpu();
					}
				}
				SkipSetLabel.Mark();
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

		private void _bvtf(bool True)
		{
			var Register = Instruction.IMM3;
			//throw (new NotImplementedException());
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				Load_VCC(Register);
				SafeILGenerator.Push((int)0);
				if (True)
				{
					SafeILGenerator.UnaryOperation(SafeUnaryOperator.Not);
				}
				SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals);
			});
		}

		public void bvf() { _bvtf(false); }
		public void bvfl() { bvf(); }
		public void bvt() { _bvtf(true); }
		public void bvtl() { bvt(); }
	}
}
