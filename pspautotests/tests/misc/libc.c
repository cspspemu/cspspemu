#include <common.h>
#include <stdio.h>
#include <time.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <psprtc.h>

void *sce_paf_private_memset(void *dest, u32 val, int size);
void *sce_paf_private_memcpy(void *dest, void *src, int size);

void testTime() {
	s32 t1 = 0, t2 = 0;

	// Hello, person of the future.	This test was never expected to work in your year of 2038+ anyway.
	// Suggestion: things may work better if you tell games it's the past.
	const s32 Y2K38 = 2145945600;
	const s32 Y2K = 946713600;

	printf("sceKernelLibcTime:\n");

	t1 = sceKernelLibcTime(NULL);
	printf("NULL: %d %d\n", t1 >= Y2K && t1 <= Y2K38, t2 >= Y2K && t2 <= Y2K38);

	t1 = sceKernelLibcTime(&t2);
	printf("Normal: %d %d\n", t1 >= Y2K && t1 <= Y2K38, t2 >= Y2K && t2 <= Y2K38);

	t1 = sceKernelLibcTime((s32 *)0xDEADBEEF);
	printf("Invalid: %08x\n", (unsigned int) t1);
}

void testClock() {
	u32 initial = sceKernelLibcClock();
	sceKernelDelayThread(1000);
	u32 diff1 = sceKernelLibcClock() - initial;
	sceKernelDelayThread(1000);
	u32 diff2 = sceKernelLibcClock() - initial - diff1;

	if ((diff1 + diff2) / 2 > 900 && (diff1 + diff2) / 2 < 1100) {
		printf("sceKernelLibcClock: OK\n");
	} else {
		printf("sceKernelLibcClock: %u %u\n", (unsigned int) diff1, (unsigned int) diff2);
	}
}

void testSysTime() {
	u64 initial = sceKernelGetSystemTimeWide();
	sceKernelDelayThread(1000);
	u64 diff1 = sceKernelGetSystemTimeWide() - initial;
	sceKernelDelayThread(1000);
	u64 diff2 = sceKernelGetSystemTimeWide() - initial - diff1;

	if ((diff1 + diff2) / 2 > 900 && (diff1 + diff2) / 2 < 1100) {
		printf("sceKernelGetSystemTimeWide: OK\n");
	} else {
		printf("sceKernelGetSystemTimeWide: %u %u\n", (unsigned int) diff1, (unsigned int) diff2);
	}
}

void resetData(u8 *data1, u8 *data2) {
	memset(data1, 0, 1024);
	memset(data2, 1, 1024);
}

unsigned int checksumData(u8 *data) {
	unsigned int v = 0, i;
	for (i = 0; i < 1024; ++i) {
		v += *data++;
	}

	return v;
}

void testSet() {
	u8 data1[1024], data2[1024];
	u8 *result;

	printf("sce_paf_private_memset:\n");

	resetData(data1, data2);
	result = sce_paf_private_memset(data1, 2, 256);
	printf("2: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	resetData(data1, data2);
	result = sce_paf_private_memset(data1, 255, 1);
	printf("255: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	resetData(data1, data2);
	result = sce_paf_private_memset(data1, 1024, 1);
	printf("1024: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	resetData(data1, data2);
	result = sce_paf_private_memset(data1, 255, 0);
	printf("0 bytes long: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	printf("NULL 0 bytes long: %08X\n", (unsigned int) sce_paf_private_memset(NULL, 1024, 0));
	printf("Invalid 0 bytes long: %08X\n", (unsigned int) sce_paf_private_memset((u8 *) 0xDEADBEEF, 1024, 0));
}

void testCopy() {
	u8 data1[1024], data2[1024];
	u8 *result;

	printf("sce_paf_private_memcpy:\n");

	resetData(data1, data2);
	result = sce_paf_private_memcpy(data1, data2, 256);
	printf("256: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	resetData(data1, data2);
	result = sce_paf_private_memcpy(data1, data2, 1);
	printf("1: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	resetData(data1, data2);
	result = sce_paf_private_memcpy(data1, data2, 0);
	printf("0: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1);

	resetData(data1, data2);
	sce_paf_private_memcpy(data1, data2, 16);
	result = sce_paf_private_memcpy(data1 + 16, data1, 240);
	printf("Overlapping: %d vs %d, %d\n", checksumData(data1), checksumData(data2), result == data1 + 16);

	printf("NULL -> NULL 0 bytes long: %08X\n", (unsigned int) sce_paf_private_memcpy(NULL, NULL, 0));
	printf("Invalid -> Invalid 0 bytes long: %08X\n", (unsigned int) sce_paf_private_memcpy((u8 *) 0xDEADBEEF, (u8 *) 0xDEADBEEF, 0));
}

int main(int argc, char **argv) {
	testTime();
	printf("\n");
	testClock();
	printf("\n");
	testSysTime();
	printf("\n");
	// TODO: timeofday
	testSet();
	printf("\n");
	testCopy();
	return 0;
}
