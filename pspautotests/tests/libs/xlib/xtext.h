/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_TEXT_H__
#define __X_TEXT_H__

#include "xconfig.h"
#include "xtexture.h"

#define X_ALIGN_LEFT   (0)
#define X_ALIGN_CENTER (1)
#define X_ALIGN_RIGHT  (2)

/* things to add
    scale is independent of texture size - rather it is the pixel size of the characters
    constant char width (another var in xBitmapFont)
    wordwrap - shouldn't be too hard with length functions
    xTextPrintSize( ... , float xsize, float ysize, ... )
    ?
 */

typedef struct xBitmapFont {
    xTexture* texture;
    u16 widths[256];
} xBitmapFont;

/* texture should be pow2 aligned and contain left-justified characters */
/* width file should contain 16bit unsigned character widths */
/* http://www.lmnopc.com/bitmapfontbuilder/ */
xBitmapFont* xTextLoadFont(xTexture* tex, char* widths_filename);

void xTextFreeFont(xBitmapFont* font);

void xTextSetFont(xBitmapFont* font);

void xTextSetColor(u32 color);

void xTextSetScale(float scale);

void xTextSetAlign(int align);

int xTextLength(char* text, int num);

int xTextNumWithLength(char* src, int length);

int xTextPrint(int x, int y, char* text);

int xTextPrintf(int x, int y, char* text, ... );

/* unimplemented/unworking
float xText3DPrint(ScePspFVector3* pos, float char_scale, float height, char* text, int num);

float xText3DPrintf(ScePspFVector3* pos, float char_scale, float height, char* text, ... );
*/

#endif
