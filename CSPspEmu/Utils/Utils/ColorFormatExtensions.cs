using CSharpUtils.Drawing;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Utils.Utils
{
    public static class ColorFormatExtensions
    {
        public static uint Encode(this ColorFormat colorFormat, OutputPixel outputPixel)
        {
            return colorFormat.Encode(outputPixel.R, outputPixel.G, outputPixel.B, outputPixel.A);
        }

        public static OutputPixel Decode(this ColorFormat colorFormat, uint value)
        {
            var outputPixel = default(OutputPixel);
            colorFormat.Decode(value, out outputPixel.R, out outputPixel.G, out outputPixel.B, out outputPixel.A);
            return outputPixel;
        }
    }
}