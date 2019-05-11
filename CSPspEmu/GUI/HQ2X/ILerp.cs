// (C) Copyright 2011 Ivan Neeson
// Use, modification and distribution are subject to the 
// Boost Software License, Version 1.0. (See accompanying file 
// LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

namespace HQ2x
{
    public interface ILerp
    {
        BGRA Lerp(BGRA colour1, int factor1, BGRA colour2, int factor2, BGRA colour3, int factor3);
    }
}