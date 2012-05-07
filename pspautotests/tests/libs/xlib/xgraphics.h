/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_GRAPHICS_H__
#define __X_GRAPHICS_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

#define X_PSM_5650 GU_PSM_5650
#define X_PSM_5551 GU_PSM_5551
#define X_PSM_4444 GU_PSM_4444
#define X_PSM_8888 GU_PSM_8888

/* graphics modes */
#define X_ALPHA_TEST          (1 << GU_ALPHA_TEST)
#define X_DEPTH_TEST          (1 << GU_DEPTH_TEST)
#define X_SCISSOR_TEST        (1 << GU_SCISSOR_TEST)
#define X_STENCIL_TEST        (1 << GU_STENCIL_TEST)
#define X_BLEND               (1 << GU_BLEND)
#define X_CULL_FACE           (1 << GU_CULL_FACE)
#define X_DITHER              (1 << GU_DITHER)
#define X_FOG                 (1 << GU_FOG)
#define X_CLIP_PLANES         (1 << GU_CLIP_PLANES)
#define X_TEXTURE_2D          (1 << GU_TEXTURE_2D)
#define X_LIGHTING            (1 << GU_LIGHTING)
#define X_LIGHT0              (1 << GU_LIGHT0)
#define X_LIGHT1              (1 << GU_LIGHT1)
#define X_LIGHT2              (1 << GU_LIGHT2)
#define X_LIGHT3              (1 << GU_LIGHT3)
#define X_LINE_SMOOTH         (1 << GU_LINE_SMOOTH)
#define X_PATCH_CULL_FACE     (1 << GU_PATCH_CULL_FACE)
#define X_COLOR_TEST          (1 << GU_COLOR_TEST)
#define X_COLOR_LOGIC_OP      (1 << GU_COLOR_LOGIC_OP)
#define X_FACE_NORMAL_REVERSE (1 << GU_FACE_NORMAL_REVERSE)
#define X_PATCH_FACE          (1 << GU_PATCH_FACE)
#define X_FRAGMENT_2X         (1 << GU_FRAGMENT_2X)
#define X_WAIT_VBLANK         (1 << 25)
#define X_PSEUDO_AA           (1 << 26)
#define X_DITHER_SMOOTH       (1 << 27)

/* texture filter modes */
#define X_NEAREST   0
#define X_BILINEAR  1
#define X_TRILINEAR 2

/* texture effects */
#define X_TFX_MODULATE GU_TFX_MODULATE
#define X_TFX_DECAL    GU_TFX_DECAL
#define X_TFX_BLEND    GU_TFX_BLEND
#define X_TFX_REPLACE  GU_TFX_REPLACE
#define X_TFX_ADD      GU_TFX_ADD

void xGuInit();

void xGuEnd();

int xGuFrameEnd();

void xGuClear(u32 color);

void xGuPerspective(float fovy, float near, float far);

int xGuSetPerspective();

int xGuSetOrtho();

void xGuEnable(int states);

void xGuDisable(int states);

void xGuRenderToTarget(int psm, int w, int h, int tbw, void* tbp);

void xGuRenderToScreen();

void xGuTexFilter(int filter);

void xGuTexMode(int tfx, int alpha);

void xGumLoadIdentity();

void xGumTranslate(float x, float y, float z);

void xGumRotateX(float angle);

void xGumRotateY(float angle);

void xGumRotateZ(float angle);

void xGumScale(float x, float y, float z);

void xGuSaveStates();
void xGuLoadStates();
void* xGuDrawPtr(int uncached, int abs);
void* xGuDispPtr(int uncached, int abs);
void* xGuDepthPtr(int uncached, int abs);
void* xGuStridePtr(int uncached, int abs);
void xGuSetDebugTex();
void xGuDrawTex(int x, int y, int w, int h, int tx, int ty, int tw, int th);
void xGuDrawTexf(float x, float y, float w, float h, float tx, float ty, float tw, float th);

#ifdef __cplusplus
}
#endif

#endif
