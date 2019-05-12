using System.Drawing;
using System.Drawing.Imaging;
using CSharpUtils.Drawing;
using CSharpUtils.Drawing.Extensions;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    public class BitmapExtensionTest
    {
        [Fact]
        public void GetChannelsDataLinearTest()
        {
            Bitmap Bitmap = new Bitmap(2, 2);
            Bitmap.SetPixel(0, 0, Color.FromArgb(0x00, 0x04, 0x08, 0x0C));
            Bitmap.SetPixel(1, 0, Color.FromArgb(0x01, 0x05, 0x09, 0x0D));
            Bitmap.SetPixel(0, 1, Color.FromArgb(0x02, 0x06, 0x0A, 0x0E));
            Bitmap.SetPixel(1, 1, Color.FromArgb(0x03, 0x07, 0x0B, 0x0F));
            Assert.Equal(
                "000102030405060708090a0b0c0d0e0f",
                Bitmap.GetChannelsDataLinear(BitmapChannel.Alpha, BitmapChannel.Red, BitmapChannel.Green,
                    BitmapChannel.Blue).ToHexString()
            );
        }

        [Fact]
        public void SetPaletteTest()
        {
            Bitmap Bitmap = new Bitmap(2, 2, PixelFormat.Format8bppIndexed);
            var Colors = new[]
            {
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 255, 255, 255),
                Color.FromArgb(255, 255, 0, 0),
                Color.FromArgb(255, 0, 255, 0),
                Color.FromArgb(255, 0, 0, 255),
            };
            Bitmap.SetPalette(Colors);
            //Assert.Inconclusive();
        }


        [Fact(Skip = "Fails on mono")]
        public unsafe void GetIndexedDataLinearTest()
        {
            Bitmap Bitmap = new Bitmap(3, 3, PixelFormat.Format8bppIndexed);

            Bitmap.LockBitsUnlock(PixelFormat.Format8bppIndexed, (BitmapData) =>
            {
                byte* Data = (byte*) BitmapData.Scan0.ToPointer();
                for (int n = 0; n < 9; n++) Data[n] = (byte) n;
            });

            Assert.Equal(
                "000102030405060708",
                Bitmap.GetIndexedDataLinear().ToHexString()
            );
        }

        [Fact(Skip = "Fails on mono")]
        public void GetIndexedDataLinearRectangleTest()
        {
            Bitmap Bitmap = new Bitmap(3, 3, PixelFormat.Format8bppIndexed);

            Bitmap.SetIndexedDataLinear(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8});

            Assert.Equal(
                "04050708",
                Bitmap.GetIndexedDataLinear(new Rectangle(1, 1, 2, 2)).ToHexString()
            );
        }

        [Fact(Skip = "Fails on mono")]
        public void SetIndexedDataLinearTest()
        {
            Bitmap Bitmap = new Bitmap(3, 3, PixelFormat.Format8bppIndexed);

            Bitmap.SetIndexedDataLinear(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8});

            Assert.Equal(
                "000102030405060708",
                Bitmap.GetIndexedDataLinear().ToHexString()
            );
        }

        [Fact]
        public void SetChannelsDataLinearTest()
        {
            Bitmap Bitmap = new Bitmap(2, 2);

            Bitmap.SetChannelsDataLinear(new byte[] {1, 0, 0, 0, 2, 2, 0, 0}, BitmapChannel.Red, BitmapChannel.Blue);
            Bitmap.SetChannelsDataLinear(new byte[] {0, 3, 0, 3}, BitmapChannel.Green);

            Assert.Equal(
                "00030003020200000100000002020000",
                Bitmap.GetChannelsDataLinear(BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Red,
                    BitmapChannel.Blue).ToHexString()
            );
        }
    }
}