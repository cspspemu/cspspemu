#include "shared.h"

void testCancel(const char *title, SceUID vpl, int withWait) {
	int waitThreads = -1;
	int result = sceKernelCancelVpl(vpl, withWait ? &waitThreads : NULL);
	if (result == 0) {
		schedf("%s (%d waiting): OK ", title, waitThreads);
	} else {
		schedf("%s (%d waiting): Failed (%X) ", title, waitThreads, result);
	}
	schedfVpl(vpl);
}

int main(int argc, char **argv) {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	void *data;

	testCancel("Unallocated", vpl, 0);
	sceKernelAllocateVpl(vpl, 0x100, &data, NULL);
	testCancel("With allocation", vpl, 0);

	testCancel("With allocation", vpl, 1);

	sceKernelDeleteVpl(vpl);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelCancelVpl(0, NULL);
	);
	BASIC_SCHED_TEST("Cancel other",
		result = sceKernelCancelVpl(vpl2, NULL);
	);
	BASIC_SCHED_TEST("Cancel same",
		result = sceKernelCancelVpl(vpl1, NULL);
	);
	BASIC_SCHED_TEST("Cancel other with threads",
		sceKernelCancelVpl(vpl2, &result);
	);
	BASIC_SCHED_TEST("Cancel same with threads",
		sceKernelCancelVpl(vpl1, &result);
	);

	testCancel("NULL", 0, 0);
	testCancel("Invalid", 0xDEADBEEF, 0);
	testCancel("Deleted", vpl, 0);

	flushschedf();
	return 0;
}