/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_CTRL_H__
#define __X_CTRL_H__

#include <pspctrl.h>
#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

#define X_CTRL_SELECT   PSP_CTRL_SELECT
#define X_CTRL_START    PSP_CTRL_START
#define X_CTRL_UP       PSP_CTRL_UP
#define X_CTRL_RIGHT    PSP_CTRL_RIGHT
#define X_CTRL_DOWN     PSP_CTRL_DOWN
#define X_CTRL_LEFT     PSP_CTRL_LEFT
#define X_CTRL_LTRIGGER PSP_CTRL_LTRIGGER
#define X_CTRL_RTRIGGER PSP_CTRL_RTRIGGER
#define X_CTRL_TRIANGLE PSP_CTRL_TRIANGLE
#define X_CTRL_CIRCLE   PSP_CTRL_CIRCLE
#define X_CTRL_CROSS    PSP_CTRL_CROSS
#define X_CTRL_SQUARE   PSP_CTRL_SQUARE
/* the following are not guaranteed to work */
#define X_CTRL_HOME     PSP_CTRL_HOME
#define X_CTRL_HOLD     PSP_CTRL_HOLD
#define X_CTRL_NOTE     PSP_CTRL_NOTE
#define X_CTRL_SCREEN   PSP_CTRL_SCREEN
#define X_CTRL_VOLUP    PSP_CTRL_VOLUP
#define X_CTRL_VOLDOWN  PSP_CTRL_VOLDOWN
#define X_CTRL_WLAN_UP  PSP_CTRL_WLAN_UP
#define X_CTRL_REMOTE   PSP_CTRL_REMOTE
#define X_CTRL_DISC     PSP_CTRL_DISC
#define X_CTRL_MS       PSP_CTRL_MS

void xCtrlInit();
u32 xCtrlUpdate(float dt);
int xCtrlPress(u32 buttons);
int xCtrlTap(u32 buttons);
int xCtrlHold(u32 buttons, float seconds);
int xCtrlAnalogAlive(float deadzone);
float xCtrlAnalogX();
float xCtrlAnalogY();

#ifdef __cplusplus
}
#endif

#endif
