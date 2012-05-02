using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSPspEmu.Core.Memory;
using Codegen;

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
					SafeILGenerator.Push((int)IMM);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				}, CanBeNull: false);
				Action();
			});
		}

		private bool MustLogWrites
		{
			get
			{
				return !(MipsMethodEmiter.Processor.Memory is FastPspMemory) && CpuProcessor.PspConfig.MustLogWrites;
			}
		}

		private void _save_common(Action Action)
		{
			_save_pc();
			MipsMethodEmiter._getmemptr(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)IMM);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			}, CanBeNull: false);
			Action();

			if (MustLogWrites)
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)IMM);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				SafeILGenerator.Push((int)PC);
				SafeILGenerator.Call((Action<uint, uint>)CpuThreadState.Methods.SetPCWriteAddress);
			}
		}

		private void _save_i<TType>()
		{
			_save_common(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				SafeILGenerator.StoreIndirect<TType>();
			});
		}

		// Load Byte/Half word/Word (Left/Right/Unsigned).
		public void lb() { _load_i(() => { SafeILGenerator.LoadIndirect<sbyte>(); }); }
		public void lh() { _load_i(() => { SafeILGenerator.LoadIndirect<short>(); }); }
		public void lw() {
			//MipsMethodEmiter.ILGenerator.EmitWriteLine(String.Format("PC(0x{0:X}) : LW: rt={1}, rs={2}, imm={3}", PC, RT, RS, Instruction.IMM));
			_load_i(() => { SafeILGenerator.LoadIndirect<int>(); });
		}

		private static readonly uint[] LwrMask = new uint[] { 0x00000000, 0xFF000000, 0xFFFF0000, 0xFFFFFF00 };
		private static readonly int[] LwrShift = new int[] { 0, 8, 16, 24 };

		private static readonly uint[] LwlMask = new uint[] { 0x00FFFFFF, 0x0000FFFF, 0x000000FF, 0x00000000 };
		private static readonly int[] LwlShift = new int[] { 24, 16, 8, 0 };

		static public uint _lwl_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint Value = *(uint*)CpuThreadState.GetMemoryPtr(Address & 0xFFFFFFFC);
			return (uint)((Value << LwlShift[AddressAlign]) | (RT & LwlMask[AddressAlign]));
		}

		static public uint _lwr_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint Value = *(uint*)CpuThreadState.GetMemoryPtr(Address & 0xFFFFFFFC);
			return (uint)((Value >> LwrShift[AddressAlign]) | (RT & LwrMask[AddressAlign]));
		}

		public void lwl()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				// ((memory.tread!(ushort)(registers[instruction.RS] + instruction.IMM - 0) << 0) & 0x_0000_FFFF)
				_save_pc();

				//_lwl_exec
				SafeILGenerator.LoadArgument0CpuThreadState(); // CpuThreadState
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)IMM);
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.CallMethod((Func<CpuThreadState, uint, int, uint, uint>)CpuEmiter._lwl_exec);
			});
		}

		public void lwr()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				// ((memory.tread!(ushort)(registers[instruction.RS] + instruction.IMM - 0) << 0) & 0x_0000_FFFF)
				_save_pc();

				SafeILGenerator.LoadArgument0CpuThreadState(); // CpuThreadState
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)IMM);
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.CallMethod((Func<CpuThreadState, uint, int, uint, uint>)CpuEmiter._lwr_exec);
			});	
		}

		public void lbu() { _load_i(() => { SafeILGenerator.LoadIndirect<byte>(); }); }
		public void lhu() { _load_i(() => { SafeILGenerator.LoadIndirect<ushort>(); }); }

		// Store Byte/Half word/Word (Left/Right).
		public void sb() { _save_i<sbyte>(); }
		public void sh() { _save_i<short>(); }
		public void sw() { _save_i<int>(); }
		//MipsMethodEmiter.ILGenerator.EmitWriteLine(String.Format("PC(0x{0:X}) : SW: rt={1}, rs={2}, imm={3}", PC, RT, RS, Instruction.IMM));

		private static readonly uint[] SwlMask = new uint[] { 0xFFFFFF00, 0xFFFF0000, 0xFF000000, 0x00000000 };
		private static readonly int[] SwlShift = new int[] { 24, 16, 8, 0 };

		private static readonly uint[] SwrMask = new uint[]  { 0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF };
		private static readonly int[] SwrShift = new int[] { 0, 8, 16, 24 };

		static public void _swl_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint* AddressPointer = (uint *)CpuThreadState.GetMemoryPtr(Address & 0xFFFFFFFC);

			*AddressPointer = (RT >> SwlShift[AddressAlign]) | (*AddressPointer & SwlMask[AddressAlign]);
		}

		static public void _swr_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint* AddressPointer = (uint*)CpuThreadState.GetMemoryPtr(Address & 0xFFFFFFFC);

			*AddressPointer = (RT << SwrShift[AddressAlign]) | (*AddressPointer & SwrMask[AddressAlign]);
		}

		public void swl()
		{
			_save_pc();

			SafeILGenerator.LoadArgument0CpuThreadState(); // CpuThreadState
			MipsMethodEmiter.LoadGPR_Unsigned(RS);
			SafeILGenerator.Push((int)IMM);
			MipsMethodEmiter.LoadGPR_Unsigned(RT);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint, int, uint>)CpuEmiter._swl_exec);
		}


		public void swr()
		{
			_save_pc();

			SafeILGenerator.LoadArgument0CpuThreadState(); // CpuThreadState
			MipsMethodEmiter.LoadGPR_Unsigned(RS);
			SafeILGenerator.Push((int)IMM);
			MipsMethodEmiter.LoadGPR_Unsigned(RT);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint, int, uint>)CpuEmiter._swr_exec);
		}

		// Load Linked word.
		// Store Conditional word.
		public void ll() { throw (new NotImplementedException()); }
		public void sc() { throw (new NotImplementedException()); }

		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		public void lwc1()
		{
#if false
			MipsMethodEmiter.SaveFPR(FT, () =>
			{
				_save_pc();
				MipsMethodEmiter._getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RS);
					SafeILGenerator.Push((int)IMM);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				});
				SafeILGenerator.LoadIndirect<float>();
			});
#else
			MipsMethodEmiter.SaveFPR_I(FT, () =>
			{
				_save_pc();
				MipsMethodEmiter._getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR_Unsigned(RS);
					SafeILGenerator.Push((int)IMM);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				});
				SafeILGenerator.LoadIndirect<uint>();
			});
#endif
		}
		public void swc1() {
			_save_common(() =>
			{
				MipsMethodEmiter.LoadFPR_I(FT);
				SafeILGenerator.StoreIndirect<int>();
			});
		}
	}
}