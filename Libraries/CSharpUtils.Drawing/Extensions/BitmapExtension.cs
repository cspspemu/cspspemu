using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using CSharpUtils;

static public class BitmapExtension
{
	unsafe static public void SetPalette(this Bitmap Bitmap, IEnumerable<Color> Colors)
	{
		ColorPalette Palette = BitmapUtils.GetColorPalette(Colors.Count());

		int n = 0;
		foreach (var Color in Colors)
		{
			Palette.Entries[n++] = Color;
		}

		Bitmap.Palette = Palette;
	}

	unsafe static public byte[] GetIndexedDataLinear(this Bitmap Bitmap)
	{
		return Bitmap.GetChannelsDataLinear(BitmapChannel.Indexed);
	}

	unsafe static public byte[] GetIndexedDataLinear(this Bitmap Bitmap, Rectangle Rectangle)
	{
		return Bitmap.GetChannelsDataLinear(Rectangle, BitmapChannel.Indexed);
	}

	unsafe static public void SetIndexedDataLinear(this Bitmap Bitmap, byte[] NewData)
	{
		Bitmap.SetChannelsDataLinear(NewData, BitmapChannel.Indexed);
	}

	unsafe static public void SetIndexedDataLinear(this Bitmap Bitmap, Rectangle Rectangle, byte[] NewData)
	{
		Bitmap.SetChannelsDataLinear(NewData, Rectangle, BitmapChannel.Indexed);
	}

	unsafe static public byte[] GetChannelsDataLinear(this Bitmap Bitmap, params BitmapChannel[] Channels)
	{
		return Bitmap.GetChannelsDataLinear(Bitmap.GetFullRectangle(), Channels);
	}

	unsafe static public byte[] GetChannelsDataLinear(this Bitmap Bitmap, Rectangle Rectangle, params BitmapChannel[] Channels)
	{
		var NewData = new byte[Rectangle.Width * Rectangle.Height * Channels.Length];
		BitmapUtils.TransferChannelsDataLinear(Rectangle, Bitmap, NewData, BitmapUtils.Direction.FromBitmapToData, Channels);
		return NewData;
	}

	unsafe static public void SetChannelsDataLinear(this Bitmap Bitmap, byte[] NewData, params BitmapChannel[] Channels)
	{
		Bitmap.SetChannelsDataLinear(NewData, Bitmap.GetFullRectangle(), Channels);
	}

	unsafe static public void SetChannelsDataLinear(this Bitmap Bitmap, byte[] NewData, Rectangle Rectangle, params BitmapChannel[] Channels)
	{
		BitmapUtils.TransferChannelsDataLinear(Rectangle, Bitmap, NewData, BitmapUtils.Direction.FromDataToBitmap, Channels);
	}

	unsafe static public byte[] GetChannelsDataInterleaved(this Bitmap Bitmap, params BitmapChannel[] Channels)
	{
		int NChannels = Channels.Length;
		int PixelCount = Bitmap.Width * Bitmap.Height;
		int BufferSize = PixelCount * NChannels;
		var Buffer = new byte[BufferSize];
		Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
		{
			var StartPtr = ((byte*)BitmapData.Scan0.ToPointer());
			fixed (byte* StartBufferPtr = &Buffer[0])
			{
				int CurrentChannel = 0;
				foreach (var Channel in Channels)
				{
					var Ptr = StartPtr + (int)Channel;
					var BufferPtr = StartBufferPtr + CurrentChannel;
					for (int n = CurrentChannel; n < BufferSize; n += NChannels, BufferPtr += NChannels, Ptr += 4)
					{
						*BufferPtr = *Ptr;
					}
					CurrentChannel++;
				}
			}
		});
		return Buffer;
	}

	static public void LockBitsUnlock(this Bitmap Bitmap, Rectangle Rectangle, PixelFormat PixelFormat, Action<BitmapData> Callback)
	{
		var BitmapData = Bitmap.LockBits(Rectangle, ImageLockMode.ReadWrite, PixelFormat);

		try
		{
			Callback(BitmapData);
		}
		finally
		{
			Bitmap.UnlockBits(BitmapData);
		}
	}

	static public void LockBitsUnlock(this Bitmap Bitmap, PixelFormat PixelFormat, Action<BitmapData> Callback)
	{
		Bitmap.LockBitsUnlock(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), PixelFormat, Callback);
	}

	static public void ForEach(this Bitmap Bitmap, Action<Color, int, int> Delegate)
	{
		int Width = Bitmap.Width, Height = Bitmap.Height;
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				Delegate(Bitmap.GetPixel(x, y), x, y);
			}
		}
	}

	static public IEnumerable<Color> Colors(this Bitmap Bitmap)
	{
		int Width = Bitmap.Width, Height = Bitmap.Height;
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				yield return Bitmap.GetPixel(x, y);
			}
		}
	}

	static public Rectangle GetFullRectangle(this Bitmap Bitmap)
	{
		return new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
	}

	static public Bitmap ConverToFormat(this Bitmap OldBitmap, PixelFormat NewPixelFormat)
	{
		var NewBitmap = new Bitmap(OldBitmap.Width, OldBitmap.Height, NewPixelFormat);
		Graphics.FromImage(NewBitmap).DrawImage(OldBitmap, Point.Empty);
		return NewBitmap;
	}
}
