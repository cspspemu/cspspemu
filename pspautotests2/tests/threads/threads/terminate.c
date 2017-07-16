#include "shared.h"

int testThread(SceSize argc, void *argp) {
	if (argc > 0) {
		checkpoint(" - testThread");
	}
	return 0;
}

int waitThread(SceSize argc, void *argp) {
	int result = sceKernelWaitThreadEnd(*(SceUID *)argp, NULL);
	checkpoint(" - waitThread woke up with %08x", result);
	return 0;
}

void testTerminate(const char *title, int termFunc(SceUID)) {
	checkpointNext(title);

	SceUID currentThread = sceKernelGetThreadId();
	SceUID createdThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	SceUID readyThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(readyThread, 4, &readyThread);
	SceUID finishedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(finishedThread, 0, NULL);
	SceUID deletedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelDeleteThread(deletedThread);
	SceUID waitingThread = sceKernelCreateThread("wait", &waitThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(waitingThread, sizeof(currentThread), &currentThread);
	SceUID suspendedThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(suspendedThread, 4, &readyThread);
	sceKernelSuspendThread(suspendedThread);

	SceUID waitingThread2 = sceKernelCreateThread("wait2", &waitThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(waitingThread2, sizeof(suspendedThread), &suspendedThread);

	checkpoint("  -1: %08x", termFunc(-1));
	checkpoint("  Zero: %08x", termFunc(0));
	checkpoint("  Invalid: %08x", termFunc(0xDEADBEEF));
	checkpoint("  Created: %08x", termFunc(createdThread));
	checkpoint("  Ready: %08x", termFunc(readyThread));
	checkpoint("  Finished: %08x", termFunc(finishedThread));
	checkpoint("  Deleted: %08x", termFunc(deletedThread));
	checkpoint("  Suspended: %08x", termFunc(suspendedThread));
	checkpoint("  Waiting: %08x", termFunc(waitingThread));
	checkpoint("  Current: %08x", termFunc(sceKernelGetThreadId()));

	sceKernelDeleteThread(readyThread);
	sceKernelDeleteThread(finishedThread);
	sceKernelDeleteThread(waitingThread);

	checkpoint("  (giving waitingThread2 a chance to wake.)");
	sceKernelResumeThread(suspendedThread);
	checkpoint("  Resumed: %08x", termFunc(suspendedThread));
	sceKernelDelayThread(200);
	sceKernelDeleteThread(waitingThread2);
	sceKernelDeleteThread(suspendedThread);
}

int main(int argc, char *argv[]) {
	int i;
	char temp[32];

	testTerminate("sceKernelTerminateThread threads:", &sceKernelTerminateThread);
	testTerminate("sceKernelTerminateDeleteThread threads:", &sceKernelTerminateDeleteThread);
	testTerminate("sceKernelDeleteThread threads:", &sceKernelDeleteThread);

	return 0;
}