#region (c)2008-2013 Hawkynt

/*
 *  cImage 
 *  Image filtering library 
    Copyright (C) 2010-2013 Hawkynt

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#endregion



#if !NET35


#endif

namespace Imager.Filters
{
    public static class libXBR
    {
        /// <summary>
        /// This is the XBR2x by Hyllian (see http://board.byuu.org/viewtopic.php?f=10&t=2248)
        /// </summary>
        public static void Xbr2X(cImage sourceImage, int srcX, int srcY, cImage targetImage, int tgtX, int tgtY,
            bool allowAlphaBlending)
        {
            var pa = sourceImage[srcX - 1, srcY - 1];
            var pb = sourceImage[srcX + 0, srcY - 1];
            var pc = sourceImage[srcX + 1, srcY - 1];

            var pd = sourceImage[srcX - 1, srcY + 0];
            var pe = sourceImage[srcX + 0, srcY + 0];
            var pf = sourceImage[srcX + 1, srcY + 0];

            var pg = sourceImage[srcX - 1, srcY + 1];
            var ph = sourceImage[srcX + 0, srcY + 1];
            var pi = sourceImage[srcX + 1, srcY + 1];

            var a1 = sourceImage[srcX - 1, srcY - 2];
            var b1 = sourceImage[srcX + 0, srcY - 2];
            var c1 = sourceImage[srcX + 1, srcY - 2];

            var a0 = sourceImage[srcX - 2, srcY - 1];
            var d0 = sourceImage[srcX - 2, srcY + 0];
            var g0 = sourceImage[srcX - 2, srcY + 1];

            var c4 = sourceImage[srcX + 2, srcY - 1];
            var f4 = sourceImage[srcX + 2, srcY + 0];
            var i4 = sourceImage[srcX + 2, srcY + 1];

            var g5 = sourceImage[srcX - 1, srcY + 2];
            var h5 = sourceImage[srcX + 0, srcY + 2];
            var i5 = sourceImage[srcX + 1, srcY + 2];

            sPixel e1, e2, e3;
            var e0 = e1 = e2 = e3 = pe;

            _Kernel2Xv5(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref e1, ref e2, ref e3, allowAlphaBlending);
            _Kernel2Xv5(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e0, ref e3, ref e1, allowAlphaBlending);
            _Kernel2Xv5(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e2, ref e1, ref e0, allowAlphaBlending);
            _Kernel2Xv5(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref e3, ref e0, ref e2, allowAlphaBlending);

            targetImage[tgtX + 0, tgtY + 0] = e0;
            targetImage[tgtX + 1, tgtY + 0] = e1;
            targetImage[tgtX + 0, tgtY + 1] = e2;
            targetImage[tgtX + 1, tgtY + 1] = e3;
        }

        private static uint _YuvDifference(sPixel a, sPixel b)
        {
            return a.AbsDifference(b);
        }

        private static bool _IsEqual(sPixel a, sPixel b)
        {
            return a.IsLike(b);
        }

        private static void _AlphaBlend64W(ref sPixel dst, sPixel src, bool blend)
        {
            if (blend)
                dst = sPixel.Interpolate(dst, src, 3, 1);
        }

        private static void _AlphaBlend128W(ref sPixel dst, sPixel src, bool blend)
        {
            if (blend)
                dst = sPixel.Interpolate(dst, src);
        }

        private static void _AlphaBlend192W(ref sPixel dst, sPixel src, bool blend)
        {
            dst = blend ? sPixel.Interpolate(dst, src, 1, 3) : src;
        }

        private static void _AlphaBlend224W(ref sPixel dst, sPixel src, bool blend)
        {
            dst = blend ? sPixel.Interpolate(dst, src, 1, 7) : src;
        }

        private static void _LeftUp2_2X(ref sPixel n3, ref sPixel n2, out sPixel n1, sPixel pixel, bool blend)
        {
            _AlphaBlend224W(ref n3, pixel, blend);
            _AlphaBlend64W(ref n2, pixel, blend);
            n1 = n2;
        }

        private static void _Left2_2X(ref sPixel n3, ref sPixel n2, sPixel pixel, bool blend)
        {
            _AlphaBlend192W(ref n3, pixel, blend);
            _AlphaBlend64W(ref n2, pixel, blend);
        }

        private static void _Up2_2X(ref sPixel n3, ref sPixel n1, sPixel pixel, bool blend)
        {
            _AlphaBlend192W(ref n3, pixel, blend);
            _AlphaBlend64W(ref n1, pixel, blend);
        }

        private static void _Dia_2X(ref sPixel n3, sPixel pixel, bool blend)
        {
            _AlphaBlend128W(ref n3, pixel, blend);
        }

        private static void _Kernel2Xv5(sPixel pe, sPixel pi, sPixel ph, sPixel pf, sPixel pg, sPixel pc, sPixel pd,
            sPixel pb, sPixel f4, sPixel i4, sPixel h5, sPixel i5, ref sPixel n1, ref sPixel n2, ref sPixel n3,
            bool blend)
        {
            var ex = pe != ph && pe != pf;
            if (!ex) return;
            var e =
                _YuvDifference(pe, pc) + _YuvDifference(pe, pg) + _YuvDifference(pi, h5) + _YuvDifference(pi, f4) +
                (_YuvDifference(ph, pf) << 2);
            var i =
                _YuvDifference(ph, pd) + _YuvDifference(ph, i5) + _YuvDifference(pf, i4) + _YuvDifference(pf, pb) +
                (_YuvDifference(pe, pi) << 2);
            var px = _YuvDifference(pe, pf) <= _YuvDifference(pe, ph) ? pf : ph;
            if (e < i && (!_IsEqual(pf, pb) && !_IsEqual(ph, pd) ||
                            _IsEqual(pe, pi) && !_IsEqual(pf, i4) && !_IsEqual(ph, i5) || _IsEqual(pe, pg) ||
                            _IsEqual(pe, pc)))
            {
                var ke = _YuvDifference(pf, pg);
                var ki = _YuvDifference(ph, pc);
                var ex2 = pe != pc && pb != pc;
                var ex3 = pe != pg && pd != pg;
                if (ke << 1 <= ki && ex3 || ke >= ki << 1 && ex2)
                {
                    if (ke << 1 <= ki && ex3) _Left2_2X(ref n3, ref n2, px, blend);
                    if (ke >= ki << 1 && ex2) _Up2_2X(ref n3, ref n1, px, blend);
                }
                else
                {
                    _Dia_2X(ref n3, px, blend);
                }
            }
            else if (e <= i)
            {
                _AlphaBlend64W(ref n3, px, blend);
            }
        }
    }
}