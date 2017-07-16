namespace CSharpUtils
{
    public class BitmapChannelList
    {
        static public readonly BitmapChannel[] RGBA = new[]
            {BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Alpha};

        static public readonly BitmapChannel[] ARGB = new[]
            {BitmapChannel.Alpha, BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue};

        static public readonly BitmapChannel[] RGB = new[] {BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue};
    }

    public enum BitmapChannel
    {
        Indexed = -1,
        Blue = 0,
        Green = 1,
        Red = 2,
        Alpha = 3,
    }
}