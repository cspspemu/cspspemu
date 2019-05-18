using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CSPspEmu.Utils;

namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public struct Rgba
    {
        /// <summary>
        /// 
        /// </summary>
        public byte R, G, B, A;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public Rgba(byte r, byte g, byte b, byte a)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Rgba(uint r, uint g, uint b, uint a)
        {
            A = (byte)a;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static implicit operator Rgba(ArgbRev col) => new Rgba(col.R, col.G, col.B, col.A);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static implicit operator ArgbRev(Rgba col) => new ArgbRev(col.A, col.R, col.G, col.B);

        public uint Value => PackInt(R, G, B, A);

        public static uint PackInt(byte r, byte  g, byte  b, byte  a) => (((uint)r) << 0) | (((uint)g) << 8) | (((uint)b) << 16) | (((uint)a) << 24);
        public static uint PackInt(uint r, uint g, uint b, uint a) => ((r & 0xFF) << 0) | ((g & 0xFF) << 8) | ((b & 0xFF) << 16) | ((a & 0xFF) << 24);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Rgba32
    {
        /// <summary>
        /// 
        /// </summary>
        public int R, G, B, A;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public Rgba32(int r, int g, int b, int a)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accum"></param>
        /// <param name="In"></param>
        /// <returns></returns>
        public static Rgba32 operator +(Rgba32 accum, ArgbRev In)
        {
            return new Rgba32()
            {
                R = accum.R + In.R,
                G = accum.G + In.G,
                B = accum.B + In.B,
                A = accum.A + In.A,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accum"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Rgba32 operator *(Rgba32 accum, int value)
        {
            return new Rgba32
            {
                R = accum.R * value,
                G = accum.G * value,
                B = accum.B * value,
                A = accum.A * value,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accum"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Rgba32 operator /(Rgba32 accum, int value)
        {
            if (value == 0)
            {
                return new Rgba32(0, 0, 0, 0);
            }
            return new Rgba32()
            {
                R = accum.R / value,
                G = accum.G / value,
                B = accum.B / value,
                A = accum.A / value,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static implicit operator Rgba32(ArgbRev col)
        {
            return new Rgba32 {R = col.R, G = col.G, B = col.B, A = col.A};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static implicit operator ArgbRev(Rgba32 col)
        {
            var a = (byte) MathUtils.FastClamp(col.A, 0x00, 0xFF);
            var r = (byte) MathUtils.FastClamp(col.R, 0x00, 0xFF);
            var g = (byte) MathUtils.FastClamp(col.G, 0x00, 0xFF);
            var b = (byte) MathUtils.FastClamp(col.B, 0x00, 0xFF);
            return new ArgbRev(a, r, g, b);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RgbaFloat
    {
        /// <summary>
        /// 
        /// </summary>
        public float R, G, B, A;

        public RgbaFloat(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public RgbaFloat(Vector4 v) : this(v.X, v.Y, v.Z, v.W)
        {
        }

        public Rgba Rgba => new Rgba(PackComponent(R), PackComponent(G), PackComponent(B), PackComponent(A));
        public uint Int => Rgba.Value;
        static private uint PackComponent(float v) => (uint)((int) (v * 255)).Clamp(0, 255);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ArgbRev
    {
        /// <summary>
        /// 
        /// </summary>
        public byte B, G, R, A;

        /// <summary>
        /// 
        /// </summary>
        public float Bf => B / 255f;

        /// <summary>
        /// 
        /// </summary>
        public float Gf => G / 255f;

        /// <summary>
        /// 
        /// </summary>
        public float Rf => R / 255f;

        /// <summary>
        /// 
        /// </summary>
        public float Af => A / 255f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public ArgbRev(int a, int r, int g, int b)
        {
            A = (byte) MathUtils.FastClamp(a, 0, 255);
            R = (byte) MathUtils.FastClamp(r, 0, 255);
            G = (byte) MathUtils.FastClamp(g, 0, 255);
            B = (byte) MathUtils.FastClamp(b, 0, 255);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public ArgbRev(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static ArgbRev Interpolate(ArgbRev color1, ArgbRev color2, float offset)
        {
            var offset1A = MathUtils.FastClamp(offset, 0, 1);
            var offset2A = 1 - offset1A;

            var offset1C = offset1A * (color1.Af);
            var offset2C = offset2A * (color2.Af);

            MathUtils.NormalizeSum(ref offset1C, ref offset2C);

            return new ArgbRev(
                (int) (color1.A * offset1A + color2.A * offset2A),
                (int) (color1.R * offset1C + color2.R * offset2C),
                (int) (color1.G * offset1C + color2.G * offset2C),
                (int) (color1.B * offset1C + color2.B * offset2C)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static ArgbRev Mix(ArgbRev color1, ArgbRev color2, float offset = 0.5f)
        {
            var offset1A = MathUtils.FastClamp(offset, 0, 1);
            var offset2A = 1 - offset1A;

            var offset1C = offset1A * (color1.Af);
            var offset2C = offset2A * (color2.Af);

            MathUtils.NormalizeSum(ref offset1C, ref offset2C);

            return new ArgbRev(
                color1.A + color2.A,
                (int) (color1.R * offset1C + color2.R * offset2C),
                (int) (color1.G * offset1C + color2.G * offset2C),
                (int) (color1.B * offset1C + color2.B * offset2C)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static int DistanceRgb(ArgbRev color1, ArgbRev color2)
        {
            var r = Math.Abs(color1.R - color2.R);
            var g = Math.Abs(color1.G - color2.G);
            var b = Math.Abs(color1.B - color2.B);
            return (r * r) + (g * g) + (b * b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static implicit operator ArgbRev(string col)
        {
            if (!col.StartsWith("#")) throw(new NotImplementedException());
            col = col.Substring(1);

            byte r, g, b, a = 0xFF;

            if (col.Length >= 6)
            {
                r = (byte) ((Convert.ToInt32(col.Substr(0, 2), 16) * 255) / 255);
                g = (byte) ((Convert.ToInt32(col.Substr(2, 2), 16) * 255) / 255);
                b = (byte) ((Convert.ToInt32(col.Substr(4, 2), 16) * 255) / 255);
                if (col.Length >= 8)
                {
                    a = (byte) ((Convert.ToInt32(col.Substr(6, 2), 16) * 255) / 255);
                }
            }
            else if (col.Length >= 3)
            {
                r = (byte) ((Convert.ToInt32(col.Substr(0, 1), 16) * 255) / 15);
                g = (byte) ((Convert.ToInt32(col.Substr(1, 1), 16) * 255) / 15);
                b = (byte) ((Convert.ToInt32(col.Substr(2, 1), 16) * 255) / 15);
                if (col.Length >= 4)
                {
                    a = (byte) ((Convert.ToInt32(col.Substr(3, 1), 16) * 255) / 15);
                }
            }
            else
            {
                throw(new NotImplementedException());
            }

            return new ArgbRev(a, r, g, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static implicit operator ArgbRev(Color col) =>
            new ArgbRev() {R = col.R, G = col.G, B = col.B, A = col.A};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static implicit operator Color(ArgbRev col) => Color.FromArgb(col.A, col.R, col.G, col.B);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Argb
    {
        /// <summary>
        /// 
        /// </summary>
        public byte A, R, G, B;
    }
}