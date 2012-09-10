using System;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		// Load/Store Vfpu (Left/Right)_
		public void lv_s()
		{
			//return;
			uint VT = Instruction.VT5 | (Instruction.VT2 << 5);
			uint Column = (VT >> 5) & 3;
			uint Matrix = (VT >> 2) & 7;
			uint Row = (VT >> 0) & 3;

			SaveVprField(CalcVprRegisterIndex(Matrix, Column, Row), () =>
			{
				_load_memory_imm14_index(0);
				SafeILGenerator.LoadIndirect<float>();
			});
		}

		// ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		public void lv_q()
		{
			uint Register = Instruction.VT5_1;

			for (uint Index = 0; Index < 4; Index++)
			{
				_VfpuLoadVectorWithIndexPointer(Register, Index, 4);

				_load_memory_imm14_index(Index);
				SafeILGenerator.LoadIndirect<float>();

				SafeILGenerator.StoreIndirect<float>();
			}
		}

		public static void _lvl_svl_q(CpuThreadState CpuThreadState, uint m, uint i, uint address, bool dir, bool save)
		{
			uint k = 3 - ((address >> 2) & 3);
			address &= unchecked((uint)~0xF);

			for (uint j = k; j < 4; ++j)
			{
				fixed (float* VFPR = &CpuThreadState.VFR0)
				{
					float* ptr;
					if (dir)
					{
						ptr = &VFPR[m * 16 + i * 4 + j];
					}
					else
					{
						ptr = &VFPR[m * 16 + j * 4 + i];
					}
					if (save)
					{
						*(float*)CpuThreadState.GetMemoryPtr(address) = *ptr;
					}
					else
					{
						*ptr = *(float*)CpuThreadState.GetMemoryPtr(address);
					}
				}
				address += 4;
			}
		}

		public static void _lvr_svr_q(CpuThreadState CpuThreadState, uint m, uint i, uint address, bool dir, bool save)
		{
			uint k = 4 - ((address >> 2) & 3);

			for (uint j = 0; j < k; ++j)
			{
				fixed (float* VFPR = &CpuThreadState.VFR0)
				{
					float* ptr;
					if (dir)
					{
						ptr = &VFPR[m * 16 + i * 4 + j];
					}
					else
					{
						ptr = &VFPR[m * 16 + j * 4 + i];
					}
					if (save)
					{
						*(float*)CpuThreadState.GetMemoryPtr(address) = *ptr;
					}
					else
					{
						*ptr = *(float*)CpuThreadState.GetMemoryPtr(address);
					}
				}
				address += 4;
			}
		}

		private void lv_sv_l_r_q(bool left, bool save)
		{
			var vt = Instruction.VT5 | (Instruction.VT1 << 5);
			var m = (vt >> 2) & 7;
			var i = (vt >> 0) & 3;
			var dir = (vt & 32) != 0;

			{
				SafeILGenerator.LoadArgument0CpuThreadState();// CpuThreadState
				SafeILGenerator.Push((int)(m));
				SafeILGenerator.Push((int)(i));
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)(Instruction.IMM14 * 4));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				SafeILGenerator.Push((int)(dir ? 1 : 0));
				SafeILGenerator.Push((int)(save ? 1 : 0));
			}

			if (left)
			{

				MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint, uint, uint, bool, bool>)CpuEmitter._lvl_svl_q);
			}
			else
			{
				MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint, uint, uint, bool, bool>)CpuEmitter._lvr_svr_q);
			}
		}

		public void lvl_q()
		{
			lv_sv_l_r_q(left: true, save: false);
		}

		public void lvr_q()
		{
			lv_sv_l_r_q(left: false, save: false);
		}

		public void sv_s()
		{
			uint VT = Instruction.VT5 | (Instruction.VT2 << 5);
			uint Column = (VT >> 5) & 3;
			uint Matrix = (VT >> 2) & 7;
			uint Row = (VT >> 0) & 3;

			_load_memory_imm14_index(0);
			{
				LoadVprFieldPtr(CalcVprRegisterIndex(Matrix, Column, Row));
				SafeILGenerator.LoadIndirect<float>();
			}
			SafeILGenerator.StoreIndirect<float>();
		}

		/// <summary>
		/// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public void sv_q()
		{
			//loadVt(4, instruction.VT5_1);

			uint Register = Instruction.VT5_1;

			uint VectorSize = 4;

			foreach (var Index in XRange(VectorSize))
			{
				_load_memory_imm14_index((uint)Index);
				{
					//Load_VT(
					//VfpuLoad_Register(Register, Index, VectorSize, PrefixTarget);
					VfpuLoad_Register(Register, Index, VectorSize, ref PrefixNone);
					//_VfpuLoadVectorWithIndexPointer(Register, (uint)Index, 4);
					//SafeILGenerator.LoadIndirect<float>();
				}
				SafeILGenerator.StoreIndirect<float>();
			}
		}

		public void svl_q()
		{
			lv_sv_l_r_q(left: true, save: true);
		}

		public void svr_q()
		{
			lv_sv_l_r_q(left: false, save: true);
		}
	}
}
