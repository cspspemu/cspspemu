using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace CSharpUtils.Drawing.Extensions
{
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="colors"></param>
        public static void SetPalette(this Bitmap bitmap, IEnumerable<Color> colors)
        {
            var colorsList = colors as Color[] ?? colors.ToArray();
            var palette = BitmapUtils.GetColorPalette(colorsList.Count());

            var n = 0;
            foreach (var color in colorsList)
            {
                palette.Entries[n++] = color;
            }

            bitmap.Palette = palette;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] GetIndexedDataLinear(this Bitmap bitmap) =>
            bitmap.GetChannelsDataLinear(BitmapChannel.Indexed);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static byte[] GetIndexedDataLinear(this Bitmap bitmap, Rectangle rectangle) =>
            bitmap.GetChannelsDataLinear(rectangle, BitmapChannel.Indexed);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="newData"></param>
        public static void SetIndexedDataLinear(this Bitmap bitmap, byte[] newData) =>
            bitmap.SetChannelsDataLinear(newData, BitmapChannel.Indexed);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="rectangle"></param>
        /// <param name="newData"></param>
        public static void SetIndexedDataLinear(this Bitmap bitmap, Rectangle rectangle, byte[] newData) =>
            bitmap.SetChannelsDataLinear(newData, rectangle, BitmapChannel.Indexed);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static byte[] GetChannelsDataLinear(this Bitmap bitmap, params BitmapChannel[] channels) =>
            bitmap.GetChannelsDataLinear(bitmap.GetFullRectangle(), channels);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="rectangle"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static byte[] GetChannelsDataLinear(this Bitmap bitmap, Rectangle rectangle,
            params BitmapChannel[] channels)
        {
            var newData = new byte[rectangle.Width * rectangle.Height * channels.Length];
            BitmapUtils.TransferChannelsDataLinear(rectangle, bitmap, newData, BitmapUtils.Direction.FromBitmapToData,
                channels);
            return newData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Bitmap Duplicate(this Bitmap bitmap) => new Bitmap(bitmap, bitmap.Size);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="bitmapChannelTransfers"></param>
        /// <returns></returns>
        public static Bitmap SetChannelsDataLinear(this Bitmap bitmap,
            params BitmapChannelTransfer[] bitmapChannelTransfers)
        {
            foreach (var bitmapChannelTransfer in bitmapChannelTransfers)
            {
                bitmap.SetChannelsDataLinear(
                    bitmapChannelTransfer.Bitmap.GetChannelsDataLinear(bitmapChannelTransfer.From),
                    bitmapChannelTransfer.To);
            }
            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="newData"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static Bitmap SetChannelsDataLinear(this Bitmap bitmap, byte[] newData, params BitmapChannel[] channels)
        {
            bitmap.SetChannelsDataLinear(newData, bitmap.GetFullRectangle(), channels);
            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="newData"></param>
        /// <param name="rectangle"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static Bitmap SetChannelsDataLinear(this Bitmap bitmap, byte[] newData, Rectangle rectangle,
            params BitmapChannel[] channels)
        {
            BitmapUtils.TransferChannelsDataLinear(rectangle, bitmap, newData, BitmapUtils.Direction.FromDataToBitmap,
                channels);
            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static unsafe byte[] GetChannelsDataInterleaved(this Bitmap bitmap, params BitmapChannel[] channels)
        {
            var nChannels = channels.Length;
            var pixelCount = bitmap.Width * bitmap.Height;
            var bufferSize = pixelCount * nChannels;
            var buffer = new byte[bufferSize];
            bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, bitmapData =>
            {
                var startPtr = ((byte*) bitmapData.Scan0.ToPointer());
                fixed (byte* startBufferPtr = &buffer[0])
                {
                    var currentChannel = 0;
                    foreach (var channel in channels)
                    {
                        var ptr = startPtr + (int) channel;
                        var bufferPtr = startBufferPtr + currentChannel;
                        for (var n = currentChannel; n < bufferSize; n += nChannels, bufferPtr += nChannels, ptr += 4)
                        {
                            *bufferPtr = *ptr;
                        }
                        currentChannel++;
                    }
                }
            });
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="buffer"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static unsafe Bitmap SetChannelsDataInterleaved(this Bitmap bitmap, byte[] buffer,
            params BitmapChannel[] channels)
        {
            var nChannels = channels.Length;
            var pixelCount = bitmap.Width * bitmap.Height;
            var bufferSize = pixelCount * nChannels;
            bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, bitmapData =>
            {
                var startPtr = ((byte*) bitmapData.Scan0.ToPointer());
                fixed (byte* startBufferPtr = &buffer[0])
                {
                    var currentChannel = 0;
                    foreach (var channel in channels)
                    {
                        var ptr = startPtr + (int) channel;
                        var bufferPtr = startBufferPtr + currentChannel;
                        for (var n = currentChannel; n < bufferSize; n += nChannels, bufferPtr += nChannels, ptr += 4)
                        {
                            *ptr = *bufferPtr;
                        }
                        currentChannel++;
                    }
                }
            });
            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="rectangle"></param>
        /// <param name="pixelFormat"></param>
        /// <param name="callback"></param>
        public static void LockBitsUnlock(this Bitmap bitmap, Rectangle rectangle, PixelFormat pixelFormat,
            Action<BitmapData> callback)
        {
            var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, pixelFormat);

            try
            {
                callback(bitmapData);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="pixelFormat"></param>
        /// <param name="callback"></param>
        public static void LockBitsUnlock(this Bitmap bitmap, PixelFormat pixelFormat, Action<BitmapData> callback)
        {
            bitmap.LockBitsUnlock(new Rectangle(0, 0, bitmap.Width, bitmap.Height), pixelFormat, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="Delegate"></param>
        public static void ForEach(this Bitmap bitmap, Action<Color, int, int> Delegate)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    Delegate(bitmap.GetPixel(x, y), x, y);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="Delegate"></param>
        public static unsafe void Shader(this Bitmap bitmap, Func<ArgbRev, int, int, ArgbRev> Delegate)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, bitmapData =>
            {
                for (var y = 0; y < height; y++)
                {
                    var ptr = (ArgbRev*) (((byte*) bitmapData.Scan0.ToPointer()) + bitmapData.Stride * y);
                    for (var x = 0; x < width; x++)
                    {
                        *ptr = Delegate(*ptr, x, y);
                        ptr++;
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static IEnumerable<Color> Colors(this Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    yield return bitmap.GetPixel(x, y);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Rectangle GetFullRectangle(this Bitmap bitmap)
        {
            return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldBitmap"></param>
        /// <param name="newPixelFormat"></param>
        /// <returns></returns>
        public static Bitmap ConvertToFormat(this Bitmap oldBitmap, PixelFormat newPixelFormat)
        {
            var newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height, newPixelFormat);
            Graphics.FromImage(newBitmap).DrawImage(oldBitmap, Point.Empty);
            return newBitmap;
        }
    }
}