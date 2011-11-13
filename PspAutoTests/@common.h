#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <pspdebug.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspctrl.h>
#include <pspiofilemgr.h>
//#include <pspkdebug.h>

#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <limits.h>
#include <sys/lock.h>
#include <sys/fcntl.h>
//#include "local.h"

PSP_MODULE_INFO("TESTMODULE", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER | THREAD_ATTR_VFPU);

//#define USE_EMIT_IMPORTS

#ifdef USE_EMIT_IMPORTS
	extern void emitInt(int v);
	extern void emitFloat(float v);
	extern void emitString(char *v);
	extern void emitComment(char *v);
	extern void emitMemoryBlock(void *address, unsigned int size);
	extern void emitHex(void *address, unsigned int size);
	extern void emitUInt(unsigned int v);
#else
	void emitInt(int v) { asm("syscall 0x1010"); }
	void emitFloat(float v) { asm("syscall 0x1011"); }
	void emitString(char *v) { asm("syscall 0x1012"); }
	void emitComment(char *v) { asm("syscall 0x1012"); }
	void emitMemoryBlock(void *address, unsigned int size) { asm("syscall 0x1013"); }
	void emitHex(void *address, unsigned int size) { asm("syscall 0x1014"); }
	void emitUInt(unsigned int v) { asm("syscall 0x1015"); }
#endif

#define lengthof(K) (sizeof((K)) / sizeof((K)[0]))
