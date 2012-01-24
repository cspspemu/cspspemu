using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Utils
{
	unsafe public class PspBitmap
	{
		protected GuPixelFormats PixelFormat;
		public int Width;
		public int BitsPerPixel;
		public int Height;
		public byte* Address;
		public ColorFormat ColorFormat;

		public PspBitmap(GuPixelFormats PixelFormat, int Width, int Height, byte* Address)
		{
			this.PixelFormat = PixelFormat;
			this.Width = Width;
			this.Height = Height;
			this.Address = Address;
			this.BitsPerPixel = PixelFormatDecoder.GetPixelsBits(PixelFormat);
			this.ColorFormat = PixelFormatDecoder.ColorFormatFromPixelFormat(PixelFormat);
		}

		public bool IsValidPosition(int X, int Y)
		{
			return ((X >= 0) && (Y >= 0) && (X < Width) && (Y < Height));
		}

		public void SetPixel(int X, int Y, OutputPixel Color)
		{
			if (!IsValidPosition(X, Y)) return;
			var Position = PixelFormatDecoder.GetPixelsSize(PixelFormat, Y * Width + X);
			uint Value = this.ColorFormat.Encode(Color);
			switch (this.BitsPerPixel)
			{
				case 16: *(ushort*)&Address[Position] = (ushort)Value; break;
				case 32: *(uint*)&Address[Position] = (uint)Value; break;
				default: throw(new NotImplementedException());
			}		
		}

		public OutputPixel GetPixel(int X, int Y)
		{
			if (!IsValidPosition(X, Y)) return new OutputPixel();
			uint Value;
			var Position = PixelFormatDecoder.GetPixelsSize(PixelFormat, Y * Width + X);
			switch (this.BitsPerPixel)
			{
				case 16: Value = *(ushort*)&Address[Position]; break;
				case 32: Value = *(uint*)&Address[Position]; break;
				default: throw (new NotImplementedException());
			}
			var OutputPixel  = default(OutputPixel);
			return this.ColorFormat.Decode(Value);
		}
	}
}
