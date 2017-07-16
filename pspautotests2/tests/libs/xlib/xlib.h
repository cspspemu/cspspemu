/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_LIB_H__
#define __X_LIB_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

int xRunning();
void xExit();

/* main function, defined by user, ran through real main */
extern int xMain();

#ifdef __cplusplus
}
#endif

#endif
