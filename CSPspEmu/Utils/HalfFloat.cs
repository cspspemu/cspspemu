using CSharpUtils;

namespace CSPspEmu.Core.Cpu
{
    public struct Float32
    {
        public uint Value;
    }

    public struct Float16
    {
        public ushort Value;

        public bool Sign => BitUtils.Extract(Value, 15, 1) != 0;

        public uint Exponent => BitUtils.Extract(Value, 10, 5);

        public uint Fraction => BitUtils.Extract(Value, 0, 10);
    }

    public class HalfFloat
    {
        //public ushort Value;
        //
        //public bool Sign { get { return BitUtils.Extract(Value, 15, 1) != 0; } }
        //public uint Exponent { get { return BitUtils.Extract(Value, 10, 5); } }
        //public uint Fraction { get { return BitUtils.Extract(Value, 0, 10); } }

        //public static ushort FloatToHalfFloat(float f)
        //{
        //	return FloatToHalfFloat(MathFloat.ReinterpretFloatAsUInt(f));
        //}
        //
        //public static ushort FloatToHalfFloat(uint f)
        //{
        //	const uint f32infty = 255 << 23;
        //	const uint f16infty = 31 << 23;
        //	const uint magic = 15 << 23;
        //
        //	const uint round_mask = ~0xfffu;
        //	const uint sign_mask = 0x80000000u;
        //
        //	ushort o;
        //
        //	uint sign = f & sign_mask;
        //	f ^= sign;
        //
        //	// Inf or NaN (all exponent bits set)
        //	if (f >= f32infty)
        //	{
        //		o = (ushort)((f > f32infty) ? 0x7e00 : 0x7c00); // NaN->qNaN and Inf->Inf
        //	}
        //	// (De)normalized number or zero
        //	else 
        //	{
        //		f &= round_mask;
        //		f = MathFloat.ReinterpretFloatAsUInt(MathFloat.ReinterpretUIntAsFloat(f) * MathFloat.ReinterpretUIntAsFloat(magic));
        //		f -= round_mask;
        //		if (f > f16infty) f = f16infty; // Clamp to signed infinity if overflowed
        //
        //		o = (ushort)(f >> 13); // Take the bits!
        //	}
        //
        //	o |= (ushort)(sign >> 16);
        //	return o;
        //}

        public static int FloatToHalfFloat(float Float)
        {
            var i = MathFloat.ReinterpretFloatAsInt(Float);
            var s = (i >> 16) & 0x00008000; // sign
            var e = ((i >> 23) & 0x000000ff) - (127 - 15); // exponent
            var f = (i >> 0) & 0x007fffff; // fraction

            // need to handle NaNs and Inf?
            if (e <= 0)
            {
                if (e < -10)
                {
                    if (s != 0)
                    {
                        // handle -0.0
                        return 0x8000;
                    }
                    return 0;
                }
                f = (f | 0x00800000) >> (1 - e);
                return s | (f >> 13);
            }
            else if (e == 0xff - (127 - 15))
            {
                if (f == 0)
                {
                    // Inf
                    return s | 0x7c00;
                }
                // NAN
                f >>= 13;
                return s | 0x7c00 | f | (f == 0 ? 1 : 0);
            }
            if (e > 30)
            {
                // Overflow
                return s | 0x7c00;
            }
            return s | (e << 10) | (f >> 13);
        }

        public static float ToFloat(int imm16)
        {
            var s = (imm16 >> 15) & 0x00000001; // Sign
            var e = (imm16 >> 10) & 0x0000001f; // Exponent
            var f = (imm16 >> 0) & 0x000003ff; // Fraction

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