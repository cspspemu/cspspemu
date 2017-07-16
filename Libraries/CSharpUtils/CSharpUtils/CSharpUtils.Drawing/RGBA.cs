using System;
using System.Drawing;

namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public struct RGBA
    {
        /// <summary>
        /// 
        /// </summary>
        public byte R, G, B, A;

        public RGBA(byte R, byte G, byte B, byte A)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public static implicit operator RGBA(ARGB_Rev Col)
        {
            return new RGBA(Col.R, Col.G, Col.B, Col.A);
        }

        public static implicit operator ARGB_Rev(RGBA Col)
        {
            return new ARGB_Rev(Col.A, Col.R, Col.G, Col.B);
        }
    }

    public struct RGBA32
    {
        public int R, G, B, A;

        public RGBA32(int R, int G, int B, int A)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public static RGBA32 operator +(RGBA32 Accum, ARGB_Rev In)
        {
            return new RGBA32()
            {
                R = Accum.R + In.R,
                G = Accum.G + In.G,
                B = Accum.B + In.B,
                A = Accum.A + In.A,
            };
        }

        public static RGBA32 operator *(RGBA32 Accum, int Value)
        {
            return new RGBA32()
            {
                R = Accum.R * Value,
                G = Accum.G * Value,
                B = Accum.B * Value,
                A = Accum.A * Value,
            };
        }

        public static RGBA32 operator /(RGBA32 Accum, int Value)
        {
            if (Value == 0)
            {
                return new RGBA32(0, 0, 0, 0);
            }
            else
            {
                return new RGBA32()
                {
                    R = Accum.R / Value,
                    G = Accum.G / Value,
                    B = Accum.B / Value,
                    A = Accum.A / Value,
                };
            }
        }

        public static implicit operator RGBA32(ARGB_Rev Col)
        {
            return new RGBA32() {R = Col.R, G = Col.G, B = Col.B, A = Col.A};
        }

        public static implicit operator ARGB_Rev(RGBA32 Col)
        {
            var A = (byte) MathUtils.FastClamp(Col.A, 0x00, 0xFF);
            var R = (byte) MathUtils.FastClamp(Col.R, 0x00, 0xFF);
            var G = (byte) MathUtils.FastClamp(Col.G, 0x00, 0xFF);
            var B = (byte) MathUtils.FastClamp(Col.B, 0x00, 0xFF);
            return new ARGB_Rev(A, R, G, B);
        }
    }

    public struct RGBAFloat
    {
        public float R, G, B, A;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ARGB_Rev
    {
        /// <summary>
        /// 
        /// </summary>
        public byte B, G, R, A;

        public float Bf
        {
            get { return B / 255f; }
        }

        public float Gf
        {
            get { return G / 255f; }
        }

        public float Rf
        {
            get { return R / 255f; }
        }

        public float Af
        {
            get { return A / 255f; }
        }

        public ARGB_Rev(int A, int R, int G, int B)
        {
            this.A = (byte) MathUtils.FastClamp(A, 0, 255);
            this.R = (byte) MathUtils.FastClamp(R, 0, 255);
            this.G = (byte) MathUtils.FastClamp(G, 0, 255);
            this.B = (byte) MathUtils.FastClamp(B, 0, 255);
        }

        public ARGB_Rev(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Color1"></param>
        /// <param name="Color2"></param>
        /// <param name="Offset"></param>
        /// <returns></returns>
        public static ARGB_Rev Interpolate(ARGB_Rev Color1, ARGB_Rev Color2, float Offset)
        {
            var Offset1A = MathUtils.FastClamp(Offset, 0, 1);
            var Offset2A = 1 - Offset1A;

            var Offset1C = Offset1A * (Color1.Af);
            var Offset2C = Offset2A * (Color2.Af);

            MathUtils.NormalizeSum(ref Offset1C, ref Offset2C);

            return new ARGB_Rev(
                (int) (Color1.A * Offset1A + Color2.A * Offset2A),
                (int) (Color1.R * Offset1C + Color2.R * Offset2C),
                (int) (Color1.G * Offset1C + Color2.G * Offset2C),
                (int) (Color1.B * Offset1C + Color2.B * Offset2C)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Color1"></param>
        /// <param name="Color2"></param>
        /// <param name="Offset"></param>
        /// <returns></returns>
        public static ARGB_Rev Mix(ARGB_Rev Color1, ARGB_Rev Color2, float Offset = 0.5f)
        {
            var Offset1A = MathUtils.FastClamp(Offset, 0, 1);
            var Offset2A = 1 - Offset1A;

            var Offset1C = Offset1A * (Color1.Af);
            var Offset2C = Offset2A * (Color2.Af);

            MathUtils.NormalizeSum(ref Offset1C, ref Offset2C);

            return new ARGB_Rev(
                (int) (Color1.A + Color2.A),
                (int) (Color1.R * Offset1C + Color2.R * Offset2C),
                (int) (Color1.G * Offset1C + Color2.G * Offset2C),
                (int) (Color1.B * Offset1C + Color2.B * Offset2C)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Color1"></param>
        /// <param name="Color2"></param>
        /// <returns></returns>
        public static int DistanceRGB(ARGB_Rev Color1, ARGB_Rev Color2)
        {
            var R = Math.Abs(Color1.R - Color2.R);
            var G = Math.Abs(Color1.G - Color2.G);
            var B = Math.Abs(Color1.B - Color2.B);
            return (R * R) + (G * G) + (B * B);
        }

        public static implicit operator ARGB_Rev(string Col)
        {
            if (Col.StartsWith("#"))
            {
                Col = Col.Substring(1);

                byte R, G, B, A = 0xFF;

                if (Col.Length >= 6)
                {
                    R = (byte) ((Convert.ToInt32(Col.Substr(0, 2), 16) * 255) / 255);
                    G = (byte) ((Convert.ToInt32(Col.Substr(2, 2), 16) * 255) / 255);
                    B = (byte) ((Convert.ToInt32(Col.Substr(4, 2), 16) * 255) / 255);
                    if (Col.Length >= 8)
                    {
                        A = (byte) ((Convert.ToInt32(Col.Substr(6, 2), 16) * 255) / 255);
                    }
                }
                else if (Col.Length >= 3)
                {
                    R = (byte) ((Convert.ToInt32(Col.Substr(0, 1), 16) * 255) / 15);
                    G = (byte) ((Convert.ToInt32(Col.Substr(1, 1), 16) * 255) / 15);
                    B = (byte) ((Convert.ToInt32(Col.Substr(2, 1), 16) * 255) / 15);
                    if (Col.Length >= 4)
                    {
                        A = (byte) ((Convert.ToInt32(Col.Substr(3, 1), 16) * 255) / 15);
                    }
                }
                else
                {
                    throw(new NotImplementedException());
                }

                return new ARGB_Rev(A, R, G, B);
            }
            throw(new NotImplementedException());
        }

        public static implicit operator ARGB_Rev(Color Col)
        {
            return new ARGB_Rev() {R = Col.R, G = Col.G, B = Col.B, A = Col.A};
        }

        public static implicit operator Color(ARGB_Rev Col)
        {
            return Color.FromArgb(Col.A, Col.R, Col.G, Col.B);
        }

        public override string ToString()
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ARGB
    {
        /// <summary>
        /// 
        /// </summary>
        public byte A, R, G, B;
    }
}