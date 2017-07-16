/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_MEMORY_H__
#define __X_MEMORY_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

/* No-cache memory area, OR with pointers to bypass cache */
#define X_MEM_NO_CACHE      (0x40000000)

/* Allegrex Scrathcpad, 16 KiB */
#define X_MEM_SCRATCH       (0x00010000)
#define X_MEM_SCRATCH_SIZE  (0x00004000)

/* Ge VRAM, 2 MiB */
#define X_MEM_VRAM          (0x04000000)
#define X_MEM_VRAM_SIZE     (0x00200000)
#define X_MEM_VRAM_NO_CACHE (X_MEM_NO_CACHE|X_MEM_VRAM)

/* Allegrex User Memory, 24 MiB */
#define X_MEM_USER          (0x08800000)
#define X_MEM_USER_SIZE     (0x01800000)
#define X_MEM_USER_NO_CACHE (X_MEM_NO_CACHE|X_MEM_USER)

/* Allegrex Kernel Memory, 8 MiB */
#define X_MEM_KERNEL        (0x88000000)
#define X_MEM_KERNEL_SIZE   (0x00800000)

/* Macros for easy memory management */
#define X_NEW(TYPE, SIZE) (TYPE*)x_malloc((SIZE)*sizeof(TYPE))
#define X_DELETE(PTR) do{if (PTR) x_free(PTR); PTR = 0;}while(0)

/* Macros for modifying VRAM pointers */
#define X_VREL(PTR) ((void*)((u32)(PTR) & ~X_MEM_VRAM))
#define X_VABS(PTR) ((void*)((u32)(PTR) | X_MEM_VRAM))
#define X_VFREE(REL_PTR) x_free(X_VABS(REL_PTR))

/* Macros for modifying cached/uncached pointers */
#define X_CACHED(PTR) ((void*)((u32)(PTR) & ~X_MEM_NO_CACHE))
#define X_UNCACHED(PTR) ((void*)((u32)(PTR) | X_MEM_NO_CACHE))

/* allocate to main mem */
void* x_malloc(u32 size);

/* allocate to vram */
void* x_valloc(u32 size);

/* allocate to scratch pad */
void* x_salloc(u32 size);

/* realloc previously malloced main mem */
void* x_remalloc(void* ptr, u32 size);

/* free any kind of mem */
void x_free(void* ptr);

u32 x_vlargest();
u32 x_slargest();

#ifdef __cplusplus
}
#endif

#endif
