using System;
using System.Drawing;
using System.Drawing.Imaging;
using CSharpUtils.Drawing.Extensions;

namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class BitmapUtils
    {
        /// <summary>
        /// 
        /// </summary>
        public static BitmapChannel[] Rgb => new[]
        {
            BitmapChannel.Red,
            BitmapChannel.Green,
            BitmapChannel.Blue,
        };

        /// <summary>
        /// 
        /// </summary>
        public struct CompareResult
        {
            /// <summary>
            /// 
            /// </summary>
            public bool Equal;

            /// <summary>
            /// 
            /// </summary>
            public int PixelTotalDifference;

            /// <summary>
            /// 
            /// </summary>
            public int DifferentPixelCount;

            /// <summary>
            /// 
            /// </summary>
            public int TotalPixelCount;

            /// <summary>
            /// 
            /// </summary>
            public double PixelTotalDifferencePercentage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceBitmap"></param>
        /// <param name="outputBitmap"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static CompareResult CompareBitmaps(Bitmap referenceBitmap, Bitmap outputBitmap, double threshold = 0.01)
        {
            var compareResult = default(CompareResult);

            if (referenceBitmap.Size != outputBitmap.Size) return compareResult;

            compareResult.PixelTotalDifference = 0;
            compareResult.DifferentPixelCount = 0;
            compareResult.TotalPixelCount = 0;
            for (var y = 0; y < referenceBitmap.Height; y++)
            {
                for (var x = 0; x < referenceBitmap.Width; x++)
                {
                    var colorReference = referenceBitmap.GetPixel(x, y);
                    var colorOutput = outputBitmap.GetPixel(x, y);
                    var difference3 = (
                        Math.Abs(colorOutput.R - colorReference.R) +
                        Math.Abs(colorOutput.G - colorReference.G) +
                        Math.Abs(colorOutput.B - colorReference.B)
                    );
                    compareResult.PixelTotalDifference += difference3;
                    if (difference3 > 6)
                    {
                        compareResult.DifferentPixelCount++;
                    }
                    compareResult.TotalPixelCount++;
                }
            }

            var pixelTotalDifferencePercentage = (double) compareResult.DifferentPixelCount * 100 /
                                                 compareResult.TotalPixelCount;
            compareResult.Equal = (pixelTotalDifferencePercentage < threshold);

            return compareResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nColors"></param>
        /// <returns></returns>
        public static ColorPalette GetColorPalette(int nColors)
        {
            // Assume monochrome image.
            PixelFormat bitscolordepth = PixelFormat.Format1bppIndexed;
            ColorPalette palette; // The Palette we are stealing
            Bitmap bitmap; // The source of the stolen palette

            // Determine number of colors.
            if (nColors > 2) bitscolordepth = PixelFormat.Format4bppIndexed;
            if (nColors > 16) bitscolordepth = PixelFormat.Format8bppIndexed;

            // Make a new Bitmap object to get its Palette.
            bitmap = new Bitmap(1, 1, bitscolordepth);

            palette = bitmap.Palette; // Grab the palette

            bitmap.Dispose(); // cleanup the source Bitmap

            return palette; // Send the palette back
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// 
            /// </summary>
            FromBitmapToData = 0,

            /// <summary>
            /// 
            /// </summary>
            FromDataToBitmap = 1,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="newData"></param>
        /// <param name="direction"></param>
        /// <param name="channels"></param>
        public static void TransferChannelsDataLinear(Bitmap bitmap, byte[] newData, Direction direction,
            params BitmapChannel[] channels)
        {
            TransferChannelsDataLinear(bitmap.GetFullRectangle(), bitmap, newData, direction, channels);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="newDataPtr"></param>
        /// <param name="direction"></param>
        /// <param name="channels"></param>
        public static unsafe void TransferChannelsDataLinear(Bitmap bitmap, byte* newDataPtr, Direction direction,
            params BitmapChannel[] channels)
        {
            TransferChannelsDataLinear(bitmap.GetFullRectangle(), bitmap, newDataPtr, direction, channels);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="bitmap"></param>
        /// <param name="newData"></param>
        /// <param name="direction"></param>
        /// <param name="channels"></param>
        public static unsafe void TransferChannelsDataLinear(Rectangle rectangle, Bitmap bitmap, byte[] newData,
            Direction direction, params BitmapChannel[] channels)
        {
            fixed (byte* newDataPtr = &newData[0])
            {
                TransferChannelsDataLinear(rectangle, bitmap, newDataPtr, direction, channels);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="bitmap"></param>
        /// <param name="newDataPtr"></param>
        /// <param name="direction"></param>
        /// <param name="channels"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static unsafe void TransferChannelsDataLinear(Rectangle rectangle, Bitmap bitmap, byte* newDataPtr,
            Direction direction, params BitmapChannel[] channels)
        {
            var widthHeight = bitmap.Width * bitmap.Height;
            var fullRectangle = bitmap.GetFullRectangle();
            if (!fullRectangle.Contains(rectangle.Location))
                throw (new InvalidOperationException("TransferChannelsDataLinear"));
            if (!fullRectangle.Contains(rectangle.Location + rectangle.Size - new Size(1, 1)))
                throw (new InvalidOperationException("TransferChannelsDataLinear"));

            var numberOfChannels = 1;
            foreach (var channel in channels)
            {
                if (channel != BitmapChannel.Indexed)
                {
                    numberOfChannels = 4;
                    break;
                }
            }

            bitmap.LockBitsUnlock((numberOfChannels == 1) ? PixelFormat.Format8bppIndexed : PixelFormat.Format32bppArgb,
                bitmapData =>
                {
                    var bitmapDataScan0 = (byte*) bitmapData.Scan0.ToPointer();
                    var width = bitmap.Width;
                    var height = bitmap.Height;
                    var stride = bitmapData.Stride;
                    if (numberOfChannels == 1)
                    {
                        stride = width;
                    }

                    var rectangleTop = rectangle.Top;
                    var rectangleBottom = rectangle.Bottom;
                    var rectangleLeft = rectangle.Left;
                    var rectangleRight = rectangle.Right;

                    var outputPtrStart = newDataPtr;
                    {
                        var outputPtr = outputPtrStart;
                        foreach (var channel in channels)
                        {
                            for (var y = rectangleTop; y < rectangleBottom; y++)
                            {
                                var inputPtr = bitmapDataScan0 + stride * y;
                                if (numberOfChannels != 1)
                                {
                                    inputPtr = inputPtr + (int) channel;
                                }
                                inputPtr += numberOfChannels * rectangleLeft;
                                for (var x = rectangleLeft; x < rectangleRight; x++)
                                {
                                    if (direction == Direction.FromBitmapToData)
                                    {
                                        *outputPtr = *inputPtr;
                                    }
                                    else
                                    {
                                        *inputPtr = *outputPtr;
                                    }
                                    outputPtr++;
                                    inputPtr += numberOfChannels;
                                }
                            }
                        }
                    }
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="bitmap"></param>
        /// <param name="newDataPtr"></param>
        /// <param name="direction"></param>
        /// <param name="channels"></param>
        public static unsafe void TransferChannelsDataInterleaved(Rectangle rectangle, Bitmap bitmap, byte* newDataPtr,
            Direction direction, params BitmapChannel[] channels)
        {
            var numberOfChannels = 1;
            foreach (var channel in channels)
            {
                if (channel != BitmapChannel.Indexed)
                {
                    numberOfChannels = 4;
                    break;
                }
            }

            bitmap.LockBitsUnlock(rectangle,
                (numberOfChannels == 1) ? PixelFormat.Format8bppIndexed : PixelFormat.Format32bppArgb, bitmapData =>
                {
                    for (var y = 0; y < bitmapData.Height; y++)
                    {
                        var bitmapPtr = ((byte*) bitmapData.Scan0.ToPointer()) + bitmapData.Stride * y;
                        var dataPtr = newDataPtr + (numberOfChannels * bitmapData.Width) * y;
                        var z = 0;
                        for (var x = 0; x < bitmapData.Width; x++)
                        {
                            for (var c = 0; c < numberOfChannels; c++)
                            {
                                var dataPtrPtr = &dataPtr[z + c];
                                var bitmapPtrPtr = &bitmapPtr[z + (int) channels[c]];

                                if (direction == Direction.FromBitmapToData)
                                {
                                    *dataPtrPtr = *bitmapPtrPtr;
                                }
                                else
                                {
                                    *bitmapPtrPtr = *dataPtrPtr;
                                }
                            }
                            z += numberOfChannels;
                        }
                    }
                });
        }
    }
}