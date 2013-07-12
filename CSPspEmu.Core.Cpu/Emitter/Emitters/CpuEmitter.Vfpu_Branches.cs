using System;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		private AstNodeStm _vcmp_end(CpuThreadState CpuThreadState, int VectorSize)
		{
			return ast.AstNotImplemented("_vcmp_end");

			//CpuThreadState.VFR_CC_4 = false;
			//CpuThreadState.VFR_CC_5 = true;
			//if (VectorSize >= 1) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_0; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_0; }
			//if (VectorSize >= 2) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_1; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_1; }
			//if (VectorSize >= 3) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_2; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_2; }
			//if (VectorSize >= 4) { CpuThreadState.VFR_CC_4 |= CpuThreadState.VFR_CC_3; CpuThreadState.VFR_CC_5 &= CpuThreadState.VFR_CC_3; }
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

		public AstNodeStm vcmp()
		{
			return ast.AstNotImplemented("vcmp");

			//var VectorSize = Instruction.ONE_TWO;
			//var Cond = Instruction.IMM4;
			//bool NormalFlag = (Cond & 8) == 0;
			//bool NotFlag = (Cond & 4) != 0;
			//uint TypeFlag = Cond & 3;
			//foreach (var Index in XRange(VectorSize))
			//{
			//	Save_VCC(Index, () =>
			//	{
			//		if (NormalFlag)
			//		{
			//			switch (TypeFlag)
			//			{
			//				// True/False
			//				case 0: SafeILGenerator.Push((int)0); break;
			//				// Equality
			//				case 1: Load_VS_VT(Index); SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals); break;
			//				// Less
			//				case 2: Load_VS_VT(Index); SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned); break;
			//				// Less than equals
			//				case 3: Load_VS_VT(Index); SafeILGenerator.CompareBinary(SafeBinaryComparison.LessOrEqualSigned); break;
			//				default: throw (new InvalidOperationException());
			//			}
			//		}
			//		else
			//		{
			//			if (TypeFlag == 0)
			//			{
			//				Load_VS(Index);
			//				SafeILGenerator.Push((float)0.0f);
			//				SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals); 
			//			}
			//			else
			//			{
			//				SafeILGenerator.Push((int)0);
			//				if ((Cond & 1) != 0)
			//				{
			//					Load_VS(Index);
			//					MipsMethodEmitter.CallMethod((Func<float, bool>)MathFloat.IsNan);
			//					SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); 
			//				}
			//				if ((Cond & 2) != 0)
			//				{
			//					Load_VS(Index);
			//					MipsMethodEmitter.CallMethod((Func<float, bool>)MathFloat.IsInfinity);
			//					SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or);
			//				}
			//
			//			}
			//		}
			//
			//		if (NotFlag)
			//		{
			//			SafeILGenerator.UnaryOperation(SafeUnaryOperator.Not);
			//		}
			//	});
			//}
			//SafeILGenerator.LoadArgument0CpuThreadState();
			//SafeILGenerator.Push((int)VectorSize);
			//MipsMethodEmitter.CallMethod((Action<CpuThreadState, int>)CpuEmitter._vcmp_end);
		}

		public AstNodeStm _vsltge(string BinaryComparison)
		{
			return ast.AstNotImplemented("_vsltge");

			//var VectorSize = Instruction.ONE_TWO;
			//
			//VectorOperationSaveVd(VectorSize, (Index) =>
			//{
			//	Load_VS(Index);
			//	Load_VT(Index);
			//	SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned);
			//	SafeILGenerator.MacroIfElse(() =>
			//	{
			//		SafeILGenerator.Push(1.0f);
			//	}, () =>
			//	{
			//		SafeILGenerator.Push(0.0f);
			//	});
			//});
		}

		[PspUntested]
		public AstNodeStm vslt() { return _vsltge("<"); }

		[PspUntested]
		public AstNodeStm vsge() { return _vsltge(">="); }
		
		[PspUntested]
		public AstNodeStm vscmp()
		{
			return ast.AstNotImplemented("vscmp");
			//var VectorSize = Instruction.ONE_TWO;
			//
			//VectorOperationSaveVd(VectorSize, (Index) =>
			//{
			//	Load_VS(Index);
			//	Load_VT(Index);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			//	SafeILGenerator.Call((Func<float, float>)MathFloat.Sign);
			//});
		}

		/*
		public public static void _vcmovtf_test(CpuThreadState CpuThreadState, int Register, int VectorSize)
		{
			Console.Error.WriteLine("_vcmovtf({0}, {1}) : {2}", Register, VectorSize, CpuThreadState.VFR_CC(Register));
		}

		public static void _vcmovtf_set(CpuThreadState CpuThreadState, int Register, int VectorSize)
		{
			Console.Error.WriteLine("SET! _vcmovtf({0}, {1}) : {2}", Register, VectorSize, CpuThreadState.VFR_CC(Register));
		}
		*/

		[PspUntested]
		public AstNodeStm _vcmovtf(bool True)
		{
			return ast.AstNotImplemented("_vcmovtf"); ;
			//var Register = Instruction.IMM3;
			//var VectorSize = Instruction.ONE_TWO;
			//
			//if (Register < 6)
			//{
			//	/*
			//	SafeILGenerator.LoadArgument0CpuThreadState()
			//	SafeILGenerator.Push((int)Register);
			//	SafeILGenerator.Push((int)VectorSize);
			//	MipsMethodEmiter.CallMethod(typeof(CpuEmitter), "_vcmovtf_test");
			//	*/
			//
			//	var SkipSetLabel = SafeILGenerator.DefineLabel("SkipSetLabel");
			//	{
			//		Load_VCC(Register);
			//		SafeILGenerator.BranchUnaryComparison(True ? SafeUnaryComparison.False : SafeUnaryComparison.True, SkipSetLabel);
			//		//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Br, SkipSetLabel);
			//		{
			//			VectorOperationSaveVd(VectorSize, (Index) =>
			//			{
			//				Load_VS(Index);
			//			});
			//
			//			// Check some cases.
			//		}
			//	}
			//	SkipSetLabel.Mark();
			//}
			//else if (Register == 6)
			//{
			//	//throw (new NotImplementedException("_vcmovtf:Register = 6"));
			//	VectorOperationSaveVd(VectorSize, (Index) =>
			//	{
			//		Load_VCC((uint)Index);
			//
			//		SafeILGenerator.MacroIfElse(() =>
			//		{
			//			Load_VS(Index);
			//		}, () =>
			//		{
			//			Load_VD(Index, VectorSize);
			//		});
			//	});
			//
			//}
			//else if (Register == 7)
			//{
			//	// Copy always
			//	if (!True)
			//	{
			//		VectorOperationSaveVd(VectorSize, (Index) =>
			//		{
			//			Load_VS(Index);
			//		});
			//	}
			//}
			//else
			//{
			//	throw (new InvalidOperationException("_vcmovtf"));
			//}
		}

		public AstNodeStm vcmovf() { return _vcmovtf(false); }
		public AstNodeStm vcmovt() { return _vcmovtf(true); }

		private AstNodeStm _bvtf(bool True)
		{
			return ast.AstNotImplemented("_bvtf"); ;
			//var Register = Instruction.IMM3;
			//MipsMethodEmitter.StoreBranchFlag(() =>
			//{
			//	Load_VCC(Register);
			//	SafeILGenerator.Push((int)0);
			//	if (True)
			//	{
			//		SafeILGenerator.UnaryOperation(SafeUnaryOperator.Not);
			//	}
			//	SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals);
			//});
		}

		public AstNodeStm bvf() { return _bvtf(false); }
		public AstNodeStm bvfl() { return bvf(); }
		public AstNodeStm bvt() { return _bvtf(true); }
		public AstNodeStm bvtl() { return bvt(); }
	}
}
