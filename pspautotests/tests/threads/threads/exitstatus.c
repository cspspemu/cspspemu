#include "shared.h"

int testThread(SceSize argc, void *argp) {
	if (argc > 0) {
		checkpoint(" - testThread");
	}
	return 0;
}

int waitThread(SceSize argc, void *argp) {
	sceKernelWaitThreadEnd(*(SceUID *)argp, NULL);
	return 0;
}

int main(int argc, char *argv[]) {
	int i;
	char temp[32];

	SceUID currentThread = sceKernelGetThreadId();
	SceUID createdThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	SceUID readyThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(readyThread, 4, &argc);
	SceUID finishedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(finishedThread, 0, NULL);
	SceUID deletedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelDeleteThread(deletedThread);
	SceUID waitingThread = sceKernelCreateThread("wait", &waitThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(waitingThread, sizeof(currentThread), &currentThread);
	SceUID suspendedThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(suspendedThread, 4, &argc);
	sceKernelSuspendThread(suspendedThread);

	checkpointNext("sceKernelGetThreadExitStatus threads:");
	checkpoint("  -1: %08x", sceKernelGetThreadExitStatus(-1));
	checkpoint("  Zero: %08x", sceKernelGetThreadExitStatus(0));
	checkpoint("  Invalid: %08x", sceKernelGetThreadExitStatus(0xDEADBEEF));
	checkpoint("  Created: %08x", sceKernelGetThreadExitStatus(createdThread));
	checkpoint("  Ready: %08x", sceKernelGetThreadExitStatus(readyThread));
	checkpoint("  Finished: %08x", sceKernelGetThreadExitStatus(finishedThread));
	checkpoint("  Deleted: %08x", sceKernelGetThreadExitStatus(deletedThread));
	checkpoint("  Suspended: %08x", sceKernelGetThreadExitStatus(suspendedThread));
	checkpoint("  Waiting: %08x", sceKernelGetThreadExitStatus(waitingThread));
	checkpoint("  Current: %08x", sceKernelGetThreadExitStatus(sceKernelGetThreadId()));

	return 0;
}