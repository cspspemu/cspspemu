/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_INI_H__
#define __X_INI_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

// notfound variable - what to return when parameter is not found

int xIniGetInt(FILE* f, char* param, int notfound);

float xIniGetFloat(FILE* f, char* param, float notfound);

void xIniGetVectorf(FILE* f, char* param, float* x, float* y, float* z);

void xIniGetVectori(FILE* f, char* param, int* x, int* y, int* z);

char* xIniGetString(FILE* f, char* param, char* str, char* notfound);

#ifdef __cplusplus
}
#endif

#endif
