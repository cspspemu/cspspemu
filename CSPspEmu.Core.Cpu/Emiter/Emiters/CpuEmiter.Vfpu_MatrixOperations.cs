using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	unsafe sealed public partial class CpuEmiter
	{
		// Vfpu Matrix MULtiplication
		// @FIX!!!
		public void vmmul()
		{
			var VectorSize = Instruction.ONE_TWO;
			//var MatrixRank = VectorSize;

			foreach (var IndexI in XRange(VectorSize))
			{
				foreach (var IndexJ in XRange(VectorSize))
				{
					// VD[j] += VS[k] * VT[k];
					//
					//VfpuSave_Register((uint)(Instruction.VD + IndexI), IndexJ, VectorSize, PrefixDestination, () =>
					Save_VD(IndexJ, VectorSize, IndexI, () =>
					{
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
						foreach (var IndexK in XRange(VectorSize))
						{
							Load_VT(IndexK, VectorSize, IndexI);
							Load_VS(IndexK, VectorSize, IndexJ);
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Mul);
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
						}
					});
				}
			}

			/*
			foreach (i; 0..vsize) {
				loadVt(vsize, instruction.VT + i);
				foreach (j; 0..vsize) {
					loadVs(vsize, instruction.VS + j);
					VD[j] = 0.0f; foreach (k; 0..vsize) VD[j] += VS[k] * VT[k];
				}
				saveVd(vsize, instruction.VD + i);
			}
			*/
			//throw (new NotImplementedException(""));
		}

		// -

		private void _vtfm_x(uint VectorSize)
		{
			VectorOperationSaveVd(VectorSize, Index =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Load_VS(n, VectorSize, RegisterOffset: Index);
					Load_VT(n, VectorSize);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Mul);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				}
			});
		}

		private void _vhtfm_x(uint VectorSize)
		{
			VectorOperationSaveVd(VectorSize, Index =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Load_VS(n, VectorSize, RegisterOffset: Index);
					if (n == VectorSize - 1)
					{
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, 1.0f);
					}
					else
					{
						Load_VT(n, VectorSize - 1);
					}
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Mul);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				}
			});
		}

		public void vtfm2()
		{
			_vtfm_x(2);
		}
		public void vtfm3()
		{
			_vtfm_x(3);
		}
		public void vtfm4()
		{
			_vtfm_x(4);
		}

		public void vhtfm2()
		{
			_vhtfm_x(2);
		}
		public void vhtfm3()
		{
			_vhtfm_x(3);
		}
		public void vhtfm4()
		{
			_vhtfm_x(4);
		}

		public void vmidt()
		{
			var MatrixSize = Instruction.ONE_TWO;

			foreach (var Index in XRange(MatrixSize))
			{
				_vidt_x(MatrixSize, (uint)(Instruction.VD + Index));
			}
		}

		public void vmzero()
		{
			var MatrixSize = Instruction.ONE_TWO;

			foreach (var Index in XRange(MatrixSize))
			{
				_vzero_x(MatrixSize, (uint)(Instruction.VD + Index));
			}

			//throw (new NotImplementedException(""));
		}

		public void vmone()
		{
			throw (new NotImplementedException(""));
		}

		/// <summary>
		/// +----------------------+--------------+----+--------------+---+--------------+
		/// |31                 23 | 22        16 | 15 | 14         8 | 7 | 6         0  |
		/// +----------------------+--------------+----+--------------+---+--------------+
		/// |  opcode 0x65008080   | vfpu_rt[6-0] |    | vfpu_rs[6-0] |   | vfpu_rd[6-0] |
		/// +----------------------+--------------+----+--------------+---+--------------+
		/// 
		/// MatrixScale.Pair/Triple/Quad, multiply all components by scale factor
		/// 
		/// vmscl.p %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 2x2 Matrix by %vfpu_rt
		/// vmscl.t %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 3x3 Matrix by %vfpu_rt
		/// vmscl.q %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 4x4 Matrix by %vfpu_rt
		/// 
		/// %vfpu_rt:       VFPU Vector Source Register, Scale (sreg 0..127)
		/// %vfpu_rs:       VFPU Vector Source Register, Matrix ([p|t|q]reg 0..127)
		/// %vfpu_rd:       VFPU Vector Destination Register, Matrix ([s|p|t|q]reg 0..127)
		/// 
		/// vfpu_mtx[%vfpu_rd] <- vfpu_mtx[%vfpu_rs] * vfpu_reg[%vfpu_rt]
		/// </summary>
		public void vmscl()
		{
			var MatrixSize = Instruction.ONE_TWO;

			foreach (var RowIndex in XRange(MatrixSize))
			{
				uint VectorSize = MatrixSize;

				foreach (var Index in XRange(VectorSize))
				{
					Save_VD(Index, VectorSize, RowIndex, () =>
					{
						Load_VS(Index, VectorSize, RowIndex);
						Load_VT(0, 1);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Mul);
					});
				}
			}
			//throw (new NotImplementedException(""));
		}

		public void vqmul()
		{
			throw (new NotImplementedException(""));
		}

		public void vmmov()
		{
			var VectorSize = Instruction.ONE_TWO;

			foreach (var y in XRange(VectorSize))
			{
				foreach (var x in XRange(VectorSize))
				{
					Save_VD(x, VectorSize, y, () =>
					{
						Load_VS(x, VectorSize, y);
					});
				}
			}
		}
	}
}
