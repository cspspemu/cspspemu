using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using CSharpUtils;

namespace CSharpUtils
{
	public class BitmapUtils
	{
		static public BitmapChannel[] RGB
		{
			get
			{
				return new BitmapChannel[] {
					BitmapChannel.Red,
					BitmapChannel.Green,
					BitmapChannel.Blue,
				};
			}
		}

		static public ColorPalette GetColorPalette(int nColors)
		{
			// Assume monochrome image.
			PixelFormat bitscolordepth = PixelFormat.Format1bppIndexed;
			ColorPalette palette;    // The Palette we are stealing
			Bitmap bitmap;     // The source of the stolen palette

			// Determine number of colors.
			if (nColors > 2) bitscolordepth = PixelFormat.Format4bppIndexed;
			if (nColors > 16) bitscolordepth = PixelFormat.Format8bppIndexed;

			// Make a new Bitmap object to get its Palette.
			bitmap = new Bitmap(1, 1, bitscolordepth);

			palette = bitmap.Palette;   // Grab the palette

			bitmap.Dispose();           // cleanup the source Bitmap

			return palette;             // Send the palette back
		}

		public enum Direction {
			FromBitmapToData = 0,
			FromDataToBitmap = 1,
		}

		unsafe static public void TransferChannelsDataLinear(Bitmap Bitmap, byte[] NewData, Direction Direction, params BitmapChannel[] Channels)
		{
			TransferChannelsDataLinear(Bitmap.GetFullRectangle(), Bitmap, NewData, Direction, Channels);
		}

		unsafe static public void TransferChannelsDataLinear(Bitmap Bitmap, byte* NewDataPtr, Direction Direction, params BitmapChannel[] Channels)
		{
			TransferChannelsDataLinear(Bitmap.GetFullRectangle(), Bitmap, NewDataPtr, Direction, Channels);
		}

		unsafe static public void TransferChannelsDataLinear(Rectangle Rectangle, Bitmap Bitmap, byte[] NewData, Direction Direction, params BitmapChannel[] Channels)
		{
			fixed (byte* NewDataPtr = &NewData[0])
			{
				TransferChannelsDataLinear(Rectangle, Bitmap, NewDataPtr, Direction, Channels);
			}
		}

		unsafe static public void TransferChannelsDataLinear(Rectangle Rectangle, Bitmap Bitmap, byte* NewDataPtr, Direction Direction, params BitmapChannel[] Channels)
		{
			int WidthHeight = Bitmap.Width * Bitmap.Height;
			var FullRectangle = Bitmap.GetFullRectangle();
			if (!FullRectangle.Contains(Rectangle.Location)) throw(new InvalidProgramException());
			if (!FullRectangle.Contains(Rectangle.Location + Rectangle.Size - new Size(1, 1))) throw (new InvalidProgramException());

			int NumberOfChannels = 1;
			foreach (var Channel in Channels)
			{
				if (Channel != BitmapChannel.Indexed)
				{
					NumberOfChannels = 4;
					break;
				}
			}

			Bitmap.LockBitsUnlock((NumberOfChannels == 1) ? PixelFormat.Format8bppIndexed : PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				byte* BitmapDataScan0 = (byte*)BitmapData.Scan0.ToPointer();
				int Width = Bitmap.Width;
				int Height = Bitmap.Height;
				int Stride = BitmapData.Stride;
				if (NumberOfChannels == 1)
				{
					Stride = Width;
				}

				int RectangleTop = Rectangle.Top;
				int RectangleBottom = Rectangle.Bottom;
				int RectangleLeft = Rectangle.Left;
				int RectangleRight = Rectangle.Right;

				byte* OutputPtrStart = NewDataPtr;
				{
					byte* OutputPtr = OutputPtrStart;
					foreach (var Channel in Channels)
					{
						for (int y = RectangleTop; y < RectangleBottom; y++)
						{
							byte* InputPtr = BitmapDataScan0 + Stride * y;
							if (NumberOfChannels != 1)
							{
								InputPtr = InputPtr + (int)Channel;
							}
							InputPtr += NumberOfChannels * RectangleLeft;
							for (int x = RectangleLeft; x < RectangleRight; x++)
							{
								if (Direction == Direction.FromBitmapToData)
								{
									*OutputPtr = *InputPtr;
								}
								else
								{
									*InputPtr  = * OutputPtr;
								}
								OutputPtr++;
								InputPtr += NumberOfChannels;
							}
						}
					}
				}
			});
		}

		public static unsafe void TransferChannelsDataInterleaved(Rectangle Rectangle, Bitmap Bitmap, byte* NewDataPtr, Direction Direction, params BitmapChannel[] Channels)
		{
			int NumberOfChannels = 1;
			foreach (var Channel in Channels)
			{
				if (Channel != BitmapChannel.Indexed)
				{
					NumberOfChannels = 4;
					break;
				}
			}

			Bitmap.LockBitsUnlock(Rectangle, (NumberOfChannels == 1) ? PixelFormat.Format8bppIndexed : PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				for (int y = 0; y < BitmapData.Height; y++)
				{
					byte* BitmapPtr = ((byte*)BitmapData.Scan0.ToPointer()) + BitmapData.Stride * y;
					byte* DataPtr = NewDataPtr + (NumberOfChannels * BitmapData.Width) * y;
					int z = 0;
					for (int x = 0; x < BitmapData.Width; x++)
					{
						for (int c = 0; c < NumberOfChannels; c++)
						{
							byte* DataPtrPtr = &DataPtr[z + c];
							byte* BitmapPtrPtr = &BitmapPtr[z + (int)Channels[c]];

							if (Direction == BitmapUtils.Direction.FromBitmapToData)
							{
								*DataPtrPtr = *BitmapPtrPtr;
							}
							else
							{
								*BitmapPtrPtr  = * DataPtrPtr;
							}
						}
						z += NumberOfChannels;
					}
				}
			});
		}
	}
}
