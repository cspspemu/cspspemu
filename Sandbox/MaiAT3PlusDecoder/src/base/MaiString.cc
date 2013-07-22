#include "SUB/MaiString.h"

Mai_I32 Astrlen(Mai_I8* s)
{
	Mai_I32 count = 0;
	while (s[count]) count++;
	return count;
}

Mai_Status Astrcpy(Mai_I8* dst, Mai_I8* src)
{
	Mai_I32 a0 = 0;
	while (src[a0]) {dst[a0] = src[a0]; a0++;}
	dst[a0] = 0;
	return 0;
}

Mai_Status Astrcmp(Mai_I8* p1, Mai_I8* p2)
{
	Mai_I8 c1, c2;
	do
	{
		c1 = *p1++;
		c2 = *p2++;
	}
	while ((c1 == c2) && (c1));
	return c1 - c2;
}

Mai_I32 Ustrlen(Mai_WChar* s)
{
	Mai_I32 count = 0;
	while (s[count]) count++;
	return count;
}

Mai_Status Ustrcpy(Mai_WChar* dst, Mai_WChar* src)
{
	Mai_I32 a0 = 0;
	while (src[a0]) {dst[a0] = src[a0]; a0++;}
	dst[a0] = 0;
	return 0;
}

Mai_Status Ustrcmp(Mai_WChar* p1, Mai_WChar* p2)
{
	Mai_WChar c1, c2;
	do
	{
		c1 = *p1++;
		c2 = *p2++;
	}
	while ((c1 == c2) && (c1));
	return c1 - c2;
}

Mai_Status DecToUStr(Mai_WChar* str, Mai_I32 off, Mai_I64 a)
{
	Mai_I32 s[64];
	Mai_I32 a0 = 0;
	do
	{
		s[a0++] = (Mai_I32)(a % 10);
		a = a / 10;
	} while (a);
	Mai_I32 a1;
	for (a1 = a0 - 1; a1 >= 0; a1--)
	{
		str[off++] = (WCHAR)(L'0' + s[a1]);
	}
	str[off++] = L'\0';
	return 0;
}

Mai_Status UStrToDec(Mai_I64 *a, Mai_WChar* str)
{
	Mai_I32 len = Ustrlen(str);
	Mai_I64 dst = 0;
	for (Mai_I32 a0 = 0; a0 < len; a0++)
	{
		if ( (str[a0] >= 0x30) && (str[a0] < 0x3A) )
		{
			dst = dst * 10 + str[a0] - 0x30;
		}
		else
		{
			*a = dst;
			return -1;
		}
	}

	*a = dst;
	return 0;
}

Mai_Status AStrToDec(Mai_I64 *a, Mai_I8* str)
{
	Mai_I32 len = Mai_strlen(str);
	Mai_I64 dst = 0;
	for (Mai_I32 a0 = 0; a0 < len; a0++)
	{
		if ( (str[a0] >= 0x30) && (str[a0] < 0x3A) )
		{
			dst = dst * 10 + str[a0] - 0x30;
		}
		else
		{
			*a = dst;
			return -1;
		}
	}

	*a = dst;
	return 0;
}

Mai_Status AStrToHex(Mai_I64 *a, Mai_I8* str)
{
	Mai_I32 len = Mai_strlen(str);
	Mai_I64 dst = 0;
	for (Mai_I32 a0 = 0; a0 < len; a0++)
	{
		if ( (str[a0] >= 0x30) && (str[a0] < 0x3A) )
		{
			dst = (dst << 4) + str[a0] - 0x30;
		}
		else if ( (str[a0] >= 0x41) && (str[a0] < 0x47) )
		{
			dst = (dst << 4) + str[a0] - 0x41 + 0xA;
		}
		else if ( (str[a0] >= 0x61) && (str[a0] < 0x67) )
		{
			dst = (dst << 4) + str[a0] - 0x61 + 0xA;
		}
		else
		{
			*a = dst;
			return -1;
		}
	}

	*a = dst;
	return 0;
}

Mai_Status ALowToCap(Mai_I8* src)
{
	Mai_I32 len = Mai_strlen(src);
	for (Mai_I32 a0 = 0; a0 < len; a0++)
	{
		if ((src[a0] >= 0x61) && (src[a0] <= 0x7A)) src[a0] -= 0x20;
	}
	return 0;
}

Mai_Status ULowToCap(Mai_WChar* src)
{
	Mai_I32 len = Mai_strlen(src);
	for (Mai_I32 a0 = 0; a0 < len; a0++)
	{
		if ((src[a0] >= 0x61) && (src[a0] <= 0x7A)) src[a0] -= 0x20;
	}
	return 0;
}

Mai_Status AremoveLRLetter(Mai_I8* src, Mai_I8 letter)
{
	Mai_I32 left = 0;
	Mai_I32 right = Mai_strlen(src) - 1;
	for (; left <= right; left++) if (src[left] != letter) break;
	for (; right >= left; right--) if (src[right] != letter) break;
	if (left <= right)
	{
		Mai_I32 a0;
		for (a0 = 0; a0 < right - left + 1; a0++)
		{
			src[a0] = src[left + a0];
		}
		src[a0] = 0;
	}
	else src[0] = 0;
	return 0;
}

Mai_Status UremoveLRLetter(Mai_WChar* src, Mai_WChar letter)
{
	Mai_I32 left = 0;
	Mai_I32 right = Mai_strlen(src) - 1;
	for (; left <= right; left++) if (src[left] != letter) break;
	for (; right >= left; right--) if (src[right] != letter) break;
	if (left <= right)
	{
		Mai_I32 a0;
		for (a0 = 0; a0 < right - left + 1; a0++)
		{
			src[a0] = src[left + a0];
		}
		src[a0] = 0;
	}
	else src[0] = 0;
	return 0;
}

Mai_Status AsplitString(Mai_I8* dst, Mai_I8* remain, Mai_I8* src, Mai_I8 splitter)
{
	Mai_I32 split_n = 0;
	Mai_I32 src_len = Mai_strlen(src);
	for (; split_n < src_len; split_n++) if (src[split_n] == splitter) break;

	Mai_I32 a0 = 0;
	for (; a0 < split_n; a0++) dst[a0] = src[a0];
	dst[a0] = 0;
	for (a0 = 0; a0 < src_len - split_n - 1; a0++) remain[a0] = src[split_n + 1 + a0];
	remain[a0] = 0;
	return 0;
}

Mai_Status UsplitString(Mai_WChar* dst, Mai_WChar* remain, Mai_WChar* src, Mai_WChar splitter)
{
	Mai_I32 split_n = 0;
	Mai_I32 src_len = Mai_strlen(src);
	for (; split_n < src_len; split_n++) if (src[split_n] == splitter) break;

	Mai_I32 a0 = 0;
	for (; a0 < split_n; a0++) dst[a0] = src[a0];
	dst[a0] = 0;
	for (a0 = 0; a0 < src_len - split_n - 1; a0++) remain[a0] = src[split_n + 1 + a0];
	remain[a0] = 0;
	return 0;
}








Mai_I32 Mai_strlen(Mai_WChar* s) {return Ustrlen(s);}

Mai_I32 Mai_strlen(Mai_I8* s) {return Astrlen(s);}

Mai_Status Mai_strcpy(Mai_WChar* dst, Mai_WChar* src) {return Ustrcpy(dst, src);}

Mai_Status Mai_strcpy(Mai_I8* dst, Mai_I8* src) {return Astrcpy(dst, src);}

Mai_Status Mai_strcat(Mai_WChar* dst, Mai_WChar* src) {return Mai_strcpy(dst + Mai_strlen(dst), src);}

Mai_Status Mai_strcat(Mai_I8* dst, Mai_I8* src) {return Mai_strcpy(dst + Mai_strlen(dst), src);}

Mai_Status Mai_strcmp(Mai_WChar* p1, Mai_WChar* p2) {return Ustrcmp(p1, p2);}

Mai_Status Mai_strcmp(Mai_I8* p1, Mai_I8* p2) {return Astrcmp(p1, p2);}

Mai_Status LowToCap(Mai_I8* src) {return ALowToCap(src);}

Mai_Status LowToCap(Mai_WChar* src) {return ULowToCap(src);}

Mai_Status removeLRLetter(Mai_I8* src, Mai_I8 letter) {return AremoveLRLetter(src, letter);}

Mai_Status removeLRLetter(Mai_WChar* src, Mai_WChar letter) {return UremoveLRLetter(src, letter);}

Mai_Status splitString(Mai_I8* dst, Mai_I8* remain, Mai_I8* src, Mai_I8 splitter) {return AsplitString(dst, remain, src, splitter);}

Mai_Status splitString(Mai_WChar* dst, Mai_WChar* remain, Mai_WChar* src, Mai_WChar splitter) {return UsplitString(dst, remain, src, splitter);}


Mai_Status MaiStrCloner(Heap_Alloc0 *heap0, Mai_WChar **pdst, Mai_WChar *src)
{
	if (*pdst) heap0->free(*pdst);
	*pdst = (Mai_WChar *)heap0->alloc(sizeof(Mai_WChar) * (Mai_strlen(src) + 1));
	Mai_strcpy(*pdst, src);
	return 0;
}

Mai_Status Mai_strcmpAllC(Mai_I8* p1, Mai_I8* p2)
{
	Mai_I8 tmp0[0x400];
	Mai_I8 tmp1[0x400];
	Mai_strcpy(tmp0, p1);
	Mai_strcpy(tmp1, p2);
	LowToCap(tmp0);
	LowToCap(tmp1);

	return Mai_strcmp(tmp0, tmp1);
}

Mai_Status Mai_strcmpAllC(Mai_WChar* p1, Mai_WChar* p2)
{
	Mai_WChar tmp0[0x400];
	Mai_WChar tmp1[0x400];
	Mai_strcpy(tmp0, p1);
	Mai_strcpy(tmp1, p2);
	LowToCap(tmp0);
	LowToCap(tmp1);

	return Mai_strcmp(tmp0, tmp1);
}

