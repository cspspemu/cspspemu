/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_BUFFER_H__
#define __X_BUFFER_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct xBuffer {
    int psm;
    u16 width;
    u16 height;
    u16 buf_width;
    u16 pow2_height;
    float u_scale;
    float v_scale;
    void* data;
} xBuffer;

xBuffer* xBufferConstruct(int psm, int width, int height);

void xBufferDestroy(xBuffer* b);

xBuffer* xBufferFrameBuffer();

void xBufferSetImage(xBuffer* b);

int xBufferSetRenderTarget(xBuffer* b);

void xBufferDrawA2B(xBuffer* a, xBuffer* b);

void xBuffer4x4Pcf(xBuffer* a, xBuffer* b);

#ifdef __cplusplus
}
#endif

#endif
