using System.Drawing;

namespace CSharpUtils.Drawing.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ushort Encode565(this Color color)
        {
            return (ushort) color.Encode(ColorFormats.Rgba5650);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static uint Encode(this Color color, ColorFormat format)
        {
            return format.Encode(color.R, color.G, color.B, color.A);
        }
    }
}