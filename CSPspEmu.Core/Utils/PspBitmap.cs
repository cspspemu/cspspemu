using System;
using System.Drawing;
using System.Drawing.Imaging;
using CSharpUtils;

namespace CSPspEmu.Core.Utils
{
	unsafe public class PspBitmap
	{
		protected GuPixelFormats GuPixelFormat;
		public int Width;
		public int BitsPerPixel;
		public int Height;
		public int BytesPerLine;
		public byte* Address;
		public ColorFormat ColorFormat;

		public PspBitmap(GuPixelFormats PixelFormat, int Width, int Height, byte* Address, int BytesPerLine = -1)
		{
			this.GuPixelFormat = PixelFormat;
			this.Width = Width;
			this.Height = Height;
			this.Address = Address;
			this.BitsPerPixel = PixelFormatDecoder.GetPixelsBits(PixelFormat);
			this.ColorFormat = PixelFormatDecoder.ColorFormatFromPixelFormat(PixelFormat);
			if (BytesPerLine < 0)
			{
				this.BytesPerLine = PixelFormatDecoder.GetPixelsSize(GuPixelFormat, Width);
			}
			else
			{
				this.BytesPerLine = BytesPerLine;
			}
		}

		public bool IsValidPosition(int X, int Y)
		{
			return ((X >= 0) && (Y >= 0) && (X < Width) && (Y < Height));
		}

		private int GetOffset(int X, int Y)
		{
			return Y * BytesPerLine + PixelFormatDecoder.GetPixelsSize(GuPixelFormat, X);
		}

		public void SetPixel(int X, int Y, OutputPixel Color)
		{
			if (!IsValidPosition(X, Y)) return;
			//var Position = PixelFormatDecoder.GetPixelsSize(GuPixelFormat, Y * Width + X);
			var Position = GetOffset(X, Y);
			uint Value = this.ColorFormat.Encode(Color);
			switch (this.BitsPerPixel)
			{
				case 16: *(ushort*)(Address + Position) = (ushort)Value; break;
				case 32: *(uint*)(Address + Position) = (uint)Value; break;
				default: throw(new NotImplementedException());
			}		
		}

		public OutputPixel GetPixel(int X, int Y)
		{
			if (!IsValidPosition(X, Y)) return new OutputPixel();
			uint Value;
			var Position = GetOffset(X, Y);
			switch (this.BitsPerPixel)
			{
				case 16: Value = *(ushort*)(Address + Position); break;
				case 32: Value = *(uint*)(Address + Position); break;
				default: throw (new NotImplementedException());
			}
			//var OutputPixel = default(OutputPixel);
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
