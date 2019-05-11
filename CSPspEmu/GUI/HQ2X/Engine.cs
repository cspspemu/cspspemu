// (C) Copyright 2011 Ivan Neeson
// Use, modification and distribution are subject to the 
// Boost Software License, Version 1.0. (See accompanying file 
// LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Drawing;
using System.Collections.Specialized;
using System.Drawing.Imaging;

namespace HQ2x
{
    public unsafe class Engine
    {
        public ILerp Lerp { get; set; }
        public IThreshold Threshold { get; set; }

        public Engine()
        {
        }

        public Engine(ILerp lerp, IThreshold threshold)
        {
            Lerp = lerp;
            Threshold = threshold;
        }

        public Bitmap Process(Bitmap inputBitmap)
        {
            if (inputBitmap == null)
                throw new ArgumentNullException("inputBitmap");

            if (Lerp == null)
                throw new InvalidOperationException("Lerp is null");

            if (Threshold == null)
                throw new InvalidOperationException("Threshold is null");

            Bitmap outputBitmap = new Bitmap(2 * inputBitmap.Width, 2 * inputBitmap.Height, inputBitmap.PixelFormat);

            var inputBitmapData = inputBitmap.LockBits(new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var inputBitmapDataPixels = (BGRA*) inputBitmapData.Scan0.ToPointer();

            var outputBitmapData = outputBitmap.LockBits(new Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var outputBitmapPixels = (BGRA*) outputBitmapData.Scan0.ToPointer();
            //outputBitmap.LockBitsUnlock(PixelFormat.
            for (int y = 0; y < inputBitmap.Height; y++)
            {
                var Row0 = outputBitmapPixels + (y * 2 + 0) * outputBitmap.Width;
                var Row1 = outputBitmapPixels + (y * 2 + 1) * outputBitmap.Width;
                for (int x = 0; x < inputBitmap.Width; x++)
                {
                    //var square0 = new Square(this, inputBitmapDataPixels + y * inputBitmapData.Width + x, inputBitmapData.Width, inputBitmapData.Height, x, y);
                    var square0 = new Square(this, inputBitmapDataPixels, inputBitmapData.Width, inputBitmapData.Height,
                        x, y);
                    var square90 = square0.RotateCCW();
                    var square180 = square90.RotateCCW();
                    var square270 = square180.RotateCCW();

                    Row0[2 * x + 0] = square0.Resolve();
                    Row0[2 * x + 1] = square90.Resolve();
                    Row1[2 * x + 1] = square180.Resolve();
                    Row1[2 * x + 0] = square270.Resolve();

                    //outputBitmap.SetPixel(2 * x + 0, 2 * y + 0, square0.Resolve().ToColor());
                    //outputBitmap.SetPixel(2 * x + 1, 2 * y + 0, square90.Resolve().ToColor());
                    //outputBitmap.SetPixel(2 * x + 1, 2 * y + 1, square180.Resolve().ToColor());
                    //outputBitmap.SetPixel(2 * x + 0, 2 * y + 1, square270.Resolve().ToColor());
                }
            }
            outputBitmap.UnlockBits(outputBitmapData);
            inputBitmap.UnlockBits(inputBitmapData);

            return outputBitmap;
        }

        private class Square
        {
            private static byte[] s_ShapeToRule = new byte[256]
            {
                14, 1, 3, 5, 12, 1, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                14, 1, 3, 5, 12, 16, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                12, 1, 3, 5, 12, 16, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                13, 16, 3, 5, 12, 16, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                14, 1, 3, 5, 13, 16, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                14, 16, 3, 5, 13, 17, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                12, 16, 3, 5, 12, 16, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                13, 17, 3, 5, 12, 15, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                14, 1, 19, 5, 12, 16, 3, 5, 18, 6, 4, 4, 2, 6, 4, 4,
                14, 1, 19, 5, 12, 1, 19, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                12, 16, 3, 5, 12, 16, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                12, 16, 19, 5, 12, 12, 19, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                14, 1, 3, 5, 12, 16, 3, 5, 18, 6, 4, 4, 18, 6, 4, 4,
                14, 16, 3, 5, 13, 17, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
                12, 1, 3, 5, 12, 12, 3, 5, 18, 6, 4, 4, 18, 6, 4, 4,
                13, 17, 3, 5, 12, 15, 3, 5, 2, 6, 4, 4, 2, 6, 4, 4,
            };

            private Engine m_Engine;
            private BGRA[] m_Colors = new BGRA[9];
            private BitVector32 m_Shape;

            public Square(Engine engine)
            {
                m_Engine = engine;
            }

            private BGRA GetColor(BGRA* Data, int Width, int Height, int x, int y)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return BGRA.FromArgb(0, 0, 0, 0);
                return Data[y * Width + x];
            }

            //public Square(Engine engine, Bitmap bitmap, int x, int y)
            public Square(Engine engine, BGRA* Data, int Width, int Height, int x, int y)
            {
                m_Engine = engine;

                // 0 1 2
                // 3 4 5
                // 6 7 8

                m_Colors[0] = GetColor(Data, Width, Height, x - 1, y - 1);
                m_Colors[1] = GetColor(Data, Width, Height, x, y - 1);
                m_Colors[2] = GetColor(Data, Width, Height, x + 1, y - 1);
                m_Colors[3] = GetColor(Data, Width, Height, x - 1, y);
                m_Colors[4] = GetColor(Data, Width, Height, x, y);
                m_Colors[5] = GetColor(Data, Width, Height, x + 1, y);
                m_Colors[6] = GetColor(Data, Width, Height, x - 1, y + 1);
                m_Colors[7] = GetColor(Data, Width, Height, x, y + 1);
                m_Colors[8] = GetColor(Data, Width, Height, x + 1, y + 1);

                // 0 1 2
                // 3   4
                // 5 6 7

                m_Shape[1 << 0] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[0]);
                m_Shape[1 << 1] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[1]);
                m_Shape[1 << 2] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[2]);
                m_Shape[1 << 3] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[3]);
                m_Shape[1 << 4] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[5]);
                m_Shape[1 << 5] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[6]);
                m_Shape[1 << 6] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[7]);
                m_Shape[1 << 7] = m_Engine.Threshold.Similar(m_Colors[4], m_Colors[8]);
            }

            public Square RotateCCW()
            {
                Square result = new Square(m_Engine);

                // 0 1 2      2 5 8
                // 3 4 5  ->  1 4 7
                // 6 7 8      0 3 6

                result.m_Colors[0] = m_Colors[2];
                result.m_Colors[1] = m_Colors[5];
                result.m_Colors[2] = m_Colors[8];
                result.m_Colors[3] = m_Colors[1];
                result.m_Colors[4] = m_Colors[4];
                result.m_Colors[5] = m_Colors[7];
                result.m_Colors[6] = m_Colors[0];
                result.m_Colors[7] = m_Colors[3];
                result.m_Colors[8] = m_Colors[6];

                // 0 1 2      2 4 7
                // 3   4  ->  1   6
                // 5 6 7      0 3 5

                result.m_Shape[1 << 0] = m_Shape[1 << 2];
                result.m_Shape[1 << 1] = m_Shape[1 << 4];
                result.m_Shape[1 << 2] = m_Shape[1 << 7];
                result.m_Shape[1 << 3] = m_Shape[1 << 1];
                result.m_Shape[1 << 4] = m_Shape[1 << 6];
                result.m_Shape[1 << 5] = m_Shape[1 << 0];
                result.m_Shape[1 << 6] = m_Shape[1 << 3];
                result.m_Shape[1 << 7] = m_Shape[1 << 5];

                return result;
            }

            public BGRA Resolve()
            {
                byte rule = s_ShapeToRule[m_Shape.Data];

                switch (rule)
                {
                    case 1: return Lerp3x1(m_Colors[4], m_Colors[0]);
                    case 2: return Lerp3x1(m_Colors[4], m_Colors[3]);
                    case 3: return Lerp3x1(m_Colors[4], m_Colors[1]);
                    case 4: return Lerp2x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                    case 5: return Lerp2x1x1(m_Colors[4], m_Colors[0], m_Colors[1]);
                    case 6: return Lerp2x1x1(m_Colors[4], m_Colors[0], m_Colors[3]);
                    case 7: return Lerp5x2x1(m_Colors[4], m_Colors[1], m_Colors[3]);
                    case 8: return Lerp5x2x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                    case 9: return Lerp6x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                    case 10: return Lerp2x3x3(m_Colors[4], m_Colors[3], m_Colors[1]);
                    case 11: return Lerp14x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                    case 12:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[3]))
                            return Lerp2x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return m_Colors[4];
                    case 13:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[3]))
                            return Lerp2x3x3(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return m_Colors[4];
                    case 14:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[3]))
                            return Lerp14x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return m_Colors[4];
                    case 15:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[3]))
                            return Lerp2x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return Lerp3x1(m_Colors[4], m_Colors[0]);
                    case 16:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[3]))
                            return Lerp6x1x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return Lerp3x1(m_Colors[4], m_Colors[0]);
                    case 17:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[3]))
                            return Lerp2x3x3(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return Lerp3x1(m_Colors[4], m_Colors[0]);
                    case 18:
                        if (m_Engine.Threshold.Similar(m_Colors[1], m_Colors[5]))
                            return Lerp5x2x1(m_Colors[4], m_Colors[1], m_Colors[3]);
                        else
                            return Lerp3x1(m_Colors[4], m_Colors[3]);
                    case 19:
                        if (m_Engine.Threshold.Similar(m_Colors[3], m_Colors[7]))
                            return Lerp5x2x1(m_Colors[4], m_Colors[3], m_Colors[1]);
                        else
                            return Lerp3x1(m_Colors[4], m_Colors[1]);
                    default:
                        return m_Colors[4];
                }
            }

            public BGRA Lerp3x1(BGRA color1, BGRA color2)
            {
                return m_Engine.Lerp.Lerp(color1, 3, color2, 1, BGRA.FromArgb(0, 0, 0, 0), 0);
            }

            public BGRA Lerp2x1x1(BGRA color1, BGRA color2, BGRA color3)
            {
                return m_Engine.Lerp.Lerp(color1, 2, color2, 1, color3, 1);
            }

            public BGRA Lerp5x2x1(BGRA color1, BGRA color2, BGRA color3)
            {
                return m_Engine.Lerp.Lerp(color1, 5, color2, 2, color3, 1);
            }

            public BGRA Lerp6x1x1(BGRA color1, BGRA color2, BGRA color3)
            {
                return m_Engine.Lerp.Lerp(color1, 6, color2, 1, color3, 1);
            }

            public BGRA Lerp2x3x3(BGRA color1, BGRA color2, BGRA color3)
            {
                return m_Engine.Lerp.Lerp(color1, 2, color2, 3, color3, 3);
            }

            public BGRA Lerp14x1x1(BGRA color1, BGRA color2, BGRA color3)
            {
                return m_Engine.Lerp.Lerp(color1, 14, color2, 1, color3, 1);
            }
        };
    }
}