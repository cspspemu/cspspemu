namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class BitmapChannelList
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly BitmapChannel[] Rgba =
            {BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Alpha};

        /// <summary>
        /// 
        /// </summary>
        public static readonly BitmapChannel[] Argb =
            {BitmapChannel.Alpha, BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue};

        /// <summary>
        /// 
        /// </summary>
        public static readonly BitmapChannel[] Rgb = {BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue};
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BitmapChannel
    {
        /// <summary>
        /// 
        /// </summary>
        Indexed = -1,

        /// <summary>
        /// 
        /// </summary>
        Blue = 0,

        /// <summary>
        /// 
        /// </summary>
        Green = 1,

        /// <summary>
        /// 
        /// </summary>
        Red = 2,

        /// <summary>
        /// 
        /// </summary>
        Alpha = 3,
    }
}