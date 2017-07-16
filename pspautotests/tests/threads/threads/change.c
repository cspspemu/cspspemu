#include "shared.h"

void schedfCurrentPriority(const char *title, SceUID threadID) {
	SceKernelThreadInfo info;
	info.size = sizeof(info);

	int result = sceKernelReferThreadStatus(threadID, &info);
	checkpoint(NULL);
	if (result >= 0) {
		schedf("%s: Current=%x, init=%x\n", title, info.currentPriority, info.initPriority);
	} else {
		schedf("%s: Invalid (%08X)\n", title, result);
	}
}

void schedfCurrentAttr(SceUID threadID) {
	SceKernelThreadInfo info;
	info.size = sizeof(info);

	int result = sceKernelReferThreadStatus(threadID, &info);
	if (result >= 0) {
		schedf(", attr=%x\n", info.attr);
	} else {
		schedf(", attr invalid (%08X)\n", result);
	}
}

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

	checkpointNext("sceKernelChangeThreadPriority threads:");
	checkpoint("  -1: %08x", sceKernelChangeThreadPriority(-1, 0x20));
	checkpoint("  Zero: %08x", sceKernelChangeThreadPriority(0, 0x20));
	checkpoint("  Invalid: %08x", sceKernelChangeThreadPriority(0xDEADBEEF, 0x20));
	checkpoint("  Created: %08x", sceKernelChangeThreadPriority(createdThread, 0x20));
	checkpoint("  Ready: %08x", sceKernelChangeThreadPriority(readyThread, 0x20));
	checkpoint("  Finished: %08x", sceKernelChangeThreadPriority(finishedThread, 0x20));
	checkpoint("  Deleted: %08x", sceKernelChangeThreadPriority(deletedThread, 0x20));
	checkpoint("  Suspended: %08x", sceKernelChangeThreadPriority(suspendedThread, 0x20));
	checkpoint("  Waiting: %08x", sceKernelChangeThreadPriority(waitingThread, 0x20));
	checkpoint("  Current: %08x", sceKernelChangeThreadPriority(sceKernelGetThreadId(), 0x20));
	checkpoint("  Current: %08x", sceKernelChangeThreadPriority(sceKernelGetThreadId(), 0x20));

	checkpointNext("sceKernelChangeThreadPriority priorities:");
	sceKernelStartThread(readyThread, 4, &argc);
	const static int prios[] = {-2, -1, 0, 1, 2, 4, 5, 6, 7, 8, 9, 0x17, 0x18, 0x2F, 0x30, 0x31, 0x50, 0x6E, 0x6F, 0x77, 0x78, 0x79};
	for (i = 0; i < sizeof(prios) / sizeof(prios[0]); ++i) {
		int result = sceKernelChangeThreadPriority(readyThread, prios[i]);
		checkpoint("  0x%02x priority: %08x", prios[i], result);
		if (result == 0) {
			sceKernelStartThread(readyThread, 4, &argc);
		}
	}

	checkpointNext("sceKernelChangeThreadPriority priority 0:");
	sceKernelChangeThreadPriority(0, 0x18);
	sceKernelStartThread(readyThread, 4, &argc);
	sceKernelChangeThreadPriority(readyThread, 0x21);
	schedfCurrentPriority("  Set to 0x21", readyThread);
	sceKernelChangeThreadPriority(readyThread, 0);
	schedfCurrentPriority("  Set to 0", readyThread);

	checkpointNext("sceKernelChangeThreadPriority restarted priorities:");
	sceKernelTerminateThread(readyThread);
	sceKernelStartThread(readyThread, 0, NULL);
	schedfCurrentPriority("  Before", readyThread);
	for (i = 0; i < sizeof(prios) / sizeof(prios[0]); ++i) {
		int result = sceKernelChangeThreadPriority(readyThread, prios[i]);
		checkpoint("  0x%02x priority: %08x", prios[i], result);
		if (result == 0) {
			sceKernelStartThread(readyThread, 0, NULL);
			schedfCurrentPriority("    After restart", readyThread);
		} else {
			schedfCurrentPriority("    After error", readyThread);
		}
	}

	checkpointNext("sceKernelChangeCurrentThreadAttr add:");
	const static int attrs[] = {0x0, 0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000, 0x8000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000};
	for (i = 0; i < sizeof(attrs) / sizeof(attrs[0]); ++i) {
		checkpoint(NULL);
		schedf("  %x: %08x", attrs[i], sceKernelChangeCurrentThreadAttr(0, attrs[i]));
		schedfCurrentAttr(currentThread);
	}

	checkpointNext("sceKernelChangeCurrentThreadAttr remove:");
	for (i = 0; i < sizeof(attrs) / sizeof(attrs[0]); ++i) {
		checkpoint(NULL);
		schedf("  %x: %08x", attrs[i], sceKernelChangeCurrentThreadAttr(attrs[i], 0));
		schedfCurrentAttr(currentThread);
	}

	return 0;
}