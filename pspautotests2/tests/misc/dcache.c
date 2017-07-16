#define sceKernelDcacheWritebackAll sceKernelDcacheWritebackAll_IGNORE
#define sceKernelDcacheWritebackInvalidateAll sceKernelDcacheWritebackInvalidateAll_IGNORE
#define sceKernelDcacheWritebackRange sceKernelDcacheWritebackRange_IGNORE
#define sceKernelDcacheWritebackInvalidateRange sceKernelDcacheWritebackInvalidateRange_IGNORE
#define sceKernelDcacheInvalidateRange sceKernelDcacheInvalidateRange_IGNORE

#include <common.h>

#include <stdio.h>
#include <time.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <psprtc.h>

#undef sceKernelDcacheWritebackAll
#undef sceKernelDcacheWritebackInvalidateAll
#undef sceKernelDcacheWritebackRange
#undef sceKernelDcacheWritebackInvalidateRange
#undef sceKernelDcacheInvalidateRange

// These are probably the correct signatures?
extern int sceKernelDcacheWritebackAll(void);
extern int sceKernelDcacheWritebackInvalidateAll(void);
extern int sceKernelDcacheWritebackRange(const void *p, int size);
extern int sceKernelDcacheWritebackInvalidateRange(const void *p, int size);
extern int sceKernelDcacheInvalidateRange(const void *p, int size);

int myfunc() {
	// Intentionally returns nothing, sorry.
}

unsigned int __attribute__((aligned(64))) cachevar;

void testCache() {
	int result;

	asm("lui $v0, 0xDEAD");
	printf("sceKernelDcacheWritebackAll: %08x\n", sceKernelDcacheWritebackAll());

	asm("lui $v0, 0xDEAD");
	printf("sceKernelDcacheWritebackInvalidateAll: %08x\n", sceKernelDcacheWritebackInvalidateAll());

	asm("lui $v0, 0xDEAD");
	printf("sceKernelDcacheWritebackRange NULL: %08x\n", sceKernelDcacheWritebackRange(NULL, 0));
	printf("sceKernelDcacheWritebackRange UINT_MAX: %08x\n", sceKernelDcacheWritebackRange(NULL, 0xFFFFFFFF));
	printf("sceKernelDcacheWritebackRange INT_MAX: %08x\n", sceKernelDcacheWritebackRange(NULL, 0x7FFFFFFF));
	printf("sceKernelDcacheWritebackRange 0x8FFFFFFF: %08x\n", sceKernelDcacheWritebackRange(NULL, 0x8FFFFFFF));
	printf("sceKernelDcacheWritebackRange w/address: %08x\n", sceKernelDcacheWritebackRange(&result, 0));
	printf("sceKernelDcacheWritebackRange w/address INT_MAX: %08x\n", sceKernelDcacheWritebackRange(&result, 0x7FFFFFFF));
	printf("sceKernelDcacheWritebackRange w/address -4: %08x\n", sceKernelDcacheWritebackRange(&result, -4));
	printf("sceKernelDcacheWritebackRange w/address 4: %08x\n", sceKernelDcacheWritebackRange(&result, 4));

	asm("lui $v0, 0xDEAD");
	printf("sceKernelDcacheWritebackInvalidateRange NULL: %08x\n", sceKernelDcacheWritebackInvalidateRange(NULL, 0));
	printf("sceKernelDcacheWritebackInvalidateRange UINT_MAX: %08x\n", sceKernelDcacheWritebackInvalidateRange(NULL, 0xFFFFFFFF));
	printf("sceKernelDcacheWritebackInvalidateRange INT_MAX: %08x\n", sceKernelDcacheWritebackInvalidateRange(NULL, 0x7FFFFFFF));
	printf("sceKernelDcacheWritebackInvalidateRange w/address: %08x\n", sceKernelDcacheWritebackInvalidateRange(&result, 0));
	printf("sceKernelDcacheWritebackInvalidateRange w/address INT_MAX: %08x\n", sceKernelDcacheWritebackInvalidateRange(&result, 0x7FFFFFFF));
	printf("sceKernelDcacheWritebackInvalidateRange w/address -4: %08x\n", sceKernelDcacheWritebackInvalidateRange(&result, -4));
	printf("sceKernelDcacheWritebackInvalidateRange w/address 4: %08x\n", sceKernelDcacheWritebackInvalidateRange(&result, 4));

	asm("lui $v0, 0xDEAD");
	printf("sceKernelDcacheInvalidateRange NULL: %08x\n", sceKernelDcacheInvalidateRange(NULL, 0));
	printf("sceKernelDcacheInvalidateRange UINT_MAX: %08x\n", sceKernelDcacheInvalidateRange(NULL, 0xFFFFFFFF));
	printf("sceKernelDcacheInvalidateRange INT_MAX: %08x\n", sceKernelDcacheInvalidateRange(NULL, 0x7FFFFFFF));
	printf("sceKernelDcacheInvalidateRange w/address: %08x\n", sceKernelDcacheInvalidateRange(&result, 0));
	printf("sceKernelDcacheInvalidateRange w/address INT_MAX: %08x\n", sceKernelDcacheInvalidateRange(&result, 0x7FFFFFFF));
	printf("sceKernelDcacheInvalidateRange w/address 24MB: %08x\n", sceKernelDcacheInvalidateRange(&result, 0x74000000));
	printf("sceKernelDcacheInvalidateRange w/address -4: %08x\n", sceKernelDcacheInvalidateRange(&result, -4));
	printf("sceKernelDcacheInvalidateRange w/address 4: %08x\n", sceKernelDcacheInvalidateRange(&result, 4));
	printf("sceKernelDcacheInvalidateRange w/address 64: %08x\n", sceKernelDcacheInvalidateRange(&result, 64));
	printf("sceKernelDcacheInvalidateRange w/aligned: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 0));
	printf("sceKernelDcacheInvalidateRange w/aligned INT_MAX - 255: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 0x7FFFFF00));
	printf("sceKernelDcacheInvalidateRange w/aligned -64: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, -64));
	printf("sceKernelDcacheInvalidateRange w/aligned 4: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 4));
	printf("sceKernelDcacheInvalidateRange w/aligned 8: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 8));
	printf("sceKernelDcacheInvalidateRange w/aligned 16: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 16));
	printf("sceKernelDcacheInvalidateRange w/aligned 32: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 32));
	printf("sceKernelDcacheInvalidateRange w/aligned 64: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 64));
	printf("sceKernelDcacheInvalidateRange w/aligned 96: %08x\n", sceKernelDcacheInvalidateRange(&cachevar, 96));
	printf("sceKernelDcacheInvalidateRange w/off aligned 32: %08x\n", sceKernelDcacheInvalidateRange(&cachevar + 32, 32));

	int bases[] = {
		0x00000000, 0x10000000, 0x20000000, 0x30000000, 0x40000000, 0x50000000, 0x60000000, 0x70000000,
		0x80000000, 0x90000000, 0xA0000000, 0xB0000000, 0xC0000000, 0xD0000000, 0xE0000000, 0xF0000000,
		0x7CFFFF80, 0x7FFFFFBF, 0x7FFFFFFF,
	};
	int i;
	for (i = 0; i < sizeof(bases) / sizeof(bases[0]); ++i) {
		printf("sceKernelDcacheInvalidateRange @%08x + 64: %08x\n", bases[i], sceKernelDcacheInvalidateRange(bases[i], 64));
	}

	// Just to show that this test works.
	asm("lui $v0, 0xDEAD");
	result = myfunc();
	printf("myfunc: %08x\n", result);
}

int main(int argc, char **argv) {
	testCache();
	return 0;
}