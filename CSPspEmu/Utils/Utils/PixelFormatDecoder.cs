using System;
using CSharpUtils;
using CSharpUtils.Drawing;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Utils.Utils
{
    public sealed unsafe class PixelFormatDecoder
    {
        internal PixelFormatDecoder()
        {
        }

        public struct Dxt1Block
        {
            public uint ColorLookup;
            public ushort Color0;
            public ushort Color1;
        }

        public struct Dxt3Block
        {
            public uint ColorLookup;
            public ushort Color0;
            public ushort Color1;
            public ulong Alpha;
        }

        public struct Dxt5Block
        {
            public uint ColorLookup;
            public ushort Color0;
            public ushort Color1;
            public ulong Alpha;
        }

        static readonly double[] Sizes =
        {
            // Rgba
            2, 2, 2, 4,
            // Palette
            0.5f, 1, 2, 4,
            // Compressed
            1, 1, 1
        };

        public static int GetPixelsBits(GuPixelFormats pixelFormat)
        {
            return (int) (Sizes[(int) pixelFormat] * 8);
        }

        public static int GetPixelsSize(GuPixelFormats pixelFormat, int pixelCount)
        {
            return (int) (Sizes[(int) pixelFormat] * pixelCount);
        }

        void* _input;
        byte* _inputByte;
        ushort* _inputShort;
        uint* _inputInt;
        OutputPixel* _output;
        int _width;
        int _height;
        void* _palette;
        GuPixelFormats _paletteType;
        int _paletteCount;
        int _paletteStart;
        int _paletteShift;
        int _paletteMask;
        int _strideWidth;

        public static ColorFormat ColorFormatFromPixelFormat(GuPixelFormats pixelFormat)
        {
            switch (pixelFormat)
            {
                case GuPixelFormats.Rgba8888: return ColorFormats.Rgba8888;
                case GuPixelFormats.Rgba5551: return ColorFormats.Rgba5551;
                case GuPixelFormats.Rgba5650: return ColorFormats.Rgba5650;
                case GuPixelFormats.Rgba4444: return ColorFormats.Rgba4444;
                default: throw(new NotImplementedException("Not implemented " + pixelFormat));
            }
        }

        /*
        public static uint EncodePixel(GuPixelFormats PixelFormat, OutputPixel Color)
        {
            return ColorFormatFromPixelFormat(PixelFormat).Encode(Color.R, Color.G, Color.B, Color.A);
        }

        public static OutputPixel DecodePixel(GuPixelFormats PixelFormat, uint Value)
        {
            throw new NotImplementedException();
        }
        */

        public static void Decode(GuPixelFormats pixelFormat, void* input, OutputPixel* output, int width, int height,
            void* palette = null, GuPixelFormats paletteType = GuPixelFormats.None, int paletteCount = 0,
            int paletteStart = 0, int paletteShift = 0, int paletteMask = 0xFF, int strideWidth = -1,
            bool ignoreAlpha = false)
        {
            if (strideWidth == -1) strideWidth = GetPixelsSize(pixelFormat, width);
            var pixelFormatInt = (int) pixelFormat;
            var pixelFormatDecoder = new PixelFormatDecoder()
            {
                _input = input,
                _inputByte = (byte*) input,
                _inputShort = (ushort*) input,
                _inputInt = (uint*) input,
                _output = output,
                _strideWidth = strideWidth,
                _width = width,
                _height = height,
                _palette = palette,
                _paletteType = paletteType,
                _paletteCount = paletteCount,
                _paletteStart = paletteStart,
                _paletteShift = paletteShift,
                _paletteMask = paletteMask,
            };
            //Console.WriteLine(PixelFormat);
            switch (pixelFormat)
            {
                case GuPixelFormats.Rgba5650:
                    pixelFormatDecoder.Decode_RGBA_5650();
                    break;
                case GuPixelFormats.Rgba5551:
                    pixelFormatDecoder.Decode_RGBA_5551();
                    break;
                case GuPixelFormats.Rgba4444:
                    pixelFormatDecoder.Decode_RGBA_4444();
                    break;
                case GuPixelFormats.Rgba8888:
                    pixelFormatDecoder.Decode_RGBA_8888();
                    break;
                case GuPixelFormats.PaletteT4:
                    pixelFormatDecoder.Decode_PALETTE_T4();
                    break;
                case GuPixelFormats.PaletteT8:
                    pixelFormatDecoder.Decode_PALETTE_T8();
                    break;
                case GuPixelFormats.PaletteT16:
                    pixelFormatDecoder.Decode_PALETTE_T16();
                    break;
                case GuPixelFormats.PaletteT32:
                    pixelFormatDecoder.Decode_PALETTE_T32();
                    break;
                case GuPixelFormats.CompressedDxt1:
                    pixelFormatDecoder.Decode_COMPRESSED_DXT1();
                    break;
                case GuPixelFormats.CompressedDxt3:
                    pixelFormatDecoder.Decode_COMPRESSED_DXT3();
                    break;
                case GuPixelFormats.CompressedDxt5:
                    pixelFormatDecoder.Decode_COMPRESSED_DXT5();
                    break;
                default: throw(new InvalidOperationException());
            }
            if (ignoreAlpha)
            {
                for (int y = 0, n = 0; y < height; y++) for (int x = 0; x < width; x++, n++) output[n].A = 0xFF;
            }
            //DecoderCallbackTable[PixelFormatInt](Input, Output, PixelCount, Width, Palette, PaletteType, PaletteCount, PaletteStart, PaletteShift, PaletteMask);
        }

        private void _Decode_Unimplemented()
        {
            for (int y = 0, n = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++, n++)
                {
                    OutputPixel outputPixel;
                    outputPixel.R = 0xFF;
                    outputPixel.G = (byte) (((n & 1) == 0) ? 0xFF : 0x00);
                    outputPixel.B = 0x00;
                    outputPixel.A = 0xFF;
                    _output[n] = outputPixel;
                }
            }
        }

        private void Decode_COMPRESSED_DXT5()
        {
            //Console.Error.WriteLine("Not Implemented: Decode_COMPRESSED_DXT5");
            //throw new NotImplementedException();

            //_Decode_Unimplemented();

            var colors = new OutputPixel[4];

            var ni = 0;
            for (var y = 0; y < _height; y += 4)
            {
                for (var x = 0; x < _width; x += 4, ni++)
                {
                    var block = ((Dxt5Block*) _inputByte)[ni];
                    colors[0] = Decode_RGBA_5650_Pixel(block.Color0)
                        .Transform((r, g, b, a) => OutputPixel.FromRgba(b, g, r, a));
                    colors[1] = Decode_RGBA_5650_Pixel(block.Color1)
                        .Transform((r, g, b, a) => OutputPixel.FromRgba(b, g, r, a));
                    colors[2] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                        (a, b) => (byte) (((a * 2) / 3) + ((b * 1) / 3)));
                    colors[3] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                        (a, b) => (byte) (((a * 1) / 3) + ((b * 2) / 3)));

                    // Create Alpha Lookup
                    var alphaLookup = new byte[8];
                    var alphas = (ushort) (block.Alpha >> 48);
                    var alpha0 = (byte) ((alphas >> 0) & 0xFF);
                    var alpha1 = (byte) ((alphas >> 8) & 0xFF);

                    alphaLookup[0] = alpha0;
                    alphaLookup[1] = alpha1;
                    if (alpha0 > alpha1)
                    {
                        alphaLookup[2] = (byte) ((6 * alpha0 + alpha1) / 7);
                        alphaLookup[3] = (byte) ((5 * alpha0 + 2 * alpha1) / 7);
                        alphaLookup[4] = (byte) ((4 * alpha0 + 3 * alpha1) / 7);
                        alphaLookup[5] = (byte) ((3 * alpha0 + 4 * alpha1) / 7);
                        alphaLookup[6] = (byte) ((2 * alpha0 + 5 * alpha1) / 7);
                        alphaLookup[7] = (byte) ((alpha0 + 6 * alpha1) / 7);
                    }
                    else
                    {
                        alphaLookup[2] = (byte) ((4 * alpha0 + alpha1) / 5);
                        alphaLookup[3] = (byte) ((3 * alpha0 + 2 * alpha1) / 5);
                        alphaLookup[4] = (byte) ((2 * alpha0 + 3 * alpha1) / 5);
                        alphaLookup[5] = (byte) ((alpha0 + 4 * alpha1) / 5);
                        alphaLookup[6] = (byte) (0x00);
                        alphaLookup[7] = (byte) (0xFF);
                    }

                    var no = 0;
                    for (var y2 = 0; y2 < 4; y2++)
                    {
                        for (var x2 = 0; x2 < 4; x2++, no++)
                        {
                            var alpha = alphaLookup[((block.Alpha >> (3 * no)) & 0x7)];
                            var color = ((block.ColorLookup >> (2 * no)) & 0x3);

                            var rx = x + x2;
                            var ry = y + y2;
                            var n = ry * _width + rx;

                            _output[n] = colors[color];
                            _output[n].A = alpha;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// DXT2 and DXT3 (collectively also known as Block Compression 2 or BC2) converts 16 input pixels
        /// (corresponding to a 4x4 pixel block) into 128 bits of output, consisting of 64 bits of alpha channel data
        /// (4 bits for each pixel) followed by 64 bits of color data, encoded the same way as DXT1 (with the exception
        /// that the 4 color version of the DXT1 algorithm is always used instead of deciding which version to use based
        /// on the relative values of  and ). In DXT2, the color data is interpreted as being premultiplied by alpha, in
        /// DXT3 it is interpreted as not having been premultiplied by alpha. Typically DXT2/3 are well suited to images
        /// with sharp alpha transitions, between translucent and opaque areas.
        /// </summary>
        private void Decode_COMPRESSED_DXT3()
        {
            var colors = new OutputPixel[4];

            var ni = 0;
            for (var y = 0; y < _height; y += 4)
            {
                for (var x = 0; x < _width; x += 4, ni++)
                {
                    var block = ((Dxt3Block*) _inputByte)[ni];
                    colors[0] = Decode_RGBA_5650_Pixel(block.Color0)
                        .Transform((r, g, b, a) => OutputPixel.FromRgba(b, g, r, a));
                    colors[1] = Decode_RGBA_5650_Pixel(block.Color1)
                        .Transform((r, g, b, a) => OutputPixel.FromRgba(b, g, r, a));
                    colors[2] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                        (a, b) => (byte) (((a * 2) / 3) + ((b * 1) / 3)));
                    colors[3] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                        (a, b) => (byte) (((a * 1) / 3) + ((b * 2) / 3)));

                    var no = 0;
                    for (var y2 = 0; y2 < 4; y2++)
                    {
                        for (var x2 = 0; x2 < 4; x2++, no++)
                        {
                            var alpha = (block.Alpha >> (4 * no)) & 0xF;
                            var color = (block.ColorLookup >> (2 * no)) & 0x3;

                            var rx = (x + x2);
                            var ry = (y + y2);
                            var n = ry * _width + rx;

                            _output[n] = colors[color];
                            _output[n].A = (byte) ((alpha * 0xFF) / 0xF);
                        }
                    }
                }
            }
        }

        private void Decode_COMPRESSED_DXT1()
        {
            var colors = new OutputPixel[4];

            for (int y = 0, ni = 0; y < _height; y += 4)
            {
                for (int x = 0; x < _width; x += 4, ni++)
                {
                    var block = ((Dxt1Block*) _inputByte)[ni];

                    colors[0] = Decode_RGBA_5650_Pixel(block.Color0)
                        .Transform((r, g, b, a) => OutputPixel.FromRgba(b, g, r, a));
                    colors[1] = Decode_RGBA_5650_Pixel(block.Color1)
                        .Transform((r, g, b, a) => OutputPixel.FromRgba(b, g, r, a));

                    if (block.Color0 > block.Color1)
                    {
                        colors[2] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                            (a, b) => (byte) (((a * 2) / 3) + ((b * 1) / 3)));
                        colors[3] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                            (a, b) => (byte) (((a * 1) / 3) + ((b * 2) / 3)));
                    }
                    else
                    {
                        colors[2] = OutputPixel.OperationPerComponent(colors[0], colors[1],
                            (a, b) => (byte) (((a * 1) / 2) + ((b * 1) / 2)));
                        colors[3] = OutputPixel.FromRgba(0, 0, 0, 0);
                    }

                    var no = 0;
                    for (var y2 = 0; y2 < 4; y2++)
                    {
                        for (var x2 = 0; x2 < 4; x2++, no++)
                        {
                            var color = ((block.ColorLookup >> (2 * no)) & 0x3);

                            var rx = (x + x2);
                            var ry = (y + y2);
                            var n = ry * _width + rx;

                            _output[n] = colors[color];
                        }
                    }
                }
            }
        }

        private void Decode_PALETTE_T32() => throw new NotImplementedException("Decode_PALETTE_T32");

        private void Decode_PALETTE_T16() => throw new NotImplementedException("Decode_PALETTE_T16");

        private void Decode_PALETTE_T8()
        {
            var input = (byte*) _input;

            var paletteSize = 256;
            var palettePixels = new OutputPixel[paletteSize];
            var translate = new int[paletteSize];

            if (_palette == null || _paletteType == GuPixelFormats.None)
            {
                var n = 0;
                for (var y = 0; y < _height; y++)
                {
                    var inputRow = &_inputByte[y * _strideWidth];
                    for (var x = 0; x < _width; x++, n++)
                    {
                        // @TODO: This is probably wrong!
                        _output[n] = palettePixels[inputRow[x]];
                    }
                }
            }
            else
            {
                fixed (OutputPixel* palettePixelsPtr = palettePixels)
                {
                    Decode(_paletteType, _palette, palettePixelsPtr, palettePixels.Length, 1);
                    //Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
                }
                for (int n = 0; n < paletteSize; n++)
                {
                    translate[n] = ((_paletteStart + n) >> _paletteShift) & _paletteMask;
                }

                for (int y = 0, n = 0; y < _height; y++)
                {
                    var inputRow = &_inputByte[y * _strideWidth];
                    for (var x = 0; x < _width; x++, n++)
                    {
                        var value = inputRow[x];
                        _output[n] = palettePixels[translate[(value >> 0) & 0xFF]];
                    }
                }
            }
        }

        private void Decode_PALETTE_T4()
        {
            if (_palette == null || _paletteType == GuPixelFormats.None)
            {
                Console.WriteLine("Palette required!");
                return;
            }

            const int paletteSize = 256;
            var palettePixels = new OutputPixel[paletteSize];
            var translate = new int[paletteSize];
            fixed (OutputPixel* palettePixelsPtr = palettePixels)
            {
                Decode(_paletteType, _palette, palettePixelsPtr, palettePixels.Length, 1);
                //Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
            }
            //Console.WriteLine(PalettePixels.Length);
            for (var n = 0; n < 16; n++)
            {
                translate[n] = ((_paletteStart + n) >> _paletteShift) & _paletteMask;
                //Console.WriteLine(PalettePixels[Translate[n]]);
            }

            for (int y = 0, n = 0; y < _height; y++)
            {
                var inputRow = &_inputByte[y * _strideWidth];
                for (var x = 0; x < _width / 2; x++, n++)
                {
                    var value = inputRow[x];
                    _output[n * 2 + 0] = palettePixels[translate[(value >> 0) & 0xF]];
                    _output[n * 2 + 1] = palettePixels[translate[(value >> 4) & 0xF]];
                }
            }
        }

        private void _Decode_RGBA_XXXX_uint(Func<uint, OutputPixel> decodePixel)
        {
            var input = (uint*) _input;

            for (int y = 0, n = 0; y < _height; y++)
            {
                var inputRow = (uint*) &_inputByte[y * _strideWidth];
                for (var x = 0; x < _width; x++, n++)
                {
                    _output[n] = decodePixel(inputRow[x]);
                }
            }
        }

        private void _Decode_RGBA_XXXX_ushort(Func<ushort, OutputPixel> decodePixel)
        {
            //var input = (uint*) _input;

            for (int y = 0, n = 0; y < _height; y++)
            {
                var inputRow = (ushort*) &_inputByte[y * _strideWidth];
                for (var x = 0; x < _width; x++, n++)
                {
                    _output[n] = decodePixel(inputRow[x]);
                }
            }
        }

        private void Decode_RGBA_8888() => _Decode_RGBA_XXXX_uint(Decode_RGBA_8888_Pixel_delegate);
        private void Decode_RGBA_4444() => _Decode_RGBA_XXXX_ushort(Decode_RGBA_4444_Pixel_delegate);
        private void Decode_RGBA_5551() => _Decode_RGBA_XXXX_ushort(Decode_RGBA_5551_Pixel_delegate);
        private void Decode_RGBA_5650() => _Decode_RGBA_XXXX_ushort(Decode_RGBA_5650_Pixel_delegate);

        private static Func<uint, OutputPixel> Decode_RGBA_8888_Pixel_delegate = Decode_RGBA_8888_Pixel;
        private static Func<ushort, OutputPixel> Decode_RGBA_4444_Pixel_delegate = Decode_RGBA_4444_Pixel;
        private static Func<ushort, OutputPixel> Decode_RGBA_5551_Pixel_delegate = Decode_RGBA_5551_Pixel;
        private static Func<ushort, OutputPixel> Decode_RGBA_5650_Pixel_delegate = Decode_RGBA_5650_Pixel;

        private static ushort Encode_RGBA_4444_Pixel(OutputPixel pixel)
        {
            uint Out = 0;
            BitUtils.InsertScaled(ref Out, 0, 4, pixel.R, 255);
            BitUtils.InsertScaled(ref Out, 4, 4, pixel.G, 255);
            BitUtils.InsertScaled(ref Out, 8, 4, pixel.B, 255);
            BitUtils.InsertScaled(ref Out, 12, 4, pixel.A, 255);
            return (ushort) Out;
        }

        private static ushort Encode_RGBA_5551_Pixel(OutputPixel pixel)
        {
            uint Out = 0;
            BitUtils.InsertScaled(ref Out, 0, 5, pixel.R, 255);
            BitUtils.InsertScaled(ref Out, 5, 5, pixel.G, 255);
            BitUtils.InsertScaled(ref Out, 10, 5, pixel.B, 255);
            BitUtils.InsertScaled(ref Out, 15, 1, pixel.A, 255);
            return (ushort) Out;
        }

        private static ushort Encode_RGBA_5650_Pixel(OutputPixel pixel)
        {
            uint Out = 0;
            BitUtils.InsertScaled(ref Out, 0, 5, pixel.R, 255);
            BitUtils.InsertScaled(ref Out, 5, 6, pixel.G, 255);
            BitUtils.InsertScaled(ref Out, 11, 5, pixel.B, 255);
            return (ushort) Out;
        }

        private static uint Encode_RGBA_8888_Pixel(OutputPixel pixel) => *(uint*) &pixel;

        public static OutputPixel Decode_RGBA_4444_Pixel(ushort value) =>
            new OutputPixel(
                (byte) value.ExtractScaled(0, 4, 255),
                (byte) value.ExtractScaled(4, 4, 255),
                (byte) value.ExtractScaled(8, 4, 255),
                (byte) value.ExtractScaled(12, 4, 255)
            );

        public static OutputPixel Decode_RGBA_5551_Pixel(ushort value) =>
            new OutputPixel(
                (byte) value.ExtractScaled(0, 5, 255),
                (byte) value.ExtractScaled(5, 5, 255),
                (byte) value.ExtractScaled(10, 5, 255),
                (byte) value.ExtractScaled(15, 1, 255)
            );

        public static OutputPixel Decode_RGBA_5650_Pixel(ushort value) =>
            new OutputPixel(
                (byte) value.ExtractScaled(0, 5, 255),
                (byte) value.ExtractScaled(5, 6, 255),
                (byte) value.ExtractScaled(11, 5, 255),
                0xFF
            );

        public static OutputPixel Decode_RGBA_8888_Pixel(uint value) => *(OutputPixel*) &value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="rowWidth">Width of the texture. In bytes? In pixels? Maybe bytes?</param>
        /// <param name="textureHeight">Height of the texture</param>
        public static void Unswizzle(byte[] input, byte[] output, int rowWidth, int textureHeight)
        {
            fixed (void* inputPtr = input)
            fixed (void* outputPtr = output)
            {
                Unswizzle(inputPtr, outputPtr, rowWidth, textureHeight);
            }
        }

        public static void Unswizzle(void* input, void* output, int rowWidth, int textureHeight)
        {
            var pitch = (rowWidth - 16) / 4;
            var bxc = rowWidth / 16;
            var byc = textureHeight / 8;

            var src = (uint*) input;
            var ydest = (byte*) output;
            for (var by = 0; by < byc; by++)
            {
                var xdest = ydest;
                for (var bx = 0; bx < bxc; bx++)
                {
                    var dest = (uint*) xdest;
                    for (var n = 0; n < 8; n++, dest += pitch)
                    {
                        *(dest++) = *(src++);
                        *(dest++) = *(src++);
                        *(dest++) = *(src++);
                        *(dest++) = *(src++);
                    }
                    xdest += 16;
                }
                ydest += rowWidth * 8;
            }
        }

        private static void UnswizzleInline(void* data, int rowWidth, int textureHeight)
        {
            var temp = new byte[rowWidth * textureHeight];
            fixed (void* tempPointer = temp)
            {
                Unswizzle(data, tempPointer, rowWidth, textureHeight);
            }
            PointerUtils.Memcpy((byte*) data, temp, rowWidth * textureHeight);
        }

        public static void UnswizzleInline(GuPixelFormats format, void* data, int width, int height)
        {
            UnswizzleInline(data, GetPixelsSize(format, width), height);
        }

        public static ulong Hash(GuPixelFormats pixelFormat, void* input, int width, int height)
        {
            var totalBytes = GetPixelsSize(pixelFormat, width * height);

            return Hashing.FastHash((byte*) input, totalBytes, (ulong) ((int) pixelFormat * width * height));
        }

        public static void Encode(GuPixelFormats guPixelFormat, OutputPixel* input, byte* output, int count)
        {
            switch (guPixelFormat)
            {
                case GuPixelFormats.Rgba8888:
                {
                    var o = (uint*) output;
                    for (var n = 0; n < count; n++) *o++ = Encode_RGBA_8888_Pixel(*input++);
                }
                    break;
                case GuPixelFormats.Rgba5551:
                {
                    var o = (ushort*) output;
                    for (var n = 0; n < count; n++) *o++ = Encode_RGBA_5551_Pixel(*input++);
                }
                    break;
                case GuPixelFormats.Rgba5650:
                {
                    var o = (ushort*) output;
                    for (var n = 0; n < count; n++) *o++ = Encode_RGBA_5650_Pixel(*input++);
                }
                    break;
                case GuPixelFormats.Rgba4444:
                {
                    var o = (ushort*) output;
                    for (var n = 0; n < count; n++) *o++ = Encode_RGBA_4444_Pixel(*input++);
                }
                    break;
                default:
                    throw new NotImplementedException("Not implemented " + guPixelFormat);
            }
        }

        public static void Encode(GuPixelFormats guPixelFormat, OutputPixel* input, byte* output, int bufferWidth,
            int width, int height)
        {
            var incOut = GetPixelsSize(guPixelFormat, bufferWidth);

            for (var y = 0; y < height; y++)
            {
                Encode(guPixelFormat, input, output, width);
                input += width;
                output += incOut;
            }
        }
    }
}