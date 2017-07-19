using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OutputPixel
    {
        public byte R, G, B, A;
        //public byte B, G, R, A;

        public OutputPixel(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public static OutputPixel FromRgba(byte r, byte g, byte b, byte a) => new OutputPixel() {R = r, G = g, B = b, A = a};

        public OutputPixel Transform(Func<byte, byte, byte, byte, OutputPixel> action) => action(R, G, B, A);

        public override string ToString() => $"RGBA({R},{G},{B},{A})";

        public static OutputPixel OperationPerComponent(OutputPixel c1, OutputPixel c2, Func<byte, byte, byte> func) => new OutputPixel
        {
            R = func(c1.R, c2.R),
            G = func(c1.G, c2.G),
            B = func(c1.B, c2.B),
            A = func(c1.A, c2.A),
        };

        public static OutputPixel operator &(OutputPixel c1, OutputPixel c2) => new OutputPixel
        {
            R = (byte) (c1.R & c2.R),
            G = (byte) (c1.G & c2.G),
            B = (byte) (c1.B & c2.B),
            A = (byte) (c1.A & c2.A),
        };

        public static bool operator ==(OutputPixel c1, OutputPixel c2) => (
            (c1.R == c2.R) &&
            (c1.G == c2.G) &&
            (c1.B == c2.B) &&
            (c1.A == c2.A)
        );

        public static bool operator !=(OutputPixel c1, OutputPixel c2) => !(c1 == c2);

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() != typeof(OutputPixel)) return false;
            return (OutputPixel) obj == this;
        }

        public override int GetHashCode() => (R << 0) | (G << 8) | (B << 16) | (A << 24);
        public int CheckSum => R + G + B + A;
    }
}