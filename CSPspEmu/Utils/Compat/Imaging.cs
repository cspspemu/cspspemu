using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace System.Drawing
{
    public class Graphics
    {
        static public BufferGraphics FromImage(Bitmap bitmap)
        {
            throw new NotImplementedException();
        }

        public void Clear(Color color)
        {
            
        }
    }

    public class BufferGraphics : Graphics
    {
        public void DrawImage(Bitmap @in, Point point)
        {
            throw new NotImplementedException();
        }
    }
    
    public class Image
    {
        static public Image FromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public static string FromFile(string file)
        {
            throw new NotImplementedException();
        }

        public void Save(MemoryStream memoryStream, ImageFormat format)
        {
            throw new NotImplementedException();
        }
    }
    
    public class Bitmap : IDisposable
    {
        public Size Size = new Size(100, 100);
        public int Width => Size.Width;
        public int Height => Size.Height;
        public PixelFormat PixelFormat { get; private set; }

        public ColorPalette Palette = null;
        public Bitmap(string filename)
        {
            throw new Exception();
        }
        public Bitmap(Bitmap bitmap, Size size)
        {
            throw new Exception();
        }
        public Bitmap(int width, int height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            throw new Exception();
        }

        public Color GetPixel(int x, int y)
        {
            throw new Exception();
        }

        public void SetPixel(int x, int y, Color color)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public BitmapData LockBits(Rectangle rectangle, ImageLockMode lockMode, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        public void UnlockBits(BitmapData bitmapData)
        {
            throw new NotImplementedException();
        }

        public void Save(string outputFile, ImageFormat? format = null)
        {
            //File.WriteAllBytes(outputFile);
            throw new NotImplementedException();
        }
    }

    public class BitmapData
    {
        public IntPtr Scan0;
        public int Stride;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ColorPalette
    {
        public Color[] Entries;
    }

    public enum ImageLockMode
    {
        ReadWrite,
        WriteOnly,
        ReadOnly
    }

    namespace Imaging
    {
        public enum PixelFormat
        {
            Format1bppIndexed, Format4bppIndexed, Format8bppIndexed, Format32bppArgb, Format24bppRgb
        }

        public enum ImageFormat
        {
            Png
        }
    }
}