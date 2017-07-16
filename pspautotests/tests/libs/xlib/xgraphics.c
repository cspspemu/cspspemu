/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <pspgu.h>
#include <pspgum.h>
#include <pspdisplay.h>
#include "xmem.h"
#include "xmath.h"

#include "xgraphics.h"

#if X_SCREEN_PSM == GU_PSM_5650 || X_SCREEN_PSM == GU_PSM_5551 || X_SCREEN_PSM == GU_PSM_4444
#define X_PIXEL_BYTES 2
#elif X_SCREEN_PSM == GU_PSM_8888
#define X_PIXEL_BYTES 4
#else
#error X_SCREEN_PSM must be equal to GU_PSM_5650, GU_PSM_5551, GU_PSM_4444, or GU_PSM_8888.
#endif

#define X_WHICH_MATRIX (x_states & X_PSEUDO_AA ? x_which_buf : 0)
#define X_WHICH_DITHER (x_states & X_DITHER_SMOOTH ? x_which_buf : 0)

static u32 __attribute__((aligned(16))) x_dlist0[X_LIST_KB*1024/sizeof(u32)];
#ifdef X_DLIST_DOUBLE
static u32 __attribute__((aligned(16))) x_dlist1[X_LIST_KB*1024/sizeof(u32)];
static int x_which_list = 0;
static u32 __attribute__((aligned(16))) x_call_list[32];
#endif

static void* x_draw_buf[2] = {0, 0};
static int x_which_buf = 0;
static void* x_depth_buf = 0;
static int x_states = 0;
static int x_inortho = 0;

static ScePspFMatrix4 x_perspect_mtx[2];
static ScePspFMatrix4 x_ortho_mtx[2];

static int x_saved_states;

static int x_dither_matrix[2][16] =
{
    { 0, 8, 0, 8,
      8, 0, 8, 0,
      0, 8, 0, 8,
      8, 0, 8, 0 },
    { 8, 8, 8, 8,
      0, 8, 0, 8,
      8, 8, 8, 8,
      0, 8, 0, 8 }
};

static int x_initialized = 0;

void xGuInit()
{
    if (x_initialized) return;
    x_initialized = 1;
    
    #ifdef X_DLIST_DOUBLE
    x_which_list = 0;
    #endif
    
    sceGuInit();
    
    #ifndef X_DLIST_DOUBLE
    sceGuStart(GU_DIRECT, x_dlist0);
    #else
    sceGuStart(GU_CALL, (x_which_list == 0 ? x_dlist0 : x_dlist1));
    #endif

    x_which_buf = 0;
    x_draw_buf[0] = X_VREL(x_valloc(X_PIXEL_BYTES*X_SCREEN_STRIDE*X_SCREEN_HEIGHT));
    x_draw_buf[1] = X_VREL(x_valloc(X_PIXEL_BYTES*X_SCREEN_STRIDE*X_SCREEN_HEIGHT));
    sceGuDrawBuffer(X_SCREEN_PSM, x_draw_buf[0], X_SCREEN_STRIDE);
    sceGuDispBuffer(X_SCREEN_WIDTH, X_SCREEN_HEIGHT, x_draw_buf[1], X_SCREEN_STRIDE);
    
    sceGuOffset(2048 - (X_SCREEN_WIDTH/2), 2048 - (X_SCREEN_HEIGHT/2));
    sceGuViewport(2048, 2048, X_SCREEN_WIDTH, X_SCREEN_HEIGHT);
    sceGuScissor(0, 0, X_SCREEN_WIDTH, X_SCREEN_HEIGHT);
    sceGuEnable(GU_SCISSOR_TEST);
    
    #ifdef X_GRAPHICS_3D
    x_depth_buf = X_VREL(x_valloc(2*X_SCREEN_STRIDE*X_SCREEN_HEIGHT));
    sceGuDepthBuffer(x_depth_buf, X_SCREEN_STRIDE);
    sceGuDepthRange(65535, 0);
    sceGuDepthFunc(GU_GEQUAL);
    sceGuClearDepth(0);
    sceGuEnable(GU_DEPTH_TEST);
    sceGuDepthMask(GU_FALSE);
    sceGuEnable(GU_CLIP_PLANES);
    sceGuEnable(GU_CULL_FACE);
    sceGuShadeModel(GU_SMOOTH);
    #else
    sceGuDisable(GU_DEPTH_TEST);
    sceGuDepthMask(GU_TRUE);
    sceGuDisable(GU_CLIP_PLANES);
    sceGuDisable(GU_CULL_FACE);
    sceGuthadeModel(GU_FLAT);
    sceGuEnable(GU_BLEND);
    sceGuBlendFunc(GU_ADD, GU_SRC_ALPHA, GU_ONE_MINUS_SRC_ALPHA, 0, 0);
    #endif

	sceGuTexWrap(GU_CLAMP, GU_CLAMP);
	sceGuTexFilter(GU_NEAREST,GU_NEAREST);
	sceGuTexFunc(GU_TFX_REPLACE, GU_TCC_RGBA);
	sceGuTexEnvColor(0xFFFFFFFF);
	sceGuColor(0xFFFFFFFF);
	sceGuAmbientColor(0xFFFFFFFF);
	sceGuTexOffset(0.0f, 0.0f);
	sceGuTexScale(1.0f, 1.0f);
    
    gumLoadIdentity(&x_ortho_mtx[0]);
    gumOrtho(&x_ortho_mtx[0], 0.0f, 480.0f, 272.0f, 0.0f, -1000.0f, 1000.0f);
    gumLoadIdentity(&x_ortho_mtx[1]);
    ScePspFVector3 displace = {-1.0f/X_SCREEN_WIDTH, 1.0f/X_SCREEN_HEIGHT, 0.0f};
    gumTranslate(&x_ortho_mtx[1], &displace);
    gumMultMatrix(&x_ortho_mtx[1], &x_ortho_mtx[1], &x_ortho_mtx[0]);
    xGuPerspective(75.0f, 0.5f, 1000.0f);
    #ifndef X_GRAPHICS_3D
    xGuSetOrtho();
    #endif
    
    sceGuDisplay(GU_TRUE);
}

void xGuEnd()
{
    if (!x_initialized) return;
    x_free(X_VABS(x_draw_buf[0]));
    x_draw_buf[0] = 0;
    x_free(X_VABS(x_draw_buf[1]));
    x_draw_buf[1] = 0;
    if (x_depth_buf)
    {
        x_free(X_VABS(x_depth_buf));
        x_depth_buf = 0;
    }
    sceGuTerm();
}

int xGuFrameEnd()
{
    /* end current frame */
    int size = sceGuFinish();
    sceGuSync(0, 0);
    if (x_states & X_WAIT_VBLANK || x_states & X_PSEUDO_AA)
    {
        sceDisplayWaitVblankStart();
    }
    x_which_buf = (sceGuSwapBuffers() == x_draw_buf[0] ? 0 : 1);
    #ifdef X_DLIST_DOUBLE
    sceGuStart(GU_DIRECT, x_call_list);
    sceGuCallList((x_which_list == 0 ? x_dlist0 : x_dlist1));
    sceGuFinith();
    x_which_list ^= 1;
    #endif
    if (x_states & X_PSEUDO_AA)
    {
        sceGumMatrixMode(GU_PROJECTION);
        if (!x_inortho) sceGumLoadMatrix(&x_perspect_mtx[X_WHICH_MATRIX]);
        else sceGumLoadMatrix(&x_ortho_mtx[X_WHICH_MATRIX]);
        sceGumMatrixMode(GU_MODEL);
        x_inortho = 0;
    }
    if (x_states & X_DITHER_SMOOTH)
    {
        sceGuSetDither((ScePspIMatrix4*)(&x_dither_matrix[X_WHICH_DITHER]));
    }
    
    /* begin next frame */
    #ifndef X_DLIST_DOUBLE
    sceGuStart(GU_DIRECT, x_dlist0);
    #else
    sceGuStart(GU_CALL, (x_which_list == 0 ? x_dlist0 : x_dlist1));
    #endif
    #ifdef X_GRAPHICS_3D
    sceGuClear(GU_DEPTH_BUFFER_BIT);
    #endif
    sceGumMatrixMode(GU_VIEW);
    sceGumLoadIdentity();
    sceGumMatrixMode(GU_MODEL);
    sceGumLoadIdentity();
    return size;
}

void xGuClear(u32 color)
{
    sceGuClearColor(color);
    sceGuClear(GU_COLOR_BUFFER_BIT|GU_FAST_CLEAR_BIT);
}

void xGuPerspective(float fovy, float near, float far)
{
    gumLoadIdentity(&x_perspect_mtx[0]);
    gumPerspective(&x_perspect_mtx[0], fovy, 16.0f/9.0f, near, far);
    gumLoadIdentity(&x_perspect_mtx[1]);
    ScePspFVector3 displace = {-1.0f/X_SCREEN_WIDTH, 1.0f/X_SCREEN_HEIGHT, 0.0f};
    gumTranslate(&x_perspect_mtx[1], &displace);
    gumMultMatrix(&x_perspect_mtx[1], &x_perspect_mtx[1], &x_perspect_mtx[0]);
    xGuSetPerspective();
}

int xGuSetPerspective()
{
    //if (!x_inortho) return 0;
    sceGumMatrixMode(GU_PROJECTION);
    sceGumLoadMatrix(&x_perspect_mtx[X_WHICH_MATRIX]);
    sceGumMatrixMode(GU_VIEW);
    sceGumLoadIdentity();
    sceGumMatrixMode(GU_MODEL);
    sceGumLoadIdentity();
    sceGuFrontFace(GU_CW);
    x_inortho = 0;
    return 1;
}

int xGuSetOrtho()
{
    //if (x_inortho) return 0;
    sceGumMatrixMode(GU_PROJECTION);
    sceGumLoadMatrix(&x_ortho_mtx[X_WHICH_MATRIX]);
    sceGumMatrixMode(GU_VIEW);
    sceGumLoadIdentity();
    sceGumMatrixMode(GU_MODEL);
    sceGumLoadIdentity();
    sceGuFrontFace(GU_CCW);
    x_inortho = 1;
    return 1;
}

void xGuEnable(int states)
{
	if (states & ~0x1ffffff)
	{
		x_states |= states & ~0x1ffffff;
		if (states & X_DITHER_SMOOTH) states |= X_DITHER;
	}
	states &= 0x1ffffff;
	if (states) sceGuSetAllStatus(sceGuGetAllStatus() | states);
}

void xGuDisable(int states)
{
    if (states & ~0x1ffffff)
	{
		x_states &= ~(states & ~0x1ffffff);
		if (states & X_DITHER_SMOOTH) states |= X_DITHER;
	}
	states &= 0x1ffffff;
	if (states) sceGuSetAllStatus(sceGuGetAllStatus() & ~states);
}

void xGuRenderToTarget(int psm, int width, int height, int tbw, void* tbp)
{
	sceGuDrawBufferList(psm, tbp, tbw);
    sceGuOffset(2048 - (width/2), 2048 - (height/2));
    sceGuViewport(2048, 2048, width, height);
    sceGuScissor(0, 0, width, height);
}

void xGuRenderToScreen()
{
	sceGuDrawBufferList(X_SCREEN_PSM, x_draw_buf[x_which_buf], X_SCREEN_STRIDE);
    sceGuOffset(2048 - (X_SCREEN_WIDTH/2), 2048 - (X_SCREEN_HEIGHT/2));
    sceGuViewport(2048, 2048, X_SCREEN_WIDTH, X_SCREEN_HEIGHT);
    sceGuScissor(0, 0, X_SCREEN_WIDTH, X_SCREEN_HEIGHT);
}

void xGuTexFilter(int filter)
{
	switch (filter)
	{
		case X_BILINEAR:
			sceGuTexFilter(GU_LINEAR, GU_LINEAR);
			break;
		case X_TRILINEAR:
			sceGuTexFilter(GU_LINEAR_MIPMAP_LINEAR, GU_LINEAR_MIPMAP_LINEAR);
			break;
		default:
			sceGuTexFilter(GU_NEAREST, GU_NEAREST);
			break;
	}
}

inline void xGuTexMode(int tfx, int alpha)
{
    sceGuTexFunc(tfx, (alpha ? GU_TCC_RGBA : GU_TCC_RGB));
}

inline void xGumLoadIdentity()
{
    sceGumLoadIdentity();
}

inline void xGumTranslate(float x, float y, float z)
{
    ScePspFVector3 trans = {x,y,z};
    sceGumTranslate(&trans);
}

inline void xGumRotateX(float angle)
{
    sceGumRotateX(angle);
}

inline void xGumRotateY(float angle)
{
    sceGumRotateY(angle);
}

inline void xGumRotateZ(float angle)
{
    sceGumRotateZ(angle);
}

inline void xGumScale(float x, float y, float z)
{
    ScePspFVector3 scale = {x,y,z};
    sceGumScale(&scale);
}

inline void xGuSaveStates()
{
    x_saved_states = x_states|sceGuGetAllStatus();
}
inline void xGuLoadStates()
{
    sceGuSetAllStatus(x_saved_states & 0x1ffffff);
    xGuEnable(x_saved_states & ~0x1ffffff);
}

inline void* xGuDrawPtr(int uncached, int abs)
{
    u32 ptr = (u32)x_draw_buf[x_which_buf];
    if (uncached) ptr |= X_MEM_NO_CACHE;
    if (abs) ptr |= X_MEM_VRAM;
    return (void*)ptr;
}

inline void* xGuDispPtr(int uncached, int abs)
{
    u32 ptr = (u32)x_draw_buf[x_which_buf^1];
    if (uncached) ptr |= X_MEM_NO_CACHE;
    if (abs) ptr |= X_MEM_VRAM;
    return (void*)ptr;
}

inline void* xGuDepthPtr(int uncached, int abs)
{
    u32 ptr = (u32)x_depth_buf;
    if (uncached) ptr |= X_MEM_NO_CACHE;
    if (abs) ptr |= X_MEM_VRAM;
    return (void*)ptr;
}

inline void* xGuStridePtr(int uncached, int abs)
{
    u32 ptr = (u32)x_draw_buf[0] + X_SCREEN_WIDTH*X_PIXEL_BYTES;
    if (uncached) ptr |= X_MEM_NO_CACHE;
    if (abs) ptr |= X_MEM_VRAM;
    return (void*)ptr;
}

#define TEX_DEBUG_PSM (GU_PSM_8888)
#define TEX_DEBUG_SCALE (8.0f)
#define TEX_DEBUG_WIDTH (2)
#define TEX_DEBUG_HEIGHT (2)

static u32 __attribute__((aligned(16))) debug_texture[4] = {
    0xffff00ff, 0xff000000,
    0xff000000, 0xffff00ff
};

void xGuSetDebugTex()
{
	sceGuTexWrap(GU_REPEAT, GU_REPEAT);
    sceGuTexMode(TEX_DEBUG_PSM, 0, 0, 0);
    sceGuTexScale(TEX_DEBUG_SCALE, TEX_DEBUG_SCALE);
    sceGuTexImage(0, TEX_DEBUG_WIDTH, TEX_DEBUG_HEIGHT, TEX_DEBUG_WIDTH, debug_texture);
}

#define TEX_SLICE (64)

void xGuDrawTex(int x, int y, int w, int h, int tx, int ty, int tw, int th)
{
	float cur_u = (float)tx;
	float ustep = (float)TEX_SLICE*tw/w;
	int slice_width = 0;
	float tex_step = 0;
	int i;
	for (i = x; i < x + w; i += TEX_SLICE)
	{
		slice_width = (i + TEX_SLICE > x + w ? x + w - i : TEX_SLICE);
		tex_step = (cur_u + ustep > tx + tw ? tx + tw - cur_u : ustep);

		TVertex2D* vertices = (TVertex2D*)sceGuGetMemory(2*sizeof(TVertex2D));
		vertices[0].u = (s16)cur_u;
		vertices[0].v = ty + th;
		vertices[0].x = i;
		vertices[0].y = y;
		vertices[0].z = 0;
		cur_u += tex_step;
		vertices[1].u = (s16)cur_u;
		vertices[1].v = ty;
		vertices[1].x = i + slice_width;
		vertices[1].y = y + h;
		vertices[1].z = 0;
		sceGuDrawArray(GU_SPRITES, TVertex2D_vtype|GU_TRANSFORM_2D, 2, 0, vertices);
	}
}

void xGuDrawTexf(float x, float y, float w, float h, float tx, float ty, float tw, float th)
{
	float cur_u = tx;
	float ustep = TEX_SLICE*tw/w;
	float slice_width = 0;
	float tex_step = 0;
	int i;
	for (i = x; i < x + w; i += TEX_SLICE)
	{
		slice_width = (i + TEX_SLICE > x + w ? x + w - i : TEX_SLICE);
		tex_step = (cur_u + ustep > tx + tw ? tx + tw - cur_u : ustep);

		TVertexF* vertices = (TVertexF*)sceGuGetMemory(2*sizeof(TVertexF));
		vertices[0].u = cur_u;
		vertices[0].v = ty + th;
		vertices[0].x = i;
		vertices[0].y = y;
		vertices[0].z = 0;
		cur_u += tex_step;
		vertices[1].u = cur_u;
		vertices[1].v = ty;
		vertices[1].x = i + slice_width;
		vertices[1].y = y + h;
		vertices[1].z = 0;
		sceGuDrawArray(GU_SPRITES, TVertexF_vtype|GU_TRANSFORM_2D, 2, 0, vertices);
	}
}
