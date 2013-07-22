#ifndef Mai_mem_h
#define Mai_mem_h

#include "SUB/Mai_All.h"

/* Mai_mem */
Mai_Status Mai_memcpy(Mai_Void* dst, Mai_Void* src, Mai_I32 size);
Mai_Status Mai_memset(Mai_Void* dst, Mai_I32 v, Mai_I32 size);

#define Mai_htons(in) ( ( ((in) << 8) & 0xFF00 ) | ( ((in) >> 8) & 0xFF ) )
#define Mai_htonl(in) ( ( ((in) << 24) & 0xFF000000 ) | ( ((in) << 8) & 0xFF0000 ) | ( ((in) >> 8) & 0xFF00 ) | ( ((in) >> 24) & 0xFF ) )

#endif
