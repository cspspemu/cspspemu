using System;
using CSharpUtils;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public enum VfpuControlRegistersEnum
		{
			/// <summary>
			/// Source prefix stack
			/// </summary>
			VFPU_PFXS = 128,

			/// <summary>
			/// Target prefix stack
			/// </summary>
			VFPU_PFXT = 129,

			/// <summary>
			/// Destination prefix stack
			/// </summary>
			VFPU_PFXD = 130,

			/// <summary>
			/// Condition information
			/// </summary>
			VFPU_CC = 131,

			/// <summary>
			/// VFPU internal information 4
			/// </summary>
			VFPU_INF4 = 132,

			/// <summary>
			/// Not used (reserved)
			/// </summary>
			VFPU_RSV5 = 133,

			/// <summary>
			/// Not used (reserved)
			/// </summary>
			VFPU_RSV6 = 134,

			/// <summary>
			/// VFPU revision information
			/// </summary>
			VFPU_REV  = 135,

			/// <summary>
			/// Pseudorandom number generator information 0
			/// </summary>
			VFPU_RCX0 = 136,

			/// <summary>
			/// Pseudorandom number generator information 1
			/// </summary>
			VFPU_RCX1 = 137,

			/// <summary>
			/// Pseudorandom number generator information 2
			/// </summary>
			VFPU_RCX2 = 138,

			/// <summary>
			/// Pseudorandom number generator information 3
			/// </summary>
			VFPU_RCX3 = 139,

			/// <summary>
			/// Pseudorandom number generator information 4
			/// </summary>
			VFPU_RCX4 = 140,

			/// <summary>
			/// Pseudorandom number generator information 5
			/// </summary>
			VFPU_RCX5 = 141,
	
			/// <summary>
			/// Pseudorandom number generator information 6
			/// </summary>
			VFPU_RCX6 = 142,

			/// <summary>
			/// Pseudorandom number generator information 7
			/// </summary>
			VFPU_RCX7 = 143,
		}

		// Vfpu DOT product
		// Vfpu SCaLe/ROTate
		/// <summary>
		/// ID("vdot",        VM("011001:001:vt:two:vs:one:vd"), "%zs, %yp, %xp", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public void vdot()
		{
			uint VectorSize = Instruction.ONE_TWO;
			if (VectorSize == 1)
			{
				throw (new NotImplementedException(""));
			}
			_VfpuLoadVectorWithIndexPointer(Instruction.VD, 0, 1);
			{
				SafeILGenerator.Push((float)0.0f);
				for (uint Index = 0; Index < VectorSize; Index++)
				{
					_VfpuLoadVectorWithIndexPointer(Instruction.VS, Index, VectorSize);
					SafeILGenerator.LoadIndirect<float>();

					_VfpuLoadVectorWithIndexPointer(Instruction.VT, Index, VectorSize);
					SafeILGenerator.LoadIndirect<float>();

					SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				}
			}
			SafeILGenerator.StoreIndirect<float>();

			/*
			loadVs(vsize);
			loadVt(vsize);
			{
				VD[0] = 0.0;
				foreach (n; 0..vsize) VD[0] += VS[n] * VT[n];
			}
			saveVd(1);
			*/
		}
		public void vscl() {
			uint VectorSize = Instruction.ONE_TWO;
			if (VectorSize == 1)
			{
				throw (new NotImplementedException(""));
			}

			foreach (var Index in XRange(VectorSize))
			{
				Save_VD(Index, VectorSize, () =>
				{
					Load_VS(Index, VectorSize);
					Load_VT(0, 1);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				});
			}
		}

		// ROTate
		public void vrot()
		{
			var VectorSize = Instruction.ONE_TWO;
			uint imm5 = Instruction.IMM5;
			if (VectorSize == 1) throw(new NotImplementedException());

			//uint imm5 = instruction.IMM5;
			int SinIndex = (int)((imm5 >> 2) & 3);
			int CosIndex = (int)((imm5 >> 0) & 3);
			bool NegateSin = ((imm5 & 16) != 0);

			foreach (var Index in XRange(VectorSize))
			{
				Save_VD(Index, VectorSize, () =>
				{
					if (SinIndex == CosIndex)
					{
						// Angle [-1, +1]
						Load_VS(0, 1); // Angle
						SafeILGenerator.Push((float)(Math.PI / 2.0f));
						SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
						MipsMethodEmiter.CallMethod((Func<float, float>)MathFloat.Sin);
						if (NegateSin)
						{
							SafeILGenerator.UnaryOperation(SafeUnaryOperator.Negate);
						}
					}
					else
					{
						SafeILGenerator.Push((float)(0.0f));
					}
				});
			}

			if (SinIndex != CosIndex)
			{
				Save_VD(SinIndex, VectorSize, () =>
				{
					// Angle [-1, +1]
					Load_VS(0, 1); // Angle
					SafeILGenerator.Push((float)(Math.PI / 2.0f));
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
					MipsMethodEmiter.CallMethod((Func<float, float>)MathFloat.Sin);
					if (NegateSin)
					{
						SafeILGenerator.UnaryOperation(SafeUnaryOperator.Negate);
					}
				});
			}

			Save_VD(CosIndex, VectorSize, () =>
			{
				// Angle [-1, +1]
				Load_VS(0, 1); // Angle
				SafeILGenerator.Push((float)(Math.PI / 2.0f));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				MipsMethodEmiter.CallMethod((Func<float, float>)MathFloat.Cos);
			});
		}

		// Vfpu ZERO/ONE
		public void vzero()
		{
			VectorOperationSaveVd((Index) => { SafeILGenerator.Push((float)0.0f); });
		}
		public void vone()
		{
			VectorOperationSaveVd((Index) => { SafeILGenerator.Push((float)1.0f); });
		}

		// Vfpu MOVe/SiGN/Reverse SQuare root/COSine/Arc SINe/LOG2
		// @CHECK
		public void vmov()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS(Index);
			});
		}
		public void vabs()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS(Index);
				MipsMethodEmiter.CallMethod((Func<float, float>)MathFloat.Abs);
			});
		}
		public void vneg()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS(Index);
				SafeILGenerator.UnaryOperation(SafeUnaryOperator.Negate);
			});
		}
		public void vocp() {
			VectorOperationSaveVd((Index) => {
				SafeILGenerator.Push((float)1.0f);
				Load_VS(Index);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			});
		}
		public void vsgn()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS(Index);
				MipsMethodEmiter.CallMethod((Func<float, float>)MathFloat.Sign);
			});
		}
		public void vrcp()
		{
			VectorOperationSaveVd((Index) =>
			{
				SafeILGenerator.Push((float)1.0f);
				Load_VS(Index);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.DivideSigned);
			});
		}

		private void _vfpu_call_single_method(Delegate Delegate)
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS(Index);
				MipsMethodEmiter.CallMethod(Delegate);
			});
		}

		// OP_V_INTERNAL_IN_N!(1, "1.0f / sqrt(v)");
		public void vsqrt() { _vfpu_call_single_method((Func<float, float>)MathFloat.Sqrt); }
		public void vrsq() { _vfpu_call_single_method((Func<float, float>)MathFloat.RSqrt); }
		public void vsin() { _vfpu_call_single_method((Func<float, float>)MathFloat.SinV1); }
		public void vcos() { _vfpu_call_single_method((Func<float, float>)MathFloat.CosV1); }
		public void vexp2() { _vfpu_call_single_method((Func<float, float>)MathFloat.Exp2); }
		public void vlog2() { _vfpu_call_single_method((Func<float, float>)MathFloat.Log2); }
		public void vasin() { _vfpu_call_single_method((Func<float, float>)MathFloat.AsinV1); }
		public void vnrcp() { throw (new NotImplementedException("")); }
		public void vnsin() { throw (new NotImplementedException("")); }
		public void vrexp2() { throw (new NotImplementedException("")); }
		public void vsat0() { _vfpu_call_single_method((Func<float, float>)MathFloat.Vsat0); }
		public void vsat1() { _vfpu_call_single_method((Func<float, float>)MathFloat.Vsat1); }

		// Vfpu ConSTant
		public void vcst()
		{
			var VectorSize = Instruction.ONE_TWO;
			float FloatConstant = (Instruction.IMM5 >= 0 && Instruction.IMM5 < VfpuConstants.Length) ? VfpuConstants[Instruction.IMM5] : 0.0f;

			foreach (var Index in XRange(VectorSize))
			{
				//Console.Error.WriteLine("{0}/{1}", Index, VectorSize);
				Save_VD(Index, VectorSize, () =>
				{
					SafeILGenerator.Push((float)FloatConstant);
				}
				//, Debug: true
				);
			}
			//_call_debug_vfpu();
		}

		// -
		public void vhdp()
		{
			var VectorSize = Instruction.ONE_TWO;

			VectorOperationSaveVd(1, (Index) =>
			{
				SafeILGenerator.Push(0.0f);
				for (int n = 0; n < VectorSize - 1; n++)
				{
					Load_VS(n);
					Load_VT(n);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				}

				Load_VT((int)(VectorSize - 1));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			});
		}

		public void vcrs_t() {
			uint VectorSize = 3;

			Save_VD(0, VectorSize, () =>
			{
				Load_VS(1, VectorSize);
				Load_VT(2, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			});

			Save_VD(1, VectorSize, () =>
			{
				Load_VS(2, VectorSize);
				Load_VT(0, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			});

			Save_VD(2, VectorSize, () =>
			{
				Load_VS(0, VectorSize);
				Load_VT(1, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			});
		}

		/// <summary>
		/// Cross product
		/// </summary>
		public void vcrsp_t() {
			uint VectorSize = 3;

			Save_VD(0, VectorSize, () =>
			{
				Load_VS(1, VectorSize);
				Load_VT(2, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				Load_VS(2, VectorSize);
				Load_VT(1, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			});

			Save_VD(1, VectorSize, () =>
			{
				Load_VS(2, VectorSize);
				Load_VT(0, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				Load_VS(0, VectorSize);
				Load_VT(2, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			});

			Save_VD(2, VectorSize, () =>
			{
				Load_VS(0, VectorSize);
				Load_VT(1, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				Load_VS(1, VectorSize);
				Load_VT(0, VectorSize);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			});
			/*
			v3[0] = +v1[1] * v2[2] - v1[2] * v2[1];
			v3[1] = +v1[2] * v2[0] - v1[0] * v2[2];
			v3[2] = +v1[0] * v2[1] - v1[1] * v2[0];
			throw (new NotImplementedException(""));
			*/
		}

		// Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
		public void vmin()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS_VT(Index);
				MipsMethodEmiter.CallMethod((Func<float, float, float>)MathFloat.Min);
			});
		}
		public void vmax()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS_VT(Index);
				MipsMethodEmiter.CallMethod((Func<float, float, float>)MathFloat.Max);
			});
		}

		/// <summary>
		/// 	+----------------------+--------------+----+--------------+---+--------------+
		///     |31                 23 | 22        16 | 15 | 14         8 | 7 | 6         0  |
		///     +----------------------+--------------+----+--------------+---+--------------+
		///     |  opcode 0x60000000   | vfpu_rt[6-0] |    | vfpu_rs[6-0] |   | vfpu_rd[6-0] |
		///     +----------------------+--------------+----+--------------+---+--------------+
		///     
		///     VectorAdd.Single/Pair/Triple/Quad
		///     
		///     vadd.s %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Single
		///     vadd.p %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Pair
		///     vadd.t %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Triple
		///     vadd.q %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Quad
		///     
		///     %vfpu_rt:	VFPU Vector Source Register ([s|p|t|q]reg 0..127)
		///     %vfpu_rs:	VFPU Vector Source Register ([s|p|t|q]reg 0..127)
		///     %vfpu_rd:	VFPU Vector Destination Register ([s|p|t|q]reg 0..127)
		///     
		///     vfpu_regs[%vfpu_rd] <- vfpu_regs[%vfpu_rs] + vfpu_regs[%vfpu_rt]
		/// </summary>
		public void vadd()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS_VT(Index);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			});
		}
		public void vsub()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS_VT(Index);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			});
		}
		public void vdiv()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS_VT(Index);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.DivideSigned);
			});
		}

		/*
		public static float _vmul(float a, float b)
		{
			Console.Error.WriteLine("{0} * {1} = {2}", a, b, a * b);
			return a * b;
		}
		*/

		public void vmul()
		{
			VectorOperationSaveVd((Index) =>
			{
				Load_VS_VT(Index);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				//MipsMethodEmiter.CallMethod(this.GetType(), "_vmul");
			});
			/*
			_VectorOperation2Registers(Index =>
			{
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			});
			*/
		}

		void _vidt_x(uint VectorSize, uint Register)
		{
			uint IndexOne = BitUtils.Extract(Register, 0, 2);
			foreach (var Index in XRange(VectorSize))
			{
				VfpuSave_Register(Register, Index, VectorSize, PrefixNone, () =>
				{
					SafeILGenerator.Push((float)((Index == IndexOne) ? 1.0f : 0.0f));
				}
				//, Debug: true
				);
			}
		}

		void _vzero_x(uint VectorSize, uint Register)
		{
			uint IndexOne = BitUtils.Extract(Register, 0, 2);
			foreach (var Index in XRange(VectorSize))
			{
				VfpuSave_Register(Register, Index, VectorSize, PrefixNone, () =>
				{
					SafeILGenerator.Push((float) 0.0f);
				}
					//, Debug: true
				);
			}
		}

		// Vfpu (Matrix) IDenTity
		public void vidt()
		{
			var MatrixSize = Instruction.ONE_TWO;
			_vidt_x(MatrixSize, (uint)(Instruction.VD));
			//throw (new NotImplementedException(""));
		}

		// Vfpu load Integer IMmediate
		public void viim()
		{
			// @CHECK!
			Save_VT(Index: 0, VectorSize: 1, Action: () =>
			{
				//SafeILGenerator.Push((float)Instruction.IMM);
				SafeILGenerator.Push((float)Instruction.IMMU);
			});
		}

		public void vdet() { throw (new NotImplementedException("")); }

		public void mfvme() { throw (new NotImplementedException("")); }
		public void mtvme() { throw (new NotImplementedException("")); }

		/// <summary>
		/// ID("vfim",        VM("110111:11:1:vt:imm16"), "%xs, %vh",      ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public void vfim()
		{
			_VfpuLoadVectorWithIndexPointer(Instruction.VT, 0, 1);
			SafeILGenerator.Push((float)Instruction.IMM_HF);
			SafeILGenerator.StoreIndirect<float>();

			//_call_debug_vfpu();
		}


		public void vlgb() { throw (new NotImplementedException("")); }
		public void vsbn() { throw (new NotImplementedException("")); }

		public void vsbz() { throw (new NotImplementedException("")); }
		public void vsocp() { throw (new NotImplementedException("")); }
		public void vus2i() { throw (new NotImplementedException("")); }

		public void vwbn() { throw (new NotImplementedException("")); }
		//public void vwb_q() { throw(new NotImplementedException()); }
	}
	/*
	union {
		struct { VfpuPrefix vfpu_prefix_s, vfpu_prefix_t, vfpu_prefix_d; }
		struct { VfpuPrefix[3] vfpu_prefixes; }
	}
	*/
}