// (C) Copyright 2011 Ivan Neeson
// Use, modification and distribution are subject to the 
// Boost Software License, Version 1.0. (See accompanying file 
// LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace HQ2x
{
    public class ColorSimpleAlphaLerp : ILerp
    {
		public BGRA Lerp(BGRA color1, int factor1, BGRA color2, int factor2, BGRA color3, int factor3)
        {
			return BGRA.FromArgb(
                LerpChannel(color1.A, factor1, color2.A, factor2, color3.A, factor3),
                LerpChannel(color1.R, factor1, color2.R, factor2, color3.R, factor3),
                LerpChannel(color1.G, factor1, color2.G, factor2, color3.G, factor3),
                LerpChannel(color1.B, factor1, color2.B, factor2, color3.B, factor3)
                );
        }

        private int LerpChannel(byte value1, int factor1, byte value2, int factor2, byte value3, int factor3)
        {
            return (value1 * factor1 + value2 * factor2 + value3 * factor3) / (factor1 + factor2 + factor3);
        }
    }
}
