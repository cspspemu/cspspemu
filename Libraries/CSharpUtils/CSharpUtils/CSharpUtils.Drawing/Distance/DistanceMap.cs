using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using CSharpUtils.Drawing.Extensions;

namespace CSharpUtils.Drawing.Distance
{
    /// <summary>
    /// 
    /// </summary>
    public unsafe class DistanceMap
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="alphaThresold"></param>
        /// <returns></returns>
        public static bool[,] GetMask(Bitmap bitmap, byte alphaThresold = 1)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var mask = new bool[bitmap.Width, bitmap.Height];
            bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, bitmapData =>
            {
                for (var y = 0; y < height; y++)
                {
                    var ptr = ((byte*) bitmapData.Scan0.ToPointer()) + bitmapData.Stride * y;
                    ptr += 3;
                    for (var x = 0; x < width; x++)
                    {
                        mask[x, y] = (*ptr >= alphaThresold);
                        ptr += 4;
                        //Console.Write(Mask[x, y] ? 1 : 0);
                    }
                    //Console.WriteLine("");
                }
            });
            return mask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static IEnumerable<int> Range(int @from, int to, int step = 1)
        {
            if (@from < to)
            {
                for (var n = @from; n <= to; n += step) yield return n;
            }
            else
            {
                for (var n = @from; n >= to; n -= step) yield return n;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static DistanceEntry[,] GetDistanceMap(bool[,] mask)
        {
            var width = mask.GetLength(0);
            var height = mask.GetLength(1);
            var distanceMap = new DistanceEntry[width, height];
            var row = new DistanceEntry[width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    distanceMap[x, y] = new DistanceEntry(1000, 1000);
                }
            }

            var pass1 = Range(0, height - 1);
            var pass2 = Range(height - 1, 0);

            foreach (var passIterator in new[] {pass1, pass2})
                //foreach (var PassIterator in new[] { Pass1 })
            {
                for (var n = 0; n < row.Length; n++) row[n] = new DistanceEntry(1000, 1000);

                //for (int y = 0; y < Height; y++)
                foreach (var y in passIterator)
                {
                    for (var x = 0; x < width; x++)
                    {
                        if (!mask[x, y]) continue;
                        
                        // Inner
                        row[x] = new DistanceEntry(0, 0);

                        // Left edge
                        if (x > 0 && !mask[x - 1, y])
                        {
                            int distanceX = 0, DistanceY = 0;
                            for (int x2 = x - 1; x2 >= 0; x2--)
                            {
                                if (mask[x2, y]) break;
                                distanceX--;
                                row[x2].SetDistanceIfLower(new DistanceEntry(distanceX, DistanceY));
                            }
                        }
                        // Right edge
                        if (x < width - 1 && !mask[x + 1, y])
                        {
                            int distanceX = 0, DistanceY = 0;
                            for (int x2 = x + 1; x2 < width; x2++)
                            {
                                if (mask[x2, y]) break;
                                distanceX++;
                                row[x2].SetDistanceIfLower(new DistanceEntry(distanceX, DistanceY));
                            }
                        }
                    }
                    for (var x = 0; x < width; x++)
                    {
                        distanceMap[x, y].SetDistanceIfLower(row[x]);
                        row[x] = distanceMap[x, y];
                        //Console.Write("{0}", Row[x].GetChar());
                    }
#if true
                    for (var x = width - 2; x >= 0; x--)
                    {
                        var right = distanceMap[x + 1, y];
                        distanceMap[x, y].SetDistanceIfLower(new DistanceEntry(right.DistanceX - 1, right.DistanceY));
                        row[x] = distanceMap[x, y];
                    }
                    for (var x = 1; x < width; x++)
                    {
                        var right = distanceMap[x - 1, y];
                        distanceMap[x, y].SetDistanceIfLower(new DistanceEntry(right.DistanceX + 1, right.DistanceY));
                        row[x] = distanceMap[x, y];
                    }
#endif
                    //Console.WriteLine("");

                    for (int x = 0; x < width; x++)
                    {
                        row[x].DistanceY++;
                        //Console.Write("{0}", Row[x].GetChar());
                    }
                }
            }

            return distanceMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="distanceMap"></param>
        /// <param name="glowDistance"></param>
        /// <param name="glowColor"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void DrawGlow(Bitmap bitmap, DistanceEntry[,] distanceMap, float glowDistance,
            ArgbRev glowColor, float min, float max = 1.0f)
        {
            DrawGlow(bitmap, distanceMap, glowDistance, glowColor,
                f => MathUtils.SmoothStep(min, max, f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="distanceMap"></param>
        /// <param name="glowDistance"></param>
        /// <param name="glowColor"></param>
        /// <param name="function"></param>
        public static void DrawGlow(Bitmap bitmap, DistanceEntry[,] distanceMap, float glowDistance,
            ArgbRev glowColor, Func<float, float> function = null)
        {
            var transparentColor = (ArgbRev) "#00000000";

            if (function == null) function = v => v;

            bitmap.Shader((color, x, y) =>
            {
                var dist = (float) distanceMap[x, y].Distance;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (dist == 0f && color.A == 0xFF) return color;
                if (dist > glowDistance) return new ArgbRev(0, 0, 0, 0);
                var genColor = ArgbRev.Interpolate(glowColor, transparentColor, 1 - function(dist / glowDistance));
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                return (dist == 0) ? ArgbRev.Mix(color, genColor) : genColor;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distanceMap"></param>
        /// <returns></returns>
        public static Bitmap BitmapFromDistanceMap(DistanceEntry[,] distanceMap)
        {
            var bitmap = new Bitmap(distanceMap.GetLength(0), distanceMap.GetLength(1));
            var width = bitmap.Width;
            var height = bitmap.Height;
            bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, bitmapData =>
            {
                for (var y = 0; y < height; y++)
                {
                    var ptr = ((byte*) bitmapData.Scan0.ToPointer()) + bitmapData.Stride * y;
                    for (var x = 0; x < width; x++)
                    {
                        var distance = (byte) MathUtils.FastClamp((int) (distanceMap[x, y].Distance * 4), 0, 255);
                        *ptr++ = distance;
                        *ptr++ = distance;
                        *ptr++ = distance;
                        *ptr++ = 0xFF;
                    }
                }
            });
            return bitmap;
        }

        /*
        [STAThread]
        static public void Main(string[] Args)
        {
            //var Bitmap = new Bitmap(128, 128);
            //for (int n = 0; n < 100; n++) Bitmap.SetPixel(10 + n, 64, Color.White);

            var Bitmap = new Bitmap(Image.FromFile("Mask.png"));
            var Mask = GetMask(Bitmap);

            var Start = DateTime.UtcNow;
            var DistanceMap = GetDistanceMap(Mask);
            var End = DateTime.UtcNow;
            Console.WriteLine(End - Start);
            BitmapFromDistanceMap(DistanceMap).Save("test.png");
            //new Bitmap
            Console.ReadKey();
        }
        */
    }
}