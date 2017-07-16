#ifndef _COMMON_H
#define _COMMON_H 1

#ifdef __cplusplus
extern "C" {
#endif

// Just to make IntelliSense happy, not attempting to compile.
#ifdef _MSC_VER
#define __builtin_va_list int
#include <stdarg.h>

#define __inline__ inline
#define __attribute__(...)
#define __extension__

#ifndef __builtin_va_start
void __va_start(va_list, ...);
void *__va_arg(va_list, ...);
void __va_end(va_list);

#define __builtin_va_start __va_start
#define __builtin_va_arg __va_arg
#define __builtin_va_end __va_end
#endif
#endif

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <string.h>
#include <pspctrl.h>
#include <pspkerneltypes.h>

#undef main
#define main test_main

extern unsigned int RUNNING_ON_EMULATOR;
extern unsigned int CHECKPOINT_ENABLE_TIME;
// Causes rescheduling (sceIoWrite) but easier to debug in the emulator.
extern unsigned int CHECKPOINT_OUTPUT_DIRECT;
extern char schedfBuffer[65536];
extern unsigned int schedfBufferPos;

void schedf(const char *format, ...);
void flushschedf();
int reschedFunc(SceSize argc, void *argp);
void checkpoint(const char *format, ...);
void checkpointNext(const char *title);

#define ARRAY_SIZE(a) (sizeof((a)) / (sizeof((a)[0])))

void emulatorEmitScreenshot();
void emulatorSendSceCtrlData(SceCtrlData* pad_data);

/*
void emitInt(int v);
void emitFloat(float v);
void emitString(char *v);
void emitComment(char *v);
void emitMemoryBlock(void *address, unsigned int size);
void emitHex(void *address, unsigned int size);
#define emitStringf(format, ...) { char temp[1024]; sprintf(temp, format, __VA_ARGS__); emitString(temp); }
*/

#ifdef __cplusplus
}
#endif

#endif