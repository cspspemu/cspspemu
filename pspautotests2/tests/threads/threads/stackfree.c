#include "shared.h"

const u32 PSP_THREAD_ATTR_LOW_STACK = 0x00400000;

static int simpleCheck;
int simpleThread(SceSize argc, void *argp) {
	simpleCheck = sceKernelCheckThreadStack();
	return sceKernelSleepThread();
}

static int ffCheck;
int ffThread(SceSize argc, void *argp) {
	int stuff[256];
	memset(stuff, 0xff, sizeof(stuff));
	ffCheck = sceKernelCheckThreadStack();
	return sceKernelSleepThread();
}

static int zeroCheck;
int zeroThread(SceSize argc, void *argp) {
	int stuff[256];
	memset(stuff, 0, sizeof(stuff));
	zeroCheck = sceKernelCheckThreadStack();
	return sceKernelSleepThread();
}

int testThread(SceSize argc, void *argp) {
	if (argc > 0) {
		checkpoint(" - testThread");
	}
	return 0;
}

void runTestThread(const char *title, int *check, SceKernelThreadEntry func, u32 attr) {
	SceUID thread = sceKernelCreateThread("test", func, 0x10, 0x1000, attr, NULL);
	sceKernelStartThread(thread, 0, NULL);
	
	checkpoint("  %s (free): %08x", title, sceKernelGetThreadStackFreeSize(thread));
	checkpoint("  %s (check): %08x", title, *check);

	sceKernelTerminateDeleteThread(thread);
}

void checkStackFreeSize(const char *title, SceUID threadID) {
	int result = sceKernelGetThreadStackFreeSize(threadID);
	if (result > 0x200) {
		// We don't need to match exactly, since syscalls write stuff to the stack and fudge it.
		checkpoint("%s: OK", title);
	} else {
		checkpoint("%s: %08x", title, result);
	}
}

int main(int argc, char *argv[]) {
	checkpointNext("Current thread:");
	checkStackFreeSize("  Free", 0);
	checkpoint("  Check: %s", sceKernelCheckThreadStack() > 0x00030000 ? "OK" : "Failed");

	checkpointNext("Stack values:");
	runTestThread("  No usage w/o stack fill", &simpleCheck, simpleThread, PSP_THREAD_ATTR_NO_FILLSTACK | PSP_THREAD_ATTR_LOW_STACK);
	runTestThread("  1k FFs w/o stack fill", &ffCheck, ffThread, PSP_THREAD_ATTR_NO_FILLSTACK | PSP_THREAD_ATTR_LOW_STACK);
	runTestThread("  1k 00s w/o stack fill", &zeroCheck, zeroThread, PSP_THREAD_ATTR_NO_FILLSTACK | PSP_THREAD_ATTR_LOW_STACK);
	runTestThread("  No usage", &simpleCheck, simpleThread, 0);
	runTestThread("  1k FFs", &ffCheck, ffThread, 0);
	runTestThread("  1k 00s", &zeroCheck, zeroThread, 0);

	SceUID currentThread = sceKernelGetThreadId();
	SceUID createdThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	SceUID readyThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(readyThread, 4, &readyThread);
	SceUID finishedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelStartThread(finishedThread, 0, NULL);
	SceUID deletedThread = sceKernelCreateThread("test", &testThread, 0x10, 0x800, 0, NULL);
	sceKernelDeleteThread(deletedThread);
	SceUID suspendedThread = sceKernelCreateThread("test", &testThread, 0x30, 0x800, 0, NULL);
	sceKernelStartThread(suspendedThread, 4, &readyThread);
	sceKernelSuspendThread(suspendedThread);

	checkpointNext("Threads:");
	checkStackFreeSize("  -1", -1);
	checkStackFreeSize("  Invalid", 0xDEADBEEF);
	checkStackFreeSize("  Created", createdThread);
	checkStackFreeSize("  Ready", readyThread);
	checkStackFreeSize("  Finished", finishedThread);
	checkStackFreeSize("  Deleted", deletedThread);
	checkStackFreeSize("  Suspended", suspendedThread);
	checkStackFreeSize(  "Current", sceKernelGetThreadId());

	return 0;
}