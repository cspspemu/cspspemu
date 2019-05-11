namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class ColorFormats
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly ColorFormat Rgba5650 = new ColorFormat()
        {
            TotalBytes = 2,
            Offsets = new[] {0, 5, 11, 0},
            Sizes = new[] {5, 6, 5, 0},
        };

        /// <summary>
        /// 
        /// </summary>
        public static readonly ColorFormat Rgba4444 = new ColorFormat()
        {
            TotalBytes = 2,
            Offsets = new[] {0, 4, 8, 12},
            Sizes = new[] {4, 4, 4, 4},
        };

        /// <summary>
        /// 
        /// </summary>
        public static readonly ColorFormat Rgba5551 = new ColorFormat()
        {
            TotalBytes = 2,
            Offsets = new[] {0, 5, 10, 15},
            Sizes = new[] {5, 5, 5, 1},
        };

        /// <summary>
        /// 
        /// </summary>
        public static readonly ColorFormat Rgba8888 = new ColorFormat()
        {
            TotalBytes = 4,
            Offsets = new[] {0, 8, 16, 24},
            Sizes = new[] {8, 8, 8, 8},
        };
    }
}