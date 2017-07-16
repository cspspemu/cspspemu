/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <pspgu.h>
#include "xmem.h"
#include "xmath.h"
#include "xgraphics.h"

#include "xbuffer.h"

xBuffer* xBufferConstruct(int psm, int width, int height)
{
    xBuffer* b = (xBuffer*)x_malloc(sizeof(xBuffer));
    if (!b) return 0;
    
    int Bpp = 0;
    switch (psm)
    {
        case GU_PSM_5650:
        case GU_PSM_5551:
        case GU_PSM_4444:
            Bpp = 2;
            break;
        default:
            Bpp = 4;
            psm = GU_PSM_8888;
            break;
    }
    b->psm = psm;
    b->width = width;
    b->height = height;
    b->buf_width = x_next_pow2(width);
    b->pow2_height = x_next_pow2(height);
    b->u_scale = (float)b->width/b->buf_width;
    b->v_scale = (float)b->height/b->pow2_height;
    b->data = x_valloc(Bpp*b->buf_width*b->height);
    if (!b->data)
	{
		return 0;
	}
    return b;
}

void xBufferDestroy(xBuffer* b)
{
    if (!b) return;
    if (b->data) x_free(b->data);
	x_free(b);
}

static xBuffer frame_buf = {
	X_SCREEN_PSM,
	X_SCREEN_WIDTH, X_SCREEN_HEIGHT,
	X_SCREEN_STRIDE, 512,
	(float)X_SCREEN_WIDTH/X_SCREEN_STRIDE, (float)X_SCREEN_HEIGHT/512,
	0
};

xBuffer* xBufferFrameBuffer()
{
	frame_buf.data = xGuDrawPtr(0, 1);
	return &frame_buf;
}

void xBufferSetImage(xBuffer* b)
{
	if (!b)
	{
		xGuSetDebugTex();
		return;
	}
	if (!b->data)
	{
		xGuSetDebugTex();
		return;
	}
	sceGuTexScale(b->u_scale, b->v_scale);
	sceGuTexMode(b->psm, 0, 0, 0);
	sceGuTexImage(0, b->buf_width, b->pow2_height, b->buf_width, b->data);
}

int xBufferSetRenderTarget(xBuffer* b)
{
	if (!b) return 1;
	if (!b->data) return 1;
	xGuRenderToTarget(b->psm, b->width, b->height, b->buf_width, X_VREL(b->data));
	return 0;
}

void xBufferDrawA2B(xBuffer* a, xBuffer* b)
{
	if (!a || !b) return;
	if (xBufferSetRenderTarget(b) != 0) return;
	xGuTexMode(GU_TFX_REPLACE, 0);
	xBufferSetImage(a);
	xGuSaveStates();
	sceGuEnable(GU_TEXTURE_2D);
	sceGuDisable(GU_LIGHTING);
	sceGuDisable(GU_DEPTH_TEST);
	sceGuDisable(GU_DITHER);
	sceGuDepthMask(GU_TRUE);
	xGuDrawTex(0, 0, b->width, b->height, 0, 0, a->width, a->height);
	sceGuDepthMask(GU_FALSE);
	xGuLoadStates();
	xGuRenderToScreen();
}

void xBuffer4x4Pcf(xBuffer* a, xBuffer* b)
{
	if (!a || !b) return;
	if (xBufferSetRenderTarget(b) != 0) return;
	xGuTexMode(GU_TFX_REPLACE, 0);
	xBufferSetImage(a);
	xGuSaveStates();
	xGuTexFilter(X_BILINEAR);
	sceGuEnable(GU_TEXTURE_2D);
	sceGuEnable(GU_BLEND);
	sceGuDisable(GU_LIGHTING);
	sceGuDisable(GU_DEPTH_TEST);
	sceGuDisable(GU_DITHER);
	sceGuDepthMask(GU_TRUE);
	
	sceGuBlendFunc(GU_ADD, GU_FIX, GU_FIX, 0x3f3f3f3f, 0x000000);
	xGuDrawTexf(-1.5f, 1.5f, b->width, b->height, 0, 0, a->width, a->height);
	sceGuBlendFunc(GU_ADD, GU_FIX, GU_FIX, 0x40404040, 0xffffffff);
	xGuDrawTexf(0.5f, -1.5f, b->width, b->height, 0, 0, a->width, a->height);
	xGuDrawTexf(-1.5f, 0.5f, b->width, b->height, 0, 0, a->width, a->height);
	xGuDrawTexf(0.5f, 0.5f, b->width, b->height, 0, 0, a->width, a->height);
	
	sceGuDepthMask(GU_FALSE);
	xGuLoadStates();
	xGuRenderToScreen();
}
