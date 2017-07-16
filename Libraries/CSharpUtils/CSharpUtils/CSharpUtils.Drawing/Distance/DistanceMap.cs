using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpUtils.Drawing.Distance
{
    /// <summary>
    /// 
    /// </summary>
    unsafe public class DistanceMap
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Bitmap"></param>
        /// <param name="AlphaThresold"></param>
        /// <returns></returns>
        static public bool[,] GetMask(Bitmap Bitmap, byte AlphaThresold = 1)
        {
            var Width = Bitmap.Width;
            var Height = Bitmap.Height;
            var Mask = new bool[Bitmap.Width, Bitmap.Height];
            Bitmap.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
            {
                for (int y = 0; y < Height; y++)
                {
                    var Ptr = ((byte*) BitmapData.Scan0.ToPointer()) + BitmapData.Stride * y;
                    Ptr += 3;
                    for (int x = 0; x < Width; x++)
                    {
                        Mask[x, y] = (*Ptr >= AlphaThresold);
                        Ptr += 4;
                        //Console.Write(Mask[x, y] ? 1 : 0);
                    }
                    //Console.WriteLine("");
                }
            });
            return Mask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="Step"></param>
        /// <returns></returns>
        static public IEnumerable<int> Range(int From, int To, int Step = 1)
        {
            if (From < To)
            {
                for (int n = From; n <= To; n += Step)
                {
                    yield return n;
                }
            }
            else
            {
                for (int n = From; n >= To; n -= Step)
                {
                    yield return n;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mask"></param>
        /// <returns></returns>
        static public DistanceEntry[,] GetDistanceMap(bool[,] Mask)
        {
            int Width = Mask.GetLength(0);
            int Height = Mask.GetLength(1);
            var DistanceMap = new DistanceEntry[Width, Height];
            var Row = new DistanceEntry[Width];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    DistanceMap[x, y] = new DistanceEntry(1000, 1000);
                }
            }

            var Pass1 = Range(0, Height - 1);
            var Pass2 = Range(Height - 1, 0);

            foreach (var PassIterator in new[] {Pass1, Pass2})
                //foreach (var PassIterator in new[] { Pass1 })
            {
                for (int n = 0; n < Row.Length; n++) Row[n] = new DistanceEntry(1000, 1000);

                //for (int y = 0; y < Height; y++)
                foreach (var y in PassIterator)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (Mask[x, y])
                        {
                            // Inner
                            Row[x] = new DistanceEntry(0, 0);

                            // Left edge
                            if (x > 0 && !Mask[x - 1, y])
                            {
                                int DistanceX = 0, DistanceY = 0;
                                for (int x2 = x - 1; x2 >= 0; x2--)
                                {
                                    if (Mask[x2, y]) break;
                                    DistanceX--;
                                    Row[x2].SetDistanceIfLower(new DistanceEntry(DistanceX, DistanceY));
                                }
                            }
                            // Right edge
                            if (x < Width - 1 && !Mask[x + 1, y])
                            {
                                int DistanceX = 0, DistanceY = 0;
                                for (int x2 = x + 1; x2 < Width; x2++)
                                {
                                    if (Mask[x2, y]) break;
                                    DistanceX++;
                                    Row[x2].SetDistanceIfLower(new DistanceEntry(DistanceX, DistanceY));
                                }
                            }
                        }
                        else
                        {
                            //Row[x].SetDistanceIfLower(new DistanceEntry(1000, 1000));
                        }
                    }
                    for (int x = 0; x < Width; x++)
                    {
                        DistanceMap[x, y].SetDistanceIfLower(Row[x]);
                        Row[x] = DistanceMap[x, y];
                        //Console.Write("{0}", Row[x].GetChar());
                    }
#if true
                    for (int x = Width - 2; x >= 0; x--)
                    {
                        var Right = DistanceMap[x + 1, y];
                        DistanceMap[x, y].SetDistanceIfLower(new DistanceEntry(Right.DistanceX - 1, Right.DistanceY));
                        Row[x] = DistanceMap[x, y];
                    }
                    for (int x = 1; x < Width; x++)
                    {
                        var Right = DistanceMap[x - 1, y];
                        DistanceMap[x, y].SetDistanceIfLower(new DistanceEntry(Right.DistanceX + 1, Right.DistanceY));
                        Row[x] = DistanceMap[x, y];
                    }
#endif
                    //Console.WriteLine("");

                    for (int x = 0; x < Width; x++)
                    {
                        Row[x].DistanceY++;
                        //Console.Write("{0}", Row[x].GetChar());
                    }
                }
            }

            return DistanceMap;
        }

        static public void DrawGlow(Bitmap Bitmap, DistanceEntry[,] _DistanceMap, float GlowDistance,
            ARGB_Rev GlowColor, float Min, float Max = 1.0f)
        {
            DistanceMap.DrawGlow(Bitmap, _DistanceMap, GlowDistance, GlowColor,
                (f) => { return MathUtils.SmoothStep(Min, Max, f); });
        }

        static public void DrawGlow(Bitmap Bitmap, DistanceEntry[,] _DistanceMap, float GlowDistance,
            ARGB_Rev GlowColor, Func<float, float> Function = null)
        {
            var TransparentColor = (ARGB_Rev) "#00000000";

            if (Function == null) Function = (v) => v;

            Bitmap.Shader((color, x, y) =>
            {
                var Dist = (float) _DistanceMap[x, y].Distance;
                if (Dist == 0 && color.A == 0xFF) return color;
                if (Dist > GlowDistance) return new ARGB_Rev(0, 0, 0, 0);
                var GenColor = ARGB_Rev.Interpolate(GlowColor, TransparentColor, 1 - Function(Dist / GlowDistance));
                return (Dist == 0) ? ARGB_Rev.Mix(color, GenColor) : GenColor;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DistanceMap"></param>
        /// <returns></returns>
        static public Bitmap BitmapFromDistanceMap(DistanceEntry[,] DistanceMap)
        {
            var Bitmap = new Bitmap(DistanceMap.GetLength(0), DistanceMap.GetLength(1));
            var Width = Bitmap.Width;
            var Height = Bitmap.Height;
            Bitmap.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
            {
                for (int y = 0; y < Height; y++)
                {
                    var Ptr = ((byte*) BitmapData.Scan0.ToPointer()) + BitmapData.Stride * y;
                    for (int x = 0; x < Width; x++)
                    {
                        byte Distance = (byte) MathUtils.FastClamp((int) (DistanceMap[x, y].Distance * 4), 0, 255);
                        *Ptr++ = Distance;
                        *Ptr++ = Distance;
                        *Ptr++ = Distance;
                        *Ptr++ = 0xFF;
                    }
                }
            });
            return Bitmap;
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