#ifndef _COMMON_H
#define _COMMON_H 1

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <string.h>

#undef main
#define main test_main

extern int RUNNING_ON_EMULATOR;

/*
void emitInt(int v);
void emitFloat(float v);
void emitString(char *v);
void emitComment(char *v);
void emitMemoryBlock(void *address, unsigned int size);
void emitHex(void *address, unsigned int size);
#define emitStringf(format, ...) { char temp[1024]; sprintf(temp, format, __VA_ARGS__); emitString(temp); }
*/

#endif