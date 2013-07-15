#include "SUB/Unicode.h"

Mai_Status UCToUTF8(Mai_I8* dst, Mai_WChar *src)
{
	Mai_I32 s_num = Ustrlen(src);
	Mai_I32 d_n = 0;
	for (Mai_I32 a0 = 0; a0 < s_num; a0++)
	{
		if ( ((Mai_U16)src[a0] < 0x80) )
		{
			dst[d_n++] = (Mai_I8)src[a0];
		}
		else if ( ((Mai_U16)src[a0] < 0x800) )
		{
			dst[d_n++] = (Mai_I8)( 0xC0 | ( (src[a0] >> 6) & 0x1F ) );
			dst[d_n++] = (Mai_I8)( 0x80 | ( src[a0] & 0x3F ) );
		}
		else if (  ((Mai_U16)src[a0] < 0xD800)  ||  ((Mai_U16)src[a0] >= 0xE000)  )
		{
			dst[d_n++] = (Mai_I8)( 0xE0 | ( (src[a0] >> 12) & 0xF ) );
			dst[d_n++] = (Mai_I8)( 0x80 | ( (src[a0] >> 6) & 0x3F ) );
			dst[d_n++] = (Mai_I8)( 0x80 | ( src[a0] & 0x3F ) );
		}
	}
	dst[d_n] = 0;

	return 0;
}

Mai_Status UTF8ToUC(Mai_WChar *dst, Mai_I8* src)
{
	Mai_I32 s_num = Mai_strlen(src);
	Mai_I32 d_n = 0;
	for (Mai_I32 a0 = 0; a0 < s_num;)
	{
		if (!(src[a0] & 0x80))
		{
			dst[d_n++] = (Mai_WChar)(  src[a0] & 0xFF  );
			a0++;
		}
		else if (     (a0 + 1 < s_num)     &&     ( (Mai_U8)(src[a0] & 0xE0) == (Mai_U8)0xC0 )     )
		{
			dst[d_n++] = (Mai_WChar)(  ((src[a0] & 0x1F) << 6) | (src[a0 + 1] & 0x3F)  );
			a0 += 2;
		}
		else if (     (a0 + 2 < s_num)     &&     ( (Mai_U8)(src[a0] & 0xF0) == (Mai_U8)0xE0 )     )
		{
			dst[d_n++] = (Mai_WChar)(  ((src[a0] & 0xF) << 12) | ((src[a0 + 1] & 0x3F) << 6) | ((src[a0 + 2] & 0x3F))  );
			a0 += 3;
		}
		else
		{
			a0++;
		}
	}
	dst[d_n] = 0;

	return 0;
}
