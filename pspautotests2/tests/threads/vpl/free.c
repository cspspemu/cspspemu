#include "shared.h"

void testFree(const char *title, SceUID vpl, void *ptr) {
	int result = sceKernelFreeVpl(vpl, ptr);
	if (result == 0) {
		schedf("%s: OK ", title);
		schedfVpl(vpl);
	} else {
		schedf("%s: Failed (%08X) ", title, result);
		schedfVpl(vpl);
	}
}

void *testAlloc(SceUID vpl, int size) {
	void *data;
	int result = sceKernelTryAllocateVpl(vpl, size, &data);
	if (result != 0) {
		schedf("TEST FAILURE\n");
	}
	return data;
}

void testFragmentation() {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000 + 0x20, NULL);
	void *data[0x10];
	memset(data, 0, sizeof(data));

	sceKernelAllocateVpl(vpl, 0x1000 - 8, &data[0], NULL);
	sceKernelAllocateVpl(vpl, 0x1000 - 8, &data[1], NULL);
	sceKernelAllocateVpl(vpl, 0x1000 - 8, &data[2], NULL);

	sceKernelAllocateVpl(vpl, 0xA000 - 8, &data[3], NULL);

	sceKernelFreeVpl(vpl, data[0]);
	sceKernelFreeVpl(vpl, data[1]);
	sceKernelFreeVpl(vpl, data[2]);

	schedf("Fragmented:\n  ");
	schedfVpl(vpl);

	int result = sceKernelTryAllocateVpl(vpl, 0x1000 - 8, &data[4]);
	schedf("  Allocate small chunk: %08x top-%x\n", result, data[4] == 0 ? 0xFFFFFFFF : (char *) data[0] - (char *) data[4]);
	sceKernelFreeVpl(vpl, data[4]);

	result = sceKernelTryAllocateVpl(vpl, 0x4000 - 8, &data[5]);
	schedf("  Allocate large chunk: %08x top-%x\n", result, data[5] == 0 ? 0xFFFFFFFF : (char *) data[0] - (char *) data[5]);

	schedf("\n");
	sceKernelDeleteVpl(vpl);
}

int main(int argc, char **argv) {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	void *data;

	schedf("Basic alloc:\n");
	data = testAlloc(vpl, 0x20);
	testFree("  Normal", vpl, data);
	testFree("  Twice", vpl, data);
	testFree("  NULL", vpl, NULL);
	testFree("  Invalid", vpl, (void *) 0xDEADBEEF);
	testFree("  Stack", vpl, &data);

	schedf("\nOffset from alloc:\n");
	data = testAlloc(vpl, 0x20);
	// Crashes.
	//testFree("  Unaligned", vpl, (char *) data + 1);
	testFree("  Offset +", vpl, (char *) data + 4);
	testFree("  Offset -", vpl, (char *) data - 4);
	schedf("\n");

	SceUID vpl2 = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	data = testAlloc(vpl2, 0x20);
	testFree("Other VPL's ptr", vpl, data);
	sceKernelDeleteVpl(vpl);
	schedf("\n");

	schedf("Bad VPLs:\n");
	testFree("  NULL", 0, data);
	testFree("  Invalid", 0xDEADBEEF, data);
	testFree("  Deleted", vpl, data);
	testFree("  NULL + Invalid", 0, (void *) 0xDEADBEEF);
	schedf("\n");
	flushschedf();

	testFragmentation();

	BASIC_SCHED_TEST("NULL",
		result = sceKernelFreeVpl(vpl1, NULL);
	);
	BASIC_SCHED_TEST("Same",
		result = sceKernelFreeVpl(vpl1, data1);
	);
	BASIC_SCHED_TEST("Same not enough",
		void *data3;
		result = sceKernelTryAllocateVpl(vpl1, 0x20, &data3);
		result = sceKernelFreeVpl(vpl1, data3);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelFreeVpl(vpl2, data2);
	);

	flushschedf();
	return 0;
}