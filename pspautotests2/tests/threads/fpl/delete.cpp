#include "shared.h"

inline void testDelete(const char *title, SceUID fpl) {
	int result = sceKernelDeleteFpl(fpl);
	if (result == 0) {
		checkpoint("%s: OK", title);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID fpl = sceKernelCreateFpl("delete", PSP_MEMORY_PARTITION_USER, 0, 0x100, 1, NULL);

	testDelete("Normal", fpl);
	testDelete("NULL", 0);
	testDelete("Invalid", 0xDEADBEEF);
	testDelete("Deleted", fpl);

	checkpointNext("Waits:");
	{
		fpl = sceKernelCreateFpl("delete", PSP_MEMORY_PARTITION_USER, 0, 0x100, 1, NULL);
		void *allocated = NULL;
		// Make sure there's nothing free so the threads wait.
		sceKernelAllocateFpl(fpl, &allocated, NULL);
		FplWaitThread wait1("waiting thread 1", fpl, NO_TIMEOUT);
		FplWaitThread wait2("waiting thread 2", fpl, 10000);
		checkpoint("With waiting threads: %08x", sceKernelDeleteFpl(fpl));
	}

	return 0;
}