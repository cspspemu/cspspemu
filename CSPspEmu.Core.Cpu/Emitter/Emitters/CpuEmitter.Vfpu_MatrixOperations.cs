using System;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
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
						SafeILGenerator.Push((float)0.0f);
						foreach (var IndexK in XRange(VectorSize))
						{
							Load_VT(IndexK, VectorSize, IndexI);
							Load_VS(IndexK, VectorSize, IndexJ);
							SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
							SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
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
				SafeILGenerator.Push((float)0.0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Load_VS(n, VectorSize, RegisterOffset: Index);
					Load_VT(n, VectorSize);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				}
			});
		}

		private void _vhtfm_x(uint VectorSize)
		{
			VectorOperationSaveVd(VectorSize, Index =>
			{
				SafeILGenerator.Push((float)0.0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Load_VS(n, VectorSize, RegisterOffset: Index);
					if (n == VectorSize - 1)
					{
						SafeILGenerator.Push((float)1.0f);
					}
					else
					{
						Load_VT(n, VectorSize - 1);
					}
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
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
		/// +----------------------+--------------+----+--------------+---+--------------+ <para/>
		/// |31                 23 | 22        16 | 15 | 14         8 | 7 | 6         0  | <para/>
		/// +----------------------+--------------+----+--------------+---+--------------+ <para/>
		/// |  opcode 0x65008080   | vfpu_rt[6-0] |    | vfpu_rs[6-0] |   | vfpu_rd[6-0] | <para/>
		/// +----------------------+--------------+----+--------------+---+--------------+ <para/>
		/// <para/>
		/// MatrixScale.Pair/Triple/Quad, multiply all components by scale factor <para/>
		/// <para/>
		/// vmscl.p %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 2x2 Matrix by %vfpu_rt <para/>
		/// vmscl.t %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 3x3 Matrix by %vfpu_rt <para/>
		/// vmscl.q %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 4x4 Matrix by %vfpu_rt <para/>
		/// <para/>
		/// %vfpu_rt:       VFPU Vector Source Register, Scale (sreg 0..127) <para/>
		/// %vfpu_rs:       VFPU Vector Source Register, Matrix ([p|t|q]reg 0..127) <para/>
		/// %vfpu_rd:       VFPU Vector Destination Register, Matrix ([s|p|t|q]reg 0..127) <para/>
		/// <para/>
		/// vfpu_mtx[%vfpu_rd] &lt;- vfpu_mtx[%vfpu_rs] * vfpu_reg[%vfpu_rt]
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
						SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
					});
				}
			}
			//throw (new NotImplementedException(""));
		}

		public float _vqmul_row0(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return +(l0 * r3) + (l1 * r2) - (l2 * r1) + (l3 * r0);
			//v3[0] = +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
		}

		public float _vqmul_row1(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return -(l0 * r2) + (l1 * r3) + (l2 * r0) + (l3 * r1);
			//v3[1] = -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
		}

		public float _vqmul_row2(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return +(l0 * r1) - (l1 * r0) + (l2 * r3) + (l3 * r2);
			//v3[2] = +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
		}

		public float _vqmul_row3(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return -(l0 * r0) - (l1 * r1) - (l2 * r2) + (l3 * r3);
			//v3[3] = -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);
		}

#if false
			loadVs(4, vs);
			loadVt(4, vt);

			v3[0] = +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
			v3[1] = -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
			v3[2] = +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
			v3[3] = -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);

			saveVd(4, vd, v3);
#endif

		public void vqmul()
		{
			//var VectorSize = Instruction.ONE_TWO;
			var VectorSize = (uint)4;
			VectorOperationSaveVd(VectorSize, (Index) =>
			{
				Load_VS(0);
				Load_VS(1);
				Load_VS(2);
				Load_VS(3);
				Load_VT(0);
				Load_VT(1);
				Load_VT(2);
				Load_VT(3);
				switch (Index)
				{
					case 0: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row0); break;
					case 1: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row1); break;
					case 2: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row2); break;
					case 3: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row3); break;
				}
			});
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
