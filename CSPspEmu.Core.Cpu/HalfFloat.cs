using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
	public class HalfFloat
	{
		//public ushort Value;
		//
		//public bool Sign { get { return BitUtils.Extract(Value, 15, 1) != 0; } }
		//public uint Exponent { get { return BitUtils.Extract(Value, 10, 5); } }
		//public uint Fraction { get { return BitUtils.Extract(Value, 0, 10); } }

		public static float ToFloat(int imm16)
		{
			int s = (imm16 >> 15) & 0x00000001; // Sign
			int e = (imm16 >> 10) & 0x0000001f; // Exponent
			int f = (imm16 >> 0) & 0x000003ff;  // Fraction

			// Need to handle 0x7C00 INF and 0xFC00 -INF?
			if (e == 0)
			{
				// Need to handle +-0 case f==0 or f=0x8000?
				if (f == 0)
				{
					// Plus or minus zero
					return MathFloat.ReinterpretIntAsFloat(s << 31);
				}
				// Denormalized number -- renormalize it
				while ((f & 0x00000400) == 0)
				{
					f <<= 1;
					e -= 1;
				}
				e += 1;
				f &= ~0x00000400;
			}
			else if (e == 31)
			{
				if (f == 0)
				{
					// Inf
					return MathFloat.ReinterpretIntAsFloat((s << 31) | 0x7f800000);
				}
				// NaN
				return MathFloat.ReinterpretIntAsFloat((s << 31) | 0x7f800000 | (f << 13));
			}

			e = e + (127 - 15);
			f = f << 13;

			return MathFloat.ReinterpretIntAsFloat((s << 31) | (e << 23) | f);
		}

		/*
		private static float ToFloat(ushort ShortValue)
		{
			uint Value = 0;
			var Significand = BitUtils.Extract(ShortValue, 0, 10);
			var Exponent    = BitUtils.Extract(ShortValue, 10, 5);
			var Sign        = BitUtils.Extract(ShortValue, 15, 1);
			BitUtils.Insert(ref Value, 0, 23, Significand);
			BitUtils.Insert(ref Value, 23, 8, Exponent);
			//BitUtils.Insert(ref Value, 13, 10, Significand);
			//BitUtils.Insert(ref Value, 26, 5, Exponent);
			BitUtils.Insert(ref Value, 31, 1, Exponent);
			//Console.Error.WriteLine("%032b".Sprintf(Value));
			float ValueFloat = MathFloat.ReinterpretUIntAsFloat(Value);
			Console.Error.WriteLine(ValueFloat);
			return ValueFloat;
		}
		*/
	}
}
