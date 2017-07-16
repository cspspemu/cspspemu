#include "shared.h"

inline void testCancel(const char *title, SceUID fpl, bool doWaiting = true) {
	int waiting = 1337;
	int result = sceKernelCancelFpl(fpl, doWaiting ? &waiting : NULL);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK (waiting=%d) ", title, waiting);
		schedfFpl(fpl);
	} else {
		checkpoint("%s: Failed (%08x, waiting=%d)", title, result, waiting);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID fpl = sceKernelCreateFpl("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, 1, NULL);

	checkpointNext("Objects:");
	testCancel("  Normal", fpl);
	testCancel("  NULL", 0);
	testCancel("  Invalid", 0xDEADBEEF);
	testCancel("  Already canceled", fpl);
	sceKernelDeleteFpl(fpl);
	testCancel("  Deleted", fpl);

	checkpointNext("Count:");
	fpl = sceKernelCreateFpl("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, 1, NULL);
	testCancel("  No thread count", fpl, false);
	sceKernelDeleteFpl(fpl);

	checkpointNext("Waits:");
	{
		fpl = sceKernelCreateFpl("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, 1, NULL);
		void *allocated = NULL;
		// Make sure there's nothing free so the threads wait.
		sceKernelAllocateFpl(fpl, &allocated, NULL);
		FplWaitThread wait1("waiting thread 1", fpl, NO_TIMEOUT);
		FplWaitThread wait2("waiting thread 2", fpl, 10000);
		testCancel("  With waiting threads", fpl);
		sceKernelDeleteFpl(fpl);
	}

	return 0;
}