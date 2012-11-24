using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe class CpuEmitterUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _rotr_impl(uint Value, int Offset) { return (Value >> Offset) | (Value << (32 - Offset)); }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int _max_impl(int Left, int Right) { return (Left > Right) ? Left : Right; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int _min_impl(int Left, int Right) { return (Left < Right) ? Left : Right; }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _bitrev_impl(uint v)
		{
			v = ((v >> 1) & 0x55555555) | ((v & 0x55555555) << 1); // swap odd and even bits
			v = ((v >> 2) & 0x33333333) | ((v & 0x33333333) << 2); // swap consecutive pairs
			v = ((v >> 4) & 0x0F0F0F0F) | ((v & 0x0F0F0F0F) << 4); // swap nibbles ... 
			v = ((v >> 8) & 0x00FF00FF) | ((v & 0x00FF00FF) << 8); // swap bytes
			v = (v >> 16) | (v << 16); // swap 2-byte long pairs
			return v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void _div_impl(CpuThreadState CpuThreadState, int Left, int Right)
		{
			if (Right == 0)
			{
				CpuThreadState.LO = 0;
				CpuThreadState.HI = 0;
			}
			if (Left == int.MinValue && Right == -1)
			{
				CpuThreadState.LO = int.MinValue;
				CpuThreadState.HI = 0;
			}
			else
			{
				CpuThreadState.LO = unchecked(Left / Right);
				CpuThreadState.HI = unchecked(Left % Right);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void _divu_impl(CpuThreadState CpuThreadState, uint Left, uint Right)
		{
			if (Right == 0)
			{
				CpuThreadState.LO = 0;
				CpuThreadState.HI = 0;
			}
			else
			{
				CpuThreadState.LO = unchecked((int)(Left / Right));
				CpuThreadState.HI = unchecked((int)(Left % Right));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _ext_impl(uint Data, int Pos, int Size) { return BitUtils.Extract(Data, Pos, Size); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _ins_impl(uint InitialData, uint Data, int Pos, int Size) { return BitUtils.Insert(InitialData, Pos, Size, Data); }

		// http://aggregate.org/MAGIC/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _clo_impl(uint x)
		{
			uint ret = 0;
			while ((x & 0x80000000) != 0) { x <<= 1; ret++; }
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _clz_impl(uint x)
		{
			return _clo_impl(~x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _wsbh_impl(uint v)
		{
			// swap bytes
			return ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _wsbw_impl(uint v)
		{
			// BSWAP
			return (
				((v & 0xFF000000) >> 24) |
				((v & 0x00FF0000) >> 8) |
				((v & 0x0000FF00) << 8) |
				((v & 0x000000FF) << 24)
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void _assign_hi_lo_impl(CpuThreadState CpuThreadState, long Value)
		{
			fixed (int* LOPtr = &CpuThreadState.LO)
			{
				*(long*)LOPtr = Value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long _get_hi_lo_impl(CpuThreadState CpuThreadState)
		{
			fixed (int* LOPtr = &CpuThreadState.LO)
			{
				return *(long*)LOPtr;
			}
		}

		public static int _cvt_w_s_impl(CpuThreadState CpuThreadState, float FS)
		{
			//Console.WriteLine("_cvt_w_s_impl: {0}", CpuThreadState.FPR[FS]);
			switch (CpuThreadState.Fcr31.RM)
			{
				case CpuThreadState.FCR31.TypeEnum.Rint: return (int)MathFloat.Rint(FS);
				case CpuThreadState.FCR31.TypeEnum.Cast: return (int)FS;
				case CpuThreadState.FCR31.TypeEnum.Ceil: return (int)MathFloat.Ceil(FS);
				case CpuThreadState.FCR31.TypeEnum.Floor: return (int)MathFloat.Floor(FS);
			}

			throw(new InvalidCastException("RM has an invalid value!!"));
			//case CpuThreadState.FCR31.TypeEnum.Floor: CpuThreadState.FPR_I[FD] = (int)MathFloat.Floor(CpuThreadState.FPR[FS]); break;
		}

		public static void _cfc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
		{
			switch (RD)
			{
				case 0: // readonly?
					throw (new NotImplementedException());
				case 31:
					CpuThreadState.GPR[RT] = (int)CpuThreadState.Fcr31.Value;
					break;
				default: throw (new Exception(String.Format("Unsupported CFC1(%d)", RD)));
			}
		}

		public static void _ctc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
		{
			switch (RD)
			{
				case 31:
					CpuThreadState.Fcr31.Value = (uint)CpuThreadState.GPR[RT];
					break;
				default: throw (new Exception(String.Format("Unsupported CFC1(%d)", RD)));
			}
		}

		public static void _comp_impl(CpuThreadState CpuThreadState, float s, float t, bool fc_unordererd, bool fc_equal, bool fc_less, bool fc_inv_qnan)
		{
			if (float.IsNaN(s) || float.IsNaN(t))
			{
				CpuThreadState.Fcr31.CC = fc_unordererd;
			}
			else
			{
				//bool cc = false;
				//if (fc_equal) cc = cc || (s == t);
				//if (fc_less) cc = cc || (s < t);
				//return cc;
				bool equal = (fc_equal) && (s == t);
				bool less = (fc_less) && (s < t);

				CpuThreadState.Fcr31.CC = (less || equal);
			}
		}
	}
}
