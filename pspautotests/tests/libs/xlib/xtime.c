/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <stdio.h>
#include <psptypes.h>
#include <psprtc.h>

#include "xtime.h"

static u64 time_cur;
static u64 time_last;
static float inv_tick_res;
static float delta_time;
static float rel_last_sec;
static float rel_time;
static u32 frames_passed;
static u32 fps;

void xTimeInit()
{
    sceRtcGetCurrentTick(&time_last);
    inv_tick_res = 1.0f/sceRtcGetTickResolution();
    rel_last_sec = 0.0f;
    rel_time = 0.0f;
    frames_passed = 0;
}

void xTimeUpdate()
{
    sceRtcGetCurrentTick(&time_cur);
    delta_time = (time_cur - time_last) * inv_tick_res;
    time_last = time_cur;
    rel_time += delta_time;
    frames_passed += 1;
    if (rel_time >= rel_last_sec + 1.0f)
    {
        fps = frames_passed;
        frames_passed = 0;
        rel_last_sec = rel_time;
    }
}

inline float xTimeGetDeltaTime()
{
    return delta_time;
}

inline int xTimeFpsApprox()
{
    return (int)fps;
}

inline float xTimeSecPassed()
{
    return rel_time;
}
