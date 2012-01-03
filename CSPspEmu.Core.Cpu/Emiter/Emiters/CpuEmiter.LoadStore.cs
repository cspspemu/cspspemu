using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emiter
{
	unsafe sealed public partial class CpuEmiter
	{
		private void _save_pc()
		{
			if (!(MipsMethodEmiter.Processor.Memory is FastPspMemory))
			{
				MipsMethodEmiter.SavePC(PC);
			}
		}

		private void _load_i(Action Action)
		{
			_save_pc();
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter._getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RS);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				});
				Action();
			});
		}

		private void _save_common(Action Action)
		{
			_save_pc();
			MipsMethodEmiter._getmemptr(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
			});
			Action();

			if (!(MipsMethodEmiter.Processor.Memory is FastPspMemory))
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, PC);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("SetPCWriteAddress"));
			}
		}

		private void _save_i(OpCode OpCode)
		{
			_save_common(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCode);
			});
		}

		// Load Byte/Half word/Word (Left/Right/Unsigned).
		public void lb() { _load_i(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I1); }); }
		public void lh() { _load_i(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I2); }); }
		public void lw() {
			//MipsMethodEmiter.ILGenerator.EmitWriteLine(String.Format("PC(0x{0:X}) : LW: rt={1}, rs={2}, imm={3}", PC, RT, RS, Instruction.IMM));
			_load_i(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4); });
		}

		private static readonly uint[] LwrMask = new uint[] { 0x00000000, 0xFF000000, 0xFFFF0000, 0xFFFFFF00 };
		private static readonly uint[] LwrShift = new uint[] { 0, 8, 16, 24 };

		private static readonly uint[] LwlMask = new uint[] { 0x00FFFFFF, 0x0000FFFF, 0x000000FF, 0x00000000 };
		private static readonly uint[] LwlShift = new uint[] { 24, 16, 8, 0 };

		public void lwl()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				// ((memory.tread!(ushort)(registers[instruction.RS] + instruction.IMM - 0) << 0) & 0x_0000_FFFF)
				_save_pc();

				// int data = memory.read32(RS + SIMM16 & 0xFFFFFFFC);
				MipsMethodEmiter._getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RS);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFFFFFC);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
				});
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, LwlShift[IMM & 3]);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shl);

				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, LwlMask[IMM & 3]);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);

				// OR
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
			});
		}

		public void lwr()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				// ((memory.tread!(ushort)(registers[instruction.RS] + instruction.IMM - 0) << 0) & 0x_0000_FFFF)
				_save_pc();

				// int data = memory.read32(RS + SIMM16 & 0xFFFFFFFC);
				MipsMethodEmiter._getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RS);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFFFFFC);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
				});
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, LwrShift[IMM & 3]);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shr_Un);

				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, LwrMask[IMM & 3]);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);

				// OR
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
			});	
		}

		public void lbu() { _load_i(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_U1); }); }
		public void lhu() { _load_i(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_U2); }); }

		// Store Byte/Half word/Word (Left/Right).
		public void sb() { _save_i(OpCodes.Stind_I1); }
		public void sh() { _save_i(OpCodes.Stind_I2); }
		public void sw()
		{
			//MipsMethodEmiter.ILGenerator.EmitWriteLine(String.Format("PC(0x{0:X}) : SW: rt={1}, rs={2}, imm={3}", PC, RT, RS, Instruction.IMM));
			_save_i(OpCodes.Stind_I4);
		}

		private static readonly uint[] SwlMask = new uint[] { 0xFFFFFF00, 0xFFFF0000, 0xFF000000, 0x00000000 };
		private static readonly uint[] SwlShift = new uint[] { 24, 16, 8, 0 };

		private static readonly uint[] SwrMask = new uint[]  { 0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF };
		private static readonly uint[] SwrShift = new uint[]  { 0, 8, 16, 24 };

		public void swl()
		{
			_save_pc();
			MipsMethodEmiter._getmemptr(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFFFFFC);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
			});
			{
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RT);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, SwlShift[IMM & 3]);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shr_Un);

					MipsMethodEmiter._getmemptr(() =>
					{
						MipsMethodEmiter.LoadGPR_Unsigned(RS);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFFFFFC);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
					});
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, SwlMask[IMM & 3]);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
				}
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
			}
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_I4);
		}


		public void swr()
		{
			_save_pc();
			MipsMethodEmiter._getmemptr(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFFFFFC);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
			});
			{
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RT);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, SwrShift[IMM & 3]);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shl);

					MipsMethodEmiter._getmemptr(() =>
					{
						MipsMethodEmiter.LoadGPR_Unsigned(RS);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFFFFFC);
						MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
					});
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, SwrMask[IMM & 3]);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
				}
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Or);
			}
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_I4);
		}

		// Load Linked word.
		// Store Conditional word.
		public void ll() { throw (new NotImplementedException()); }
		public void sc() { throw (new NotImplementedException()); }

		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		public void lwc1()
		{
			MipsMethodEmiter.SaveFPR_I(FT, () =>
			{
				_save_pc();
				MipsMethodEmiter._getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RS);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				});
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_U4);
			});
		}
		public void swc1() {
			_save_common(() =>
			{
				MipsMethodEmiter.LoadFPR_I(FT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_I4); 
			});
		}
	}
}