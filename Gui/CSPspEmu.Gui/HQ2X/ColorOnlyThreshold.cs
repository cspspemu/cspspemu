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
    public class ColorOnlyThreshold : IThreshold
    {
        private static int[,] s_Matrix = new int[3, 3]
        {
            {19595, 38470, 7471},
            {-9642, -18931, 28574},
            {40304, -33750, -6554},
        };

        private int[] m_Thresholds = new int[3];

        public ColorOnlyThreshold(byte yThreshold, byte uThreshold, byte vThreshold)
        {
            m_Thresholds[0] = yThreshold * 65536;
            m_Thresholds[1] = uThreshold * 65536;
            m_Thresholds[2] = vThreshold * 65536;
        }

        public bool Similar(BGRA color1, BGRA color2)
        {
            // Quick exit if same:
            if (color1 == color2)
                return true;

            // Compare YUV thresholds:
            for (int i = 0; i < 3; ++i)
            {
                int comp1 =
                    s_Matrix[i, 0] * color1.R +
                    s_Matrix[i, 1] * color1.G +
                    s_Matrix[i, 2] * color1.B;
                int comp2 =
                    s_Matrix[i, 0] * color2.R +
                    s_Matrix[i, 1] * color2.G +
                    s_Matrix[i, 2] * color2.B;

                if (Math.Abs(comp1 - comp2) > m_Thresholds[i])
                    return false;
            }

            return true;
        }
    }
}