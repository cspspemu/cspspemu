using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Utils
{
	unsafe public class PspBitmap
	{
		protected GuPixelFormats GuPixelFormat;
		public int Width;
		public int BitsPerPixel;
		public int Height;
		public byte* Address;
		public ColorFormat ColorFormat;

		public PspBitmap(GuPixelFormats PixelFormat, int Width, int Height, byte* Address)
		{
			this.GuPixelFormat = PixelFormat;
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
			var Position = PixelFormatDecoder.GetPixelsSize(GuPixelFormat, Y * Width + X);
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
			var Position = PixelFormatDecoder.GetPixelsSize(GuPixelFormat, Y * Width + X);
			switch (this.BitsPerPixel)
			{
				case 16: Value = *(ushort*)&Address[Position]; break;
				case 32: Value = *(uint*)&Address[Position]; break;
				default: throw (new NotImplementedException());
			}
			var OutputPixel  = default(OutputPixel);
			return this.ColorFormat.Decode(Value);
		}

		public Bitmap ToBitmap()
		{
			var Bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
			Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				int Count = Width * Height;
				var Output = (OutputPixel*)BitmapData.Scan0;
				PixelFormatDecoder.Decode(
					GuPixelFormat,
					Address,
					Output,
					Width,
					Height,
					IgnoreAlpha: true
				);

				for (int n = 0; n < Count; n++)
				{
					var Color = Output[n];
					Output[n].R = Color.B;
					Output[n].G = Color.G;
					Output[n].B = Color.R;
					Output[n].A = 0xFF;
				}
			});
			return Bitmap;
		}
	}
}
