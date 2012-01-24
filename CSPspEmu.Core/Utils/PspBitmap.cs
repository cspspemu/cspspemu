using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Utils
{
	unsafe public class PspBitmap
	{
		protected GuPixelFormats PixelFormat;
		public int Width;
		public int BitsPerPixel;
		public int Height;
		public byte* Address;

		public PspBitmap(GuPixelFormats PixelFormat, int Width, int Height, byte* Address)
		{
			this.PixelFormat = PixelFormat;
			this.Width = Width;
			this.Height = Height;
			this.Address = Address;
			this.BitsPerPixel = PixelFormatDecoder.GetPixelsBits(PixelFormat);
		}

		public bool IsValidPosition(int X, int Y)
		{
			return X >= 0 && Y >= 0 && X < Width && Y < Height;
		}

		public void SetPixel(int X, int Y, OutputPixel Color)
		{
			if (!IsValidPosition(X, Y)) return;
			var Position = PixelFormatDecoder.GetPixelsSize(PixelFormat, Y * Width + X);
			switch (this.BitsPerPixel)
			{
				// 16 bits
				case 16:
					*(ushort*)&Address[Position] = (ushort)PixelFormatDecoder.EncodePixel(PixelFormat, Color);
					break;
				// 32 bits
				case 32:
					*(uint*)&Address[Position] = (uint)PixelFormatDecoder.EncodePixel(PixelFormat, Color);
					break;
				default:
					throw(new NotImplementedException());
			}		
		}

		public OutputPixel GetPixel(int X, int Y)
		{
			if (!IsValidPosition(X, Y)) return new OutputPixel();
			uint Value;
			var Position = PixelFormatDecoder.GetPixelsSize(PixelFormat, Y * Width + X);
			switch (this.BitsPerPixel)
			{
				// 16 bits
				case 16: Value = *(ushort*)&Address[Position]; break;
				// 32 bits
				case 32: Value = *(uint*)&Address[Position]; break;
				default: throw (new NotImplementedException());
			}
			return PixelFormatDecoder.DecodePixel(PixelFormat, Value);
		}
	}
}
