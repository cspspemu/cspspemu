using System;
using Codegen;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
	{
		[PspUntested]
		public void vbfy1()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 2 || VectorSize != 4)
			{
				Console.Error.WriteLine("vbfy1 : just support .p or .q");
				return;
			}

			VectorOperationSaveVd(VectorSize, (Index) =>
			{
				switch (Index)
				{
					case 0: Load_VS(0); Load_VS(1); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
					case 1: Load_VS(0); Load_VS(1); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
					case 2: Load_VS(2); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
					case 3: Load_VS(2); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
				}
			}, AsInteger: false);
		}

		[PspUntested]
		public void vbfy2()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4)
			{
				Console.Error.WriteLine("vbfy2 : just support .q");
				return;
			}

			VectorOperationSaveVd(VectorSize, (Index) =>
			{
				switch (Index)
				{
					case 0: Load_VS(0); Load_VS(2); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
					case 1: Load_VS(1); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
					case 2: Load_VS(0); Load_VS(2); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
					case 3: Load_VS(1); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
				}
			}, AsInteger: false);
		}

		private void _vsrt_doMinMax(Func<float, float, float> Func, int Left, int Right)
		{
			Load_VS(Left);
			Load_VS(Right);
			MipsMethodEmiter.CallMethod(Func);
		}

		[PspUntested]
		public void vsrt1()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");

			VectorOperationSaveVd(4, (Index) =>
			{
				switch (Index)
				{
					case 0: _vsrt_doMinMax(MathFloat.Min, 0, 1); break;
					case 1: _vsrt_doMinMax(MathFloat.Max, 0, 1); break;
					case 2: _vsrt_doMinMax(MathFloat.Min, 2, 3); break;
					case 3: _vsrt_doMinMax(MathFloat.Max, 2, 3); break;
				}
			}, AsInteger: false);
		}

		[PspUntested]
		public void vsrt2()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");

			VectorOperationSaveVd(4, (Index) =>
			{
				switch (Index)
				{
					case 0: _vsrt_doMinMax(MathFloat.Min, 0, 3); break;
					case 1: _vsrt_doMinMax(MathFloat.Min, 1, 2); break;
					case 2: _vsrt_doMinMax(MathFloat.Max, 1, 2); break;
					case 3: _vsrt_doMinMax(MathFloat.Max, 0, 3); break;
				}
			}, AsInteger: false);
		}

		[PspUntested]
		public void vsrt3()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");

			VectorOperationSaveVd(4, (Index) =>
			{
				switch (Index)
				{
					case 0: _vsrt_doMinMax(MathFloat.Max, 0, 1); break;
					case 1: _vsrt_doMinMax(MathFloat.Min, 0, 1); break;
					case 2: _vsrt_doMinMax(MathFloat.Max, 2, 3); break;
					case 3: _vsrt_doMinMax(MathFloat.Min, 2, 3); break;
				}
			}, AsInteger: false);
		}

		[PspUntested]
		public void vsrt4()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");

			VectorOperationSaveVd(4, (Index) =>
			{
				switch (Index)
				{
					case 0: _vsrt_doMinMax(MathFloat.Max, 0, 3); break;
					case 1: _vsrt_doMinMax(MathFloat.Max, 1, 2); break;
					case 2: _vsrt_doMinMax(MathFloat.Min, 1, 2); break;
					case 3: _vsrt_doMinMax(MathFloat.Min, 0, 3); break;
				}
			}, AsInteger: false);
		}

		/// <summary>
		/// +-------------------------------------+----+--------------+---+--------------+ 
		/// |31                                16 | 15 | 14         8 | 7 | 6         0  | 
		/// +-------------------------------------+----+--------------+---+--------------+ 
		/// |        opcode 0xd046 (p)            | 0  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | 
		/// |        opcode 0xd046 (t)            | 1  | vfpu_rs[6-0] | 0 | vfpu_rd[6-0] | 
		/// |        opcode 0xd046 (q)            | 1  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | 
		/// +-------------------------------------+----+--------------+---+--------------+ 
		/// 
		/// Float ADD?.Pair/Triple/Quad  --  Accumulate Components of Vector into Single Float
		/// 
		/// vfad.p %vfpu_rd, %vfpu_rs  ; Accumulate Components of Pair 
		/// vfad.t %vfpu_rd, %vfpu_rs  ; Accumulate Components of Triple 
		/// vfad.q %vfpu_rd, %vfpu_rs  ; Accumulate Components of Quad 
		/// 
		/// %vfpu_rs:   VFPU Vector Source Register ([p|t|q]reg 0..127) 
		/// %vfpu_rd:   VFPU Vector Destination Register (sreg 0..127) 
		/// 
		/// vfpu_regs[%vfpu_rd] <- Sum_Of_Components(vfpu_regs[%vfpu_rs]) 
		/// </summary>
		public void vfad()
		{
			uint VectorSize = Instruction.ONE_TWO;
			//Console.Error.WriteLine("VectorSize: {0}", VectorSize);
			VectorOperationSaveAggregatedVd(VectorSize,
				delegate()
				{
					SafeILGenerator.Push((float)0.0f);
				},
				delegate(int Index, Action<int> Load)
				{
					Load(1);
					//Load_VS(Index, VectorSize);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
					//EmitLogFloatResult();
				},
				delegate()
				{
					//EmitLogResult();
				}
			);
		}

		/// <summary>
		/// +-------------------------------------+----+--------------+---+--------------+ 
		/// |31                                16 | 15 | 14         8 | 7 | 6         0  | 
		/// +-------------------------------------+----+--------------+---+--------------+ 
		/// |        opcode 0xd047 (p)            | 0  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | 
		/// |        opcode 0xd047 (t)            | 1  | vfpu_rs[6-0] | 0 | vfpu_rd[6-0] | 
		/// |        opcode 0xd047 (q)            | 1  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | 
		/// +-------------------------------------+----+--------------+---+--------------+ 
		/// 
		///   VectorAverage.Pair/Triple/Quad  --  Average Components of Vector into Single Float
		/// 
		/// 		vavg.p %vfpu_rd, %vfpu_rs  ; Accumulate Components of Pair 
		/// 		vavg.t %vfpu_rd, %vfpu_rs  ; Accumulate Components of Triple 
		/// 		vavg.q %vfpu_rd, %vfpu_rs  ; Accumulate Components of Quad 
		/// 
		/// 				%vfpu_rs:   VFPU Vector Source Register ([p|t|q]reg 0..127) 
		/// 				%vfpu_rd:   VFPU Vector Destination Register (sreg 0..127) 
		/// 
		/// 		vfpu_regs[%vfpu_rd] <- Average_Of_Components(vfpu_regs[%vfpu_rs]) 
		/// </summary>
		public void vavg()
		{
			uint VectorSize = Instruction.ONE_TWO;
			//Console.Error.WriteLine("VectorSize: {0}", VectorSize);
			VectorOperationSaveAggregatedVd(VectorSize,
				delegate()
				{
					SafeILGenerator.Push((float)0.0f);
				},
				delegate(int Index, Action<int> Load)
				{
					Load(1);
					//Load_VS(Index, VectorSize);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
					//EmitLogFloatResult();
				},
				delegate()
				{
					SafeILGenerator.Push((float)VectorSize);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.DivideSigned);
					//EmitLogResult();
				}
			);
		}

	}
}
