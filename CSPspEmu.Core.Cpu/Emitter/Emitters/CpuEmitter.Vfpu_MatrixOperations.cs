using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		// Vfpu Matrix MULtiplication
		// @FIX!!!
		public AstNodeStm vmmul()
		{
			int VectorSize = Instruction.ONE_TWO;
			var Dest = _Matrix(VD_NoPrefix);
			var Src = _Matrix(VS_NoPrefix);
			var Target = _Matrix(VT_NoPrefix);

			return Dest.SetMatrix((Column, Row) =>
			{
				var Adder = (AstNodeExpr)ast.Immediate(0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Adder += Target[Column, n] * Src[Row, n];
				}
				return Adder;
			});
		}

		// -

		private AstNodeStm _vtfm_x(uint VectorSize)
		{
			return AstNotImplemented("_vtfm_x");
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	SafeILGenerator.Push((float)0.0f);
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		Load_VS(n, VectorSize, RegisterOffset: Index);
			//		Load_VT(n, VectorSize);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//	}
			//});
		}

		private AstNodeStm _vhtfm_x(uint VectorSize)
		{
			return AstNotImplemented("_vhtfm_x");
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	SafeILGenerator.Push((float)0.0f);
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		Load_VS(n, VectorSize, RegisterOffset: Index);
			//		if (n == VectorSize - 1)
			//		{
			//			SafeILGenerator.Push((float)1.0f);
			//		}
			//		else
			//		{
			//			Load_VT(n, VectorSize - 1);
			//		}
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//	}
			//});
		}

		public AstNodeStm vtfm2()
		{
			return _vtfm_x(2);
		}
		public AstNodeStm vtfm3()
		{
			return _vtfm_x(3);
		}
		public AstNodeStm vtfm4()
		{
			return _vtfm_x(4);
		}

		public AstNodeStm vhtfm2()
		{
			return _vhtfm_x(2);
		}
		public AstNodeStm vhtfm3()
		{
			return _vhtfm_x(3);
		}
		public AstNodeStm vhtfm4()
		{
			return _vhtfm_x(4);
		}

		public AstNodeStm vmidt() { return _Matrix(VD).SetMatrix((Column, Row) => (Column == Row) ? 1f : 0f); }
		public AstNodeStm vmzero() { return _Matrix(VD).SetMatrix((Column, Row) => 0f); }
		public AstNodeStm vmone() { return _Matrix(VD).SetMatrix((Column, Row) => 1f); }

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
		public AstNodeStm vmscl()
		{
			var D = _Matrix(VD);
			var S = _Matrix(VS);
			var T = _Cell(VT);
			return D.SetMatrix((Column, Row) =>
				S[Column, Row] * T.Get()
			);
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

		public AstNodeStm vqmul()
		{
			return AstNotImplemented("vqmul");
			////var VectorSize = Instruction.ONE_TWO;
			//var VectorSize = (uint)4;
			//VectorOperationSaveVd(VectorSize, (Index) =>
			//{
			//	Load_VS(0);
			//	Load_VS(1);
			//	Load_VS(2);
			//	Load_VS(3);
			//	Load_VT(0);
			//	Load_VT(1);
			//	Load_VT(2);
			//	Load_VT(3);
			//	switch (Index)
			//	{
			//		case 0: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row0); break;
			//		case 1: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row1); break;
			//		case 2: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row2); break;
			//		case 3: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row3); break;
			//	}
			//});
		}

		public AstNodeStm vmmov() { return _Matrix(VD).SetMatrix((Column, Row) => _Matrix(VS)[Column, Row]); }
	}
}
