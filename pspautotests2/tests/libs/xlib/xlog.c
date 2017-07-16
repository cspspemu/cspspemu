/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <stdarg.h>
#include <stdio.h>
#include <psptypes.h>
#include <psprtc.h>

#include "xlog.h"

#define X_BUFFER_SIZE (1024)

FILE* xlog_file = 0;
char buffer[X_BUFFER_SIZE];

void xLogPrint(char* text)
{
    xlog_file = fopen("./xlog.txt", "a");
    if (!xlog_file) return;
    pspTime time_struct;
    sceRtcGetCurrentClockLocalTime(&time_struct);
    fprintf(xlog_file, "[%02u:%02u:%02u:%06u] %s\r\n", time_struct.hour, time_struct.minutes, time_struct.seconds, (unsigned int)time_struct.microseconds, text);
    fclose(xlog_file);
    xlog_file = 0;
}

void xLogPrintf(char* text, ... )
{
#ifdef X_DEBUG
    va_list ap;
    va_start(ap, text);
	vsnprintf(buffer, X_BUFFER_SIZE, text, ap);
    va_end(ap);
    xLogPrint(buffer);
#endif
}
