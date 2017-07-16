#include "shared.h"

inline void testAllocate(const char *title, SceUID fpl, void **data, SceUInt timeout = 1000) {
	int result = sceKernelAllocateFpl(fpl, data, timeout == NO_TIMEOUT ? NULL : &timeout);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK (timeout=%dus) ", title, timeout == NO_TIMEOUT ? 0 : timeout / 1000);
		schedfFpl(fpl);
	} else {
		checkpoint(NULL);
		schedf("%s: Failed (%08x, timeout=%dus) ", title, result, timeout == NO_TIMEOUT ? 0 : timeout / 1000);
		schedfFpl(fpl);
	}
}

inline void drainFpl(SceUID fpl) {
	void *data;
	while (sceKernelTryAllocateFpl(fpl, &data) == 0) {
		continue;
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, NULL);
	void *temp;
	void *base;

	checkpointNext("Objects:");
	testAllocate("  Normal", fpl, &temp, 0);
	testAllocate("  NULL", 0, &temp, 0);
	testAllocate("  Invalid", 0xDEADBEEF, &temp, 0);
	sceKernelDeleteFpl(fpl);
	testAllocate("  Deleted", fpl, &temp, 0);

	fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, NULL);
	//checkpointNext("Pointers:");
	// Crashes.
	//testAllocate("  NULL", fpl, NULL, 0);
	sceKernelDeleteFpl(fpl);

	fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, NULL);
	checkpointNext("Timeout:");
	testAllocate("  No timeout", fpl, &temp, NO_TIMEOUT);
	testAllocate("  1000 with free", fpl, &temp, 1000);
	drainFpl(fpl);
	testAllocate("  1000 without free", fpl, &temp, 1000);
	sceKernelDeleteFpl(fpl);

	fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, NULL);
	checkpointNext("Allocation order (lowmem):");
	testAllocate("  Base #1", fpl, &base);
	testAllocate("  Alloc #2", fpl, &temp);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #1", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #1", -((char *)temp - (char *)base));
	}
	checkpoint("  Free #1: %08x", sceKernelFreeFpl(fpl, base));
	testAllocate("  Alloc #3", fpl, &base);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #3", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #3", -((char *)temp - (char *)base));
	}
	checkpoint("  Free #3: %08x", sceKernelFreeFpl(fpl, base));
	testAllocate("  Alloc #4", fpl, &base);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #4", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #4", -((char *)temp - (char *)base));
	}
	sceKernelDeleteFpl(fpl);

	fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, PSP_FPL_ATTR_HIGHMEM, 0x10, 8, NULL);
	checkpointNext("Allocation order (highmem):");
	testAllocate("  Base #1", fpl, &base);
	testAllocate("  Alloc #2", fpl, &temp);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #1", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #1", -((char *)temp - (char *)base));
	}
	checkpoint("  Free #1: %08x", sceKernelFreeFpl(fpl, base));
	testAllocate("  Alloc #3", fpl, &base);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #3", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #3", -((char *)temp - (char *)base));
	}
	checkpoint("  Free #3: %08x", sceKernelFreeFpl(fpl, base));
	testAllocate("  Alloc #4", fpl, &base);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #4", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #4", -((char *)temp - (char *)base));
	}
	sceKernelDeleteFpl(fpl);

	SceKernelFplOptParam2 opt;
	opt.size = sizeof(opt);
	opt.alignment = 32;
	fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, (SceKernelFplOptParam *)&opt);
	checkpointNext("Different alignment:");
	testAllocate("  Base #1", fpl, &base);
	testAllocate("  Alloc #2", fpl, &temp);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #1", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #1", -((char *)temp - (char *)base));
	}
	checkpoint("  Free #1: %08x", sceKernelFreeFpl(fpl, base));
	testAllocate("  Alloc #3", fpl, &base);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #3", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #3", -((char *)temp - (char *)base));
	}
	checkpoint("  Free #3: %08x", sceKernelFreeFpl(fpl, base));
	testAllocate("  Alloc #4", fpl, &base);
	if (temp > base) {
		checkpoint("  Alloc #2 is %d bytes after #4", (char *)temp - (char *)base);
	} else {
		checkpoint("  Alloc #2 is %d bytes before #4", -((char *)temp - (char *)base));
	}
	sceKernelDeleteFpl(fpl);

	checkpointNext("With two waiting:");
	{
		fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 1, NULL);
		testAllocate("  With waiting threads", fpl, &temp);
		schedfFpl(fpl);
		FplWaitThread wait1("waiting thread 1", fpl, 10000);
		FplWaitThread wait2("waiting thread 2", fpl, 10000);
		checkpoint("  Freed: %08x", sceKernelFreeFpl(fpl, temp));
		sceKernelDeleteFpl(fpl);
	}

	return 0;
}