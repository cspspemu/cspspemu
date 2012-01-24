using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Utils
{
	static public class ColorFormatExtensions
	{
		static public uint Encode(this ColorFormat ColorFormat, OutputPixel OutputPixel)
		{
			return ColorFormat.Encode(OutputPixel.R, OutputPixel.G, OutputPixel.B, OutputPixel.A);
		}

		static public OutputPixel Decode(this ColorFormat ColorFormat, uint Value)
		{
			var OutputPixel = default(OutputPixel);
			ColorFormat.Decode(Value, out OutputPixel.R, out OutputPixel.G, out OutputPixel.B, out OutputPixel.A);
			return OutputPixel;
		}
	}
}
