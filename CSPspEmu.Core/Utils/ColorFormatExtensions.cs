using CSharpUtils;

namespace CSPspEmu.Core.Utils
{
	public static class ColorFormatExtensions
	{
		public static uint Encode(this ColorFormat ColorFormat, OutputPixel OutputPixel)
		{
			return ColorFormat.Encode(OutputPixel.R, OutputPixel.G, OutputPixel.B, OutputPixel.A);
		}

		public static OutputPixel Decode(this ColorFormat ColorFormat, uint Value)
		{
			var OutputPixel = default(OutputPixel);
			ColorFormat.Decode(Value, out OutputPixel.R, out OutputPixel.G, out OutputPixel.B, out OutputPixel.A);
			return OutputPixel;
		}
	}
}
