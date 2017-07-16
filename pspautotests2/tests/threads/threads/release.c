#include "shared.h"

int waitWithDelay(SceSize argc, void *argv) {
	// 10 seconds is forever in automated test years.
	int ret = sceKernelDelayThread(10000000);

	schedf("* waitWithDelay awake: %08x\n", ret);
	return ret;
}

int waitWithSleep(SceSize argc, void *argv) {
	int ret = sceKernelSleepThread();

	schedf("* waitWithSleep awake: %08x\n", ret);
	return ret;
}

int waitSomeSema(SceSize argc, void *argv) {
	SceUID sema = sceKernelCreateSema("example", 0, 0, 1, NULL);
	int ret = sceKernelWaitSema(sema, 1, NULL);
	sceKernelDeleteSema(sema);

	schedf("* waitSomeSema awake: %08x\n", ret);
	return ret;
}

int nothingFunc(SceSize argc, void *argv) {
	return 0;
}

void testRelease(const char *title, SceKernelThreadEntry func) {
	SceUID thread = sceKernelCreateThread("test", func, 0x1F, 0x500, 0, NULL);
	sceKernelStartThread(thread, 0, NULL);

	int result = sceKernelReleaseWaitThread(thread);
	schedf("%s: %08x\n", title, result);

	SceUInt timeout = 500;
	sceKernelWaitThreadEnd(thread, &timeout);
	sceKernelTerminateDeleteThread(thread);
}

int main(int argc, char *argv[]) {
	testRelease("Delay", waitWithDelay);
	testRelease("Sleep", waitWithSleep);
	testRelease("Semaphore", waitSomeSema);
	testRelease("Not waiting", nothingFunc);

	schedf("Invalid: %08x\n", sceKernelReleaseWaitThread(0xDEADBEEF));
	schedf("Zero: %08x\n", sceKernelReleaseWaitThread(0));
	schedf("-1: %08x\n", sceKernelReleaseWaitThread(-1));
	schedf("Self: %08x\n", sceKernelReleaseWaitThread(sceKernelGetThreadId()));

	SceUID thread = sceKernelCreateThread("test", waitWithDelay, 0x1F, 0x500, 0, NULL);
	schedf("Not started: %08x\n", sceKernelReleaseWaitThread(thread));
	sceKernelDeleteThread(thread);
	schedf("Deleted: %08x\n", sceKernelReleaseWaitThread(thread));

	flushschedf();
	
	thread = sceKernelCreateThread("test", waitWithDelay, 0x1F, 0x500, 0, NULL);
	sceKernelStartThread(thread, 0, NULL);
	BASIC_SCHED_TEST("Normal",
		result = sceKernelReleaseWaitThread(thread);
	);
	sceKernelTerminateDeleteThread(thread);

	BASIC_SCHED_TEST("Zero",
		result = sceKernelReleaseWaitThread(0);
	);

	return 0;
}
