#include "shared.h"

void *testTryAlloc(const char *title, SceUID vpl, unsigned int size, int with_data) {
	void *data = (void *) 0xDEADBEEF;
	int result = sceKernelTryAllocateVpl(vpl, size, with_data ? &data : NULL);
	if (result == 0) {
		schedf("%s: OK ", title);
		schedfVpl(vpl);
	} else {
		schedf("%s: Failed (%08X) ", title, result);
		schedfVpl(vpl);
	}

	return data;
}

static int allocFunc(SceSize argSize, void* argPointer) {
	SceUInt timeout = 1000;
	void *data;
	schedulingResult = sceKernelAllocateVpl(*(SceUID *) argPointer, 0x20, &data, &timeout);
	schedf("L1 ");
	sceKernelDelayThread(1000);
	sceKernelFreeVpl(*(SceUID *) argPointer, data);
	return 0;
}

void testTryAllocThread(const char *title, u32 attr, u32 requestBytes, u32 initialBytes) {
	schedf("%s: ", title);

	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, attr, 0x100, NULL);

	// This way we have some allocated + free.
	void *data;
	sceKernelAllocateVpl(vpl, initialBytes, &data, NULL);

	SceUID allocThread = sceKernelCreateThread("allocThread", &allocFunc, 0x12, 0x1000, 0, NULL);
	sceKernelStartThread(allocThread, sizeof(SceUID), &vpl);
	sceKernelDelayThread(400);

	int result = sceKernelTryAllocateVpl(vpl, requestBytes, &data);
	schedf("L2 ");
	sceKernelDelayThread(600);

	sceKernelDeleteVpl(vpl);
	sceKernelWaitThreadEnd(allocThread, NULL);
	sceKernelTerminateDeleteThread(allocThread);

	if (result == 0) {
		schedf("OK (thread=%08X)\n", schedulingResult);
	} else {
		schedf("Failed (thread=%08X, main=%08X)\n", schedulingResult, result);
	}
}

static int deleteMeFunc(SceSize argSize, void* argPointer) {
	void *data;
	int result = sceKernelTryAllocateVpl(*(SceUID *) argPointer, 1, &data);
	printf("After delete: %08X\n", result);
	return 0;
}

void testDiff() {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	char *data1, *data2, *data3;
	sceKernelTryAllocateVpl(vpl, 0x08, (void **) &data1);
	sceKernelTryAllocateVpl(vpl, 0x10, (void **) &data2);
	sceKernelTryAllocateVpl(vpl, 0x01, (void **) &data3);

	schedf("Three 0x08s: diff=%x, %x\n", data1 - data2, data2 - data3);
	flushschedf();

	sceKernelDeleteVpl(vpl);
}

int main(int argc, char **argv) {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	void *data;

	testTryAlloc("More than free", vpl, 0xFFE1, 1);
	testTryAlloc("0 bytes", vpl, 0, 1);
	testTryAlloc("1 byte", vpl, 1, 1);
	testTryAlloc("-1 bytes", vpl, -1, 1);
	testTryAlloc("16 bytes", vpl, 16, 1);
	testTryAlloc("8 bytes", vpl, 8, 1);

	// Crashes.
	//testTryAlloc("Into NULL", vpl, 8, 0);

	testTryAlloc("Most remaining", vpl, 0xFF00, 1);
	testTryAlloc("More than remaining", vpl, 0xA1, 1);
	testTryAlloc("All remaining", vpl, 0xA0, 1);
	testTryAlloc("All remaining - 7", vpl, 0xA0 - 7, 1);
	testTryAlloc("All remaining - 8", vpl, 0xA0 - 8, 1);
	testTryAlloc("1 byte (none left)", vpl, 1, 1);

	sceKernelDeleteVpl(vpl);

	testTryAlloc("NULL", 0, 0x100, 1);
	testTryAlloc("NULL with invalid", 0, 0, 0);
	testTryAlloc("Invalid", 0xDEADBEEF, 0x100, 1);
	testTryAlloc("Deleted", vpl, 0x100, 1);
	flushschedf();

	testDiff();

	u32 attrs[] = {PSP_VPL_ATTR_FIFO, PSP_VPL_ATTR_PRIORITY, PSP_VPL_ATTR_SMALLEST, PSP_VPL_ATTR_HIGHMEM};
	int i;
	for (i = 0; i < sizeof(attrs) / sizeof(attrs[0]); ++i) {
		schedf("Attr %x:\n", attrs[i]);
		testTryAllocThread("  Alloc 0x20 of 0x00/0xE0", attrs[i], 0x20, 0xE0 - 0x08);
		testTryAllocThread("  Alloc 0x20 of 0x20/0xE0", attrs[i], 0x20, 0xE0 - 0x08 - (0x20 + 0x08));
		testTryAllocThread("  Alloc 0x20 of 0x40/0xE0", attrs[i], 0x20, 0xE0 - 0x08 - (0x20 + 0x08) * 2);
		schedf("\n");
	}

	BASIC_SCHED_TEST("Zero",
		result = sceKernelTryAllocateVpl(vpl1, 0, &data);
	);
	BASIC_SCHED_TEST("Alloc same",
		result = sceKernelTryAllocateVpl(vpl1, 0x6000, &data);
	);
	BASIC_SCHED_TEST("Alloc other",
		result = sceKernelTryAllocateVpl(vpl2, 0x6000, &data);
	);

	flushschedf();
	return 0;
}