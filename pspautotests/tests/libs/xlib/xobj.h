/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_OBJ_H__
#define __X_OBJ_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct xObj {
	int type;
	int vtype;
	ScePspFVector3 bbox[8];
	ScePspFVector3 scale;
	ScePspFVector3 pos;
    int num_verts;
	void* vertices;
} xObj;

xObj* xObjLoad(char* filename, int optimize);

void xObjFree(xObj* object);

void xObjTranslate(xObj* object);

void xObjDraw(xObj* object, int reverse_frontface);

#ifdef __cplusplus
}
#endif

#endif
