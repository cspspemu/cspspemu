#include <common.h>

int testThread(SceSize argc, void *argp) {
	if (argc > 0) {
		checkpoint(" - testThread");
	}
	return 0;
}

int main(int argc, char *argv[]) {
	SceUID createdThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	SceUID readyThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(readyThread, 4, &argc);
	SceUID finishedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(finishedThread, 0, NULL);
	SceUID deletedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelDeleteThread(deletedThread);

	checkpointNext("sceKernelSuspendThread:");
	checkpoint("  Zero: %08x", sceKernelSuspendThread(0));
	checkpoint("  Invalid: %08x", sceKernelSuspendThread(0xDEADBEEF));
	checkpoint("  Created: %08x", sceKernelSuspendThread(createdThread));
	checkpoint("  Ready: %08x", sceKernelSuspendThread(readyThread));
	checkpoint("  Finished: %08x", sceKernelSuspendThread(finishedThread));
	checkpoint("  Deleted: %08x", sceKernelSuspendThread(deletedThread));
	checkpoint("  Suspended: %08x", sceKernelSuspendThread(readyThread));
	checkpoint("  Current: %08x", sceKernelSuspendThread(sceKernelGetThreadId()));
	
	checkpointNext("sceKernelResumeThread:");
	// Reset to ready.
	sceKernelResumeThread(readyThread);

	checkpoint("  Zero: %08x", sceKernelResumeThread(0));
	checkpoint("  Invalid: %08x", sceKernelResumeThread(0xDEADBEEF));
	checkpoint("  Created: %08x", sceKernelResumeThread(createdThread));
	checkpoint("  Ready: %08x", sceKernelResumeThread(readyThread));
	checkpoint("  Finished: %08x", sceKernelResumeThread(finishedThread));
	checkpoint("  Deleted: %08x", sceKernelResumeThread(deletedThread));
	checkpoint("  Suspended: %08x", sceKernelResumeThread(readyThread));
	checkpoint("  Current: %08x", sceKernelResumeThread(sceKernelGetThreadId()));

	// Reset to suspended.
	sceKernelSuspendThread(readyThread);
	
	checkpointNext("Wait for wakeup:");
	checkpoint("  sceKernelDelayThread: %08x", sceKernelDelayThread(1000));
	SceUInt timeout = 5000;
	checkpoint("  sceKernelWaitThreadEnd: %08x", sceKernelWaitThreadEnd(readyThread, &timeout));
	checkpoint("  sceKernelResumeThread: %08x", sceKernelResumeThread(readyThread));
	checkpoint("  sceKernelSuspendThread again: %08x", sceKernelSuspendThread(readyThread));

	checkpointNext("Reset scenario:");
	checkpoint("  Terminate: %08x", sceKernelTerminateThread(readyThread));
	checkpoint("  Start again: %08x", sceKernelStartThread(readyThread, 4, &argc));
	checkpoint("  Delay: %08x", sceKernelDelayThread(1000));

	return 0;
}