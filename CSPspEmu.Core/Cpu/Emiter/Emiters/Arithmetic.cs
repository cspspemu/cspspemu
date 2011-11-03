﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void add()
		{
			MipsMethodEmiter.OP_3REG(RD, RS, RT, OpCodes.Add);
		}

		public void addu()
		{
			add();
		}

		public void addi()
		{
			MipsMethodEmiter.OP_2REG_IMM(RT, RS, (short)IMM, OpCodes.Add);
		}

		public void addiu()
		{
			addi();
		}

		public void sub()
		{
			MipsMethodEmiter.OP_3REG(RD, RS, RT, OpCodes.Sub);
		}

		public void subu()
		{
			sub();
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void and()
		{
			MipsMethodEmiter.OP_3REG(RD, RS, RT, OpCodes.And);
		}

		public void andi()
		{
			MipsMethodEmiter.OP_2REG_IMM(RT, RS, (short)IMM, OpCodes.And);
		}

		public void or()
		{
			MipsMethodEmiter.OP_3REG(RD, RS, RT, OpCodes.Or);
		}

		public void ori()
		{
			MipsMethodEmiter.OP_2REG_IMM(RT, RS, (short)IMM, OpCodes.Or);
		}

		public void xor()
		{
			MipsMethodEmiter.OP_3REG(RD, RS, RT, OpCodes.Xor);
		}

		public void xori()
		{
			MipsMethodEmiter.OP_2REG_IMM(RT, RS, (short)IMM, OpCodes.Xor);
		}

		public void nor()
		{
			or();
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Not);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void sll() { throw (new NotImplementedException()); }
		public void sllv() { throw(new NotImplementedException()); }
		public void sra() { throw(new NotImplementedException()); }
		public void srav() { throw(new NotImplementedException()); }
		public void srl() { throw(new NotImplementedException()); }
		public void srlv() { throw(new NotImplementedException()); }
		public void rotr() { throw(new NotImplementedException()); }
		public void rotrv() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void slt() { throw (new NotImplementedException()); }
		public void slti() { throw(new NotImplementedException()); }
		public void sltu() { throw(new NotImplementedException()); }
		public void sltiu() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Upper Immediate.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void lui() { throw (new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Sign Extend Byte/Half word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void seb() { throw (new NotImplementedException()); }
		public void seh() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// BIT REVerse.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void bitrev() { throw (new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MAXimum/MINimum.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void max() { throw (new NotImplementedException()); }
		public void min() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// DIVide (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void div() { throw (new NotImplementedException()); }
		public void divu() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mult() { throw (new NotImplementedException()); }
		public void multu() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Multiply ADD/SUBstract (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void madd() { throw (new NotImplementedException()); }
		public void maddu() { throw(new NotImplementedException()); }
		public void msub() { throw(new NotImplementedException()); }
		public void msubu() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move To/From HI/LO.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfhi() { throw (new NotImplementedException()); }
		public void mflo() { throw(new NotImplementedException()); }
		public void mthi() { throw(new NotImplementedException()); }
		public void mtlo() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void movz() { throw (new NotImplementedException()); }
		public void movn() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// EXTract/INSert.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void ext() { throw (new NotImplementedException()); }
		public void ins() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void clz() { throw (new NotImplementedException()); }
		public void clo() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void wsbh() { throw (new NotImplementedException()); }
		public void wsbw() { throw (new NotImplementedException()); }
	}
}
