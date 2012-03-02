using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emiter
{
	unsafe sealed public partial class CpuEmiter
	{
		public void vbfy1() { throw (new NotImplementedException("")); }
		public void vbfy2() { throw (new NotImplementedException("")); }

		public void vsrt1() { throw (new NotImplementedException("")); }
		public void vsrt2() { throw (new NotImplementedException("")); }
		public void vsrt3() { throw (new NotImplementedException("")); }
		public void vsrt4() { throw (new NotImplementedException("")); }

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
