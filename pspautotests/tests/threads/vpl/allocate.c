#include "shared.h"

void *testAlloc(const char *title, SceUID vpl, unsigned int size, int with_data) {
	void *data = (void *) 0xDEADBEEF;
	int result = sceKernelAllocateVpl(vpl, size, with_data ? &data : NULL, NULL);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK ", title);
		schedfVpl(vpl);
	} else {
		checkpoint(NULL);
		schedf("%s: Failed (%08X) ", title, result);
		schedfVpl(vpl);
	}

	return data;
}

void *testAllocTimeout(const char *title, SceUID vpl, unsigned int size, unsigned int timeout, int with_data) {
	void *data = (void *) 0xDEADBEEF;
	int result = sceKernelAllocateVpl(vpl, size, with_data ? &data : NULL, &timeout);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK (%dms left) ", title, timeout);
		schedfVpl(vpl);
	} else {
		checkpoint(NULL);
		schedf("%s: Failed (%08X, %dms left) ", title, result, timeout);
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

void testAllocThread(const char *title, u32 attr, u32 requestBytes, u32 initialBytes) {
	checkpoint(NULL);
	schedf("%s: ", title);

	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, attr, 0x100, NULL);

	// This way we have some allocated + free.
	void *data;
	sceKernelAllocateVpl(vpl, initialBytes, &data, NULL);

	SceUID allocThread = sceKernelCreateThread("allocThread", &allocFunc, 0x12, 0x1000, 0, NULL);
	sceKernelStartThread(allocThread, sizeof(SceUID), &vpl);
	sceKernelDelayThread(400);

	unsigned int timeout = 1000;
	int result = sceKernelAllocateVpl(vpl, requestBytes, &data, &timeout);
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
	int result = sceKernelAllocateVpl(*(SceUID *) argPointer, 1, &data, NULL);
	checkpoint("  After delete: %08X", result);
	return 0;
}

void testDiff() {
	checkpointNext("Allocation difference:");

	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	char *data1, *data2, *data3;
	sceKernelAllocateVpl(vpl, 0x08, (void **) &data1, NULL);
	sceKernelAllocateVpl(vpl, 0x10, (void **) &data2, NULL);
	sceKernelAllocateVpl(vpl, 0x01, (void **) &data3, NULL);

	checkpoint("  Three 0x08s: diff=%x, %x", data1 - data2, data2 - data3);

	sceKernelDeleteVpl(vpl);
}

int callbackFunc(int a1, int a2, void *a3) {
	checkpoint("  * callbackFunc: %d, %d, %08x", a1, a2, a3);
	return 0;
}

int main(int argc, char **argv) {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	void *data;

	checkpoint("No timeout:");
	testAlloc("  More than free", vpl, 0xFFE1, 1);
	testAlloc("  0 bytes", vpl, 0, 1);
	testAlloc("  1 byte", vpl, 1, 1);
	testAlloc("  -1 bytes", vpl, -1, 1);
	testAlloc("  16 bytes", vpl, 16, 1);
	testAlloc("  8 bytes", vpl, 8, 1);

	checkpointNext("With timeout:");
	testAllocTimeout("  More than free", vpl, 0xFFE1, 500, 1);
	testAllocTimeout("  0 bytes", vpl, 0, 500, 1);
	testAllocTimeout("  1 byte", vpl, 1, 500, 1);
	testAllocTimeout("  -1 bytes", vpl, -1, 500, 1);
	testAllocTimeout("  16 bytes", vpl, 16, 500, 1);
	testAllocTimeout("  8 bytes", vpl, 8, 500, 1);
	
	checkpointNext("Errors:");
	// Crashes.
	//testAllocTimeout("Into NULL", vpl, 8, 500, 0);
	testAllocTimeout("  Into NULL", vpl, 0xFFE0, 500, 0);

	testAllocTimeout("  Most remaining", vpl, 0xFF00, 500, 1);
	testAllocTimeout("  More than remaining", vpl, 0xA1, 500, 1);
	testAllocTimeout("  All remaining", vpl, 0x68, 500, 1);
	testAllocTimeout("  All remaining - 7", vpl, 0x68 - 7, 500, 1);
	testAllocTimeout("  All remaining - 8", vpl, 0x68 - 8, 500, 1);
	testAllocTimeout("  1 byte (none left)", vpl, 1, 500, 1);
	testAllocTimeout("  1 byte (none left, 0 timeout)", vpl, 1, 0, 1);

	sceKernelDeleteVpl(vpl);

	checkpointNext("Objects:");
	testAllocTimeout("  NULL", 0, 0x100, 500, 1);
	testAllocTimeout("  NULL with invalid", 0, 0, 500, 0);
	testAllocTimeout("  Invalid", 0xDEADBEEF, 0x100, 500, 1);
	testAllocTimeout("  Deleted", vpl, 0x100, 500, 1);

	testDiff();

	u32 attrs[] = {PSP_VPL_ATTR_FIFO, PSP_VPL_ATTR_PRIORITY, PSP_VPL_ATTR_SMALLEST, PSP_VPL_ATTR_HIGHMEM};
	int i;
	for (i = 0; i < sizeof(attrs) / sizeof(attrs[0]); ++i) {
		char temp[64];
		snprintf(temp, sizeof(temp), "Attr %x:", attrs[i]);
		checkpointNext(temp);
		testAllocThread("  Alloc 0x10 of 0x00/0xE0", attrs[i], 0x20, 0xE0 - 0x08);
		testAllocThread("  Alloc 0x10 of 0x20/0xE0", attrs[i], 0x20, 0xE0 - 0x08 - (0x20 + 0x08));
		testAllocThread("  Alloc 0x10 of 0x40/0xE0", attrs[i], 0x20, 0xE0 - 0x08 - (0x20 + 0x08) * 2);
	}

	checkpointNext("Delete while waiting:");
	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	sceKernelAllocateVpl(vpl, 0x10000 - 0x20 - 0x08, &data, NULL);
	sceKernelStartThread(deleteThread, sizeof(SceUID), &vpl);
	sceKernelDeleteVpl(vpl);

	checkpointNext("Scheduling:");
	unsigned int timeout;
	BASIC_SCHED_TEST("Zero",
		timeout = 500;
		result = sceKernelAllocateVpl(vpl1, 0, &data, &timeout);
	);
	BASIC_SCHED_TEST("Alloc same",
		timeout = 500;
		result = sceKernelAllocateVpl(vpl1, 0x6000, &data, &timeout);
	);
	BASIC_SCHED_TEST("Alloc other",
		timeout = 500;
		result = sceKernelAllocateVpl(vpl2, 0x6000, &data, &timeout);
	);

	checkpointNext("Callbacks:");

	SceUID cb = sceKernelCreateCallback("cb", callbackFunc, 0);
	vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);

	sceKernelNotifyCallback(cb, 1);
	checkpoint(NULL);
	schedf("  Invalid: ");
	timeout = 500;
	sceKernelAllocateVplCB(0, 0x10000 - 0x20 - 0x01, &data, &timeout);
	schedf("\n");

	sceKernelNotifyCallback(cb, 1);
	checkpoint(NULL);
	schedf("  Successful: ");
	sceKernelAllocateVplCB(vpl, 1, &data, NULL);
	schedf("\n");

	sceKernelNotifyCallback(cb, 1);
	checkpoint(NULL);
	schedf("  More than available: ");
	sceKernelAllocateVplCB(vpl, 0x10000, &data, NULL);
	schedf("\n");

	sceKernelNotifyCallback(cb, 1);
	checkpoint(NULL);
	schedf("  Unsuccessful: ");
	timeout = 500;
	sceKernelAllocateVplCB(vpl, 0x10000 - 0x20 - 0x01, &data, &timeout);

	/*checkpointNext("Timeouts:");
	unsigned int timeouts[] = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 30, 100, 210, 211, 215, 216, 217, 218, 219, 220};
	for (i = 0; i < sizeof(timeouts) / sizeof(timeouts[0]); ++i) {
		s64 t1 = sceKernelGetSystemTimeWide();
		timeout = timeouts[i];
		sceKernelAllocateVpl(vpl, 0x10000 - 0x20 - 0x01, &data, &timeout);
		s64 t2 = sceKernelGetSystemTimeWide();
		schedf("  %d = %lld\n", timeouts[i], t2 - t1);
	}*/

	sceKernelDeleteVpl(vpl);
	sceKernelDeleteCallback(cb);

	return 0;
}