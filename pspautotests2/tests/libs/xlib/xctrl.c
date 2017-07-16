/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <string.h>
#include <pspctrl.h>

#include "xctrl.h"

#define ONE_OVER_127point5 (0.0078431373f)

#define BUTTONS_PRESSED(check, buttons) (((check) & (buttons)) == (buttons))

static u32 x_buttons_cur;
static u32 x_buttons_last;
static float x_analog_x;
static float x_analog_y;
static float x_button_held[32];

void xCtrlInit()
{
    sceCtrlSetSamplingCycle(0);
    sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);
    x_buttons_cur = 0;
    x_buttons_last = 0;
    x_analog_x = 0.0f;
    x_analog_y = 0.0f;
    int i;
    for (i = 0; i < 32; i++)
    {
        x_button_held[i] = 0.0f;
    }
}

u32 xCtrlUpdate(float dt)
{
    SceCtrlData ctrl_read;
    sceCtrlPeekBufferPositive(&ctrl_read, 1);
    x_analog_x = (ctrl_read.Lx - 127.5f) * ONE_OVER_127point5;
    x_analog_y = -(ctrl_read.Ly - 127.5f) * ONE_OVER_127point5;
    x_buttons_last = x_buttons_cur;
    x_buttons_cur = ctrl_read.Buttons;
    int i;
    if (dt > 0.0f)
    {
        for (i = 0; i < 32; i++)
        {
            if (BUTTONS_PRESSED(x_buttons_cur, 1<<i)) x_button_held[i] += dt;
            else x_button_held[i] = 0.0f;
        }
    }
    return x_buttons_cur;
}

inline int xCtrlPress(u32 buttons)
{
    return BUTTONS_PRESSED(x_buttons_cur, buttons);
}

inline int xCtrlTap(u32 buttons)
{
    return (BUTTONS_PRESSED(x_buttons_cur, buttons) && !BUTTONS_PRESSED(x_buttons_last, buttons));
}

int xCtrlHold(u32 buttons, float seconds)
{
    int i;
    for (i = 0; i < 32; i++)
    {
        if (buttons & (1<<i))
        {
            if (x_button_held[i] < seconds)
            {
                return 0;
            }
        }
    }
    return 1;
}

inline int xCtrlAnalogAlive(float deadzone)
{
    return (x_analog_x > deadzone || x_analog_x < -deadzone || x_analog_y > deadzone || x_analog_y < -deadzone);
}

inline float xCtrlAnalogX()
{
    return x_analog_x;
}

inline float xCtrlAnalogY()
{
    return x_analog_y;
}
