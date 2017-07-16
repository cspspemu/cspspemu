using System.Drawing;

namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class ColorUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <param name="colors"></param>
        private static void InternalAdd(ref int r, ref int g, ref int b, ref int a, params Color[] colors)
        {
            foreach (var color in colors)
            {
                r += color.R;
                g += color.G;
                b += color.B;
                a += color.A;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        public static Color Average(params Color[] colors)
        {
            int r = 0, g = 0, b = 0, a = 0;
            var l = colors.Length;
            InternalAdd(ref r, ref g, ref b, ref a, colors);
            if (l == 0) l = 1;
            return Color.FromArgb(
                (byte) (a / l),
                (byte) (r / l),
                (byte) (g / l),
                (byte) (b / l)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static Color Average(Color color1, Color color2)
        {
            return Color.FromArgb(
                (byte) ((color1.A + color2.A) / 2),
                (byte) ((color1.R + color2.R) / 2),
                (byte) ((color1.G + color2.G) / 2),
                (byte) ((color1.B + color2.B) / 2)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        public static Color Add(params Color[] colors)
        {
            int r = 0, g = 0, b = 0, a = 0;
            InternalAdd(ref r, ref g, ref b, ref a, colors);
            return Color.FromArgb((byte) a, (byte) r, (byte) g, (byte) b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static Color Add(Color color1, Color color2)
        {
            return Color.FromArgb(
                (byte) ((color1.A + color2.A)),
                (byte) ((color1.R + color2.R)),
                (byte) ((color1.G + color2.G)),
                (byte) ((color1.B + color2.B))
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorLeft"></param>
        /// <param name="colorRight"></param>
        /// <returns></returns>
        public static Color Substract(Color colorLeft, Color colorRight)
        {
            return Color.FromArgb(
                (byte) (colorLeft.A - colorRight.A),
                (byte) (colorLeft.R - colorRight.R),
                (byte) (colorLeft.G - colorRight.G),
                (byte) (colorLeft.B - colorRight.B)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Color Average(Bitmap bitmap)
        {
            var colors = new Color[bitmap.Width * bitmap.Height];
            for (int y = 0, n = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++, n++)
                {
                    colors[n] = bitmap.GetPixel(x, y);
                }
            }
            return Average(colors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorFormat"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color Encode(ColorFormat colorFormat, uint value)
        {
            return Color.FromArgb(
                (int) (BitUtils.ExtractScaled(value, colorFormat.Alpha.Offset, colorFormat.Alpha.Size, 255)),
                (int) (BitUtils.ExtractScaled(value, colorFormat.Red.Offset, colorFormat.Red.Size, 255)),
                (int) (BitUtils.ExtractScaled(value, colorFormat.Green.Offset, colorFormat.Green.Size, 255)),
                (int) (BitUtils.ExtractScaled(value, colorFormat.Blue.Offset, colorFormat.Blue.Size, 255))
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="weightSum"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static int MixComponent(int color1, int color2, int weightSum, int weight1, int weight2)
        {
            return MathUtils.FastClamp((color1 * weight1 + color2 * weight2) / weightSum, 0, 255);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static Color Mix(Color color1, Color color2, double step)
        {
            var weightSum = (int) ushort.MaxValue;
            var weight2 = (int) (weightSum * step);
            var weight1 = weightSum - weight2;
            return Mix(color1, color2, weightSum, weight1, weight2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="weightSum"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static Color Mix(Color color1, Color color2, int weightSum, int weight1, int weight2)
        {
            return Color.FromArgb(
                MixComponent(color1.A, color2.A, weightSum, weight1, weight2),
                MixComponent(color1.R, color2.R, weightSum, weight1, weight2),
                MixComponent(color1.G, color2.G, weightSum, weight1, weight2),
                MixComponent(color1.B, color2.B, weightSum, weight1, weight2)
            );
        }
    }
}