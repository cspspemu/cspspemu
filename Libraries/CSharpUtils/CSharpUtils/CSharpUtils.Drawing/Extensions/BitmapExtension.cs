using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CSharpUtils;
using CSharpUtils.Drawing;

/// <summary>
/// 
/// </summary>
public class BitmapChannelTransfer
{
	/// <summary>
	/// 
	/// </summary>
	public Bitmap Bitmap;

	/// <summary>
	/// 
	/// </summary>
	public BitmapChannel From;

	/// <summary>
	/// 
	/// </summary>
	public BitmapChannel To;
}

/// <summary>
/// 
/// </summary>
public static class BitmapExtension
{
	public static void SetPalette(this Bitmap Bitmap, IEnumerable<Color> Colors)
	{
		ColorPalette Palette = BitmapUtils.GetColorPalette(Colors.Count());

		int n = 0;
		foreach (var Color in Colors)
		{
			Palette.Entries[n++] = Color;
		}

		Bitmap.Palette = Palette;
	}

	public static byte[] GetIndexedDataLinear(this Bitmap Bitmap)
	{
		return Bitmap.GetChannelsDataLinear(BitmapChannel.Indexed);
	}

	public static byte[] GetIndexedDataLinear(this Bitmap Bitmap, Rectangle Rectangle)
	{
		return Bitmap.GetChannelsDataLinear(Rectangle, BitmapChannel.Indexed);
	}

	public static void SetIndexedDataLinear(this Bitmap Bitmap, byte[] NewData)
	{
		Bitmap.SetChannelsDataLinear(NewData, BitmapChannel.Indexed);
	}

	public static void SetIndexedDataLinear(this Bitmap Bitmap, Rectangle Rectangle, byte[] NewData)
	{
		Bitmap.SetChannelsDataLinear(NewData, Rectangle, BitmapChannel.Indexed);
	}

	public static byte[] GetChannelsDataLinear(this Bitmap Bitmap, params BitmapChannel[] Channels)
	{
		return Bitmap.GetChannelsDataLinear(Bitmap.GetFullRectangle(), Channels);
	}

	public static byte[] GetChannelsDataLinear(this Bitmap Bitmap, Rectangle Rectangle, params BitmapChannel[] Channels)
	{
		var NewData = new byte[Rectangle.Width * Rectangle.Height * Channels.Length];
		BitmapUtils.TransferChannelsDataLinear(Rectangle, Bitmap, NewData, BitmapUtils.Direction.FromBitmapToData, Channels);
		return NewData;
	}

	public static Bitmap Duplicate(this Bitmap Bitmap)
	{
		return new Bitmap(Bitmap, Bitmap.Size);
	}

	public static Bitmap SetChannelsDataLinear(this Bitmap Bitmap, params BitmapChannelTransfer[] BitmapChannelTransfers)
	{
		foreach (var BitmapChannelTransfer in BitmapChannelTransfers)
		{
			Bitmap.SetChannelsDataLinear(BitmapChannelTransfer.Bitmap.GetChannelsDataLinear(BitmapChannelTransfer.From), BitmapChannelTransfer.To);
		}
		return Bitmap;
	}

	public static Bitmap SetChannelsDataLinear(this Bitmap Bitmap, byte[] NewData, params BitmapChannel[] Channels)
	{
		Bitmap.SetChannelsDataLinear(NewData, Bitmap.GetFullRectangle(), Channels);
		return Bitmap;
	}

	public static Bitmap SetChannelsDataLinear(this Bitmap Bitmap, byte[] NewData, Rectangle Rectangle, params BitmapChannel[] Channels)
	{
		BitmapUtils.TransferChannelsDataLinear(Rectangle, Bitmap, NewData, BitmapUtils.Direction.FromDataToBitmap, Channels);
		return Bitmap;
	}

	public unsafe static byte[] GetChannelsDataInterleaved(this Bitmap Bitmap, params BitmapChannel[] Channels)
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

	public unsafe static Bitmap SetChannelsDataInterleaved(this Bitmap Bitmap, byte[] Buffer, params BitmapChannel[] Channels)
	{
		int NChannels = Channels.Length;
		int PixelCount = Bitmap.Width * Bitmap.Height;
		int BufferSize = PixelCount * NChannels;
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
						*Ptr = * BufferPtr;
					}
					CurrentChannel++;
				}
			}
		});
		return Bitmap;
	}

	public static void LockBitsUnlock(this Bitmap Bitmap, Rectangle Rectangle, PixelFormat PixelFormat, Action<BitmapData> Callback)
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

	public static void LockBitsUnlock(this Bitmap Bitmap, PixelFormat PixelFormat, Action<BitmapData> Callback)
	{
		Bitmap.LockBitsUnlock(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), PixelFormat, Callback);
	}

	public static void ForEach(this Bitmap Bitmap, Action<Color, int, int> Delegate)
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

	unsafe public static void Shader(this Bitmap Bitmap, Func<ARGB_Rev, int, int, ARGB_Rev> Delegate)
	{
		int Width = Bitmap.Width, Height = Bitmap.Height;
		Bitmap.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
		{
			for (int y = 0; y < Height; y++)
			{
				var Ptr = (ARGB_Rev*)(((byte*)BitmapData.Scan0.ToPointer()) + BitmapData.Stride * y);
				for (int x = 0; x < Width; x++)
				{
					*Ptr = Delegate(*Ptr, x, y);
					Ptr++;
				}
			}
		});
	}

	public static IEnumerable<Color> Colors(this Bitmap Bitmap)
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

	public static Rectangle GetFullRectangle(this Bitmap Bitmap)
	{
		return new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
	}

	public static Bitmap ConvertToFormat(this Bitmap OldBitmap, PixelFormat NewPixelFormat)
	{
		var NewBitmap = new Bitmap(OldBitmap.Width, OldBitmap.Height, NewPixelFormat);
		Graphics.FromImage(NewBitmap).DrawImage(OldBitmap, Point.Empty);
		return NewBitmap;
	}
}
