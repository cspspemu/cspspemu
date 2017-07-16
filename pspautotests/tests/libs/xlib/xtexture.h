/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_TEXTURE_H__
#define __X_TEXTURE_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

#define X_TEX_TOP_IN_VRAM   (1<<0) /* top level in vram */
#define X_TEX_MIPS_IN_VRAM  (1<<1) /* all mipmaps in vram */
#define X_TEX_ALL_IN_VRAM   (X_TEX_TOP_IN_VRAM|X_TEX_MIPS_IN_VRAM) /* all levels in vram */
#define X_TEX_GRAY_TO_ALPHA (1<<2)

typedef struct x_mipmap {
    u16 width;
    u16 height;
    u16 buf_width;
    u16 buf_height;
    u16 pow2_height;
    void* data;
} x_mipmap;

typedef struct xTexture {
    int cpsm;
    int psm;
    int clut_entries;
    u8 swizzled;
    u16 width;
    u16 height;
    u16 buf_width;
    u16 buf_height;
    u16 pow2_height;
    float u_scale;
    float v_scale;
    void* clut;
    void* data;
    int num_mips;
    x_mipmap* mipmaps;
} xTexture;

xTexture* xTexLoadTex(char* filename, int levels, int flags);

xTexture* xTexLoadTGA(char* filename, int levels, int flags);

xTexture* xTexLoadPNG(char* filename, int levels, int flags);

xTexture* xTexLoadBMP(char* filename, int levels, int flags);

void xTexFree(xTexture* t);

int xTexUploadVRAM(xTexture* t, int level);

int xTexDownloadVRAM(xTexture* t, int level);

void xTexSetImage(xTexture* t);

/* draws texture from top left - must set blend modes and vert colors manually */
void xTexDraw(xTexture* t, int x, int y, int w, int h, int tx, int ty, int tw, int th);

/* draws texture from center with width w and height h, at a rotation of angle */
void xTexDrawAngle(xTexture* t, float x, float y, float w, float h, int tx, int ty, int tw, int th, float angle);

#ifdef __cplusplus
}
#endif

#endif
