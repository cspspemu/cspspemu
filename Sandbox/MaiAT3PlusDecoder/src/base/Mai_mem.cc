#include "SUB/Mai_mem.h"

Mai_Status Mai_memcpy(Mai_Void* dst, Mai_Void* src, Mai_I32 size)
{
	Mai_I8 *dst_t = (Mai_I8*)dst;
	Mai_I8 *src_t = (Mai_I8*)src;
	while (size--) *(dst_t++) = *(src_t++);
	return 0;
}

Mai_Status Mai_memset(Mai_Void* dst, Mai_I32 v, Mai_I32 size)
{
	for (Mai_I32 a0 = 0; a0 < size; a0++)
	{
		((Mai_I8*)dst)[a0] = (Mai_I8)v;
	}
	return 0;
}
