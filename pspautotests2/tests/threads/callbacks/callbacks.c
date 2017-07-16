#include <common.h>
#include <stdarg.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspumd.h>

int cb1, cb2, cb3;
int thread1, thread2, thread3;

int cbHandler(int unknown, int info, void *arg) {
	int thread = -1;
	if (sceKernelGetThreadId() == thread1)
		thread = 1;
	else if (sceKernelGetThreadId() == thread2)
		thread = 2;
	else if (sceKernelGetThreadId() == thread3)
		thread = 3;

	schedf("thread%d   cbHandler called: %08X, %08X, %08X\n", thread, (uint)unknown, (uint)info, (uint)arg);

	return 0;
}

int sleeperFunc(SceSize argc, void* argv) {
	thread2 = sceKernelGetThreadId();
	cb2 = sceKernelCreateCallback("cbHandler2", cbHandler, (void *)0x4567);
	sceKernelNotifyCallback(cb2, 0x22);
	sceKernelCheckCallback();
	sceKernelSleepThreadCB();
	schedf("thread2   -> awake\n");

	return 0;
}

int sleeperFunc3(SceSize argc, void* argv) {
	thread3 = sceKernelGetThreadId();
	cb3 = sceKernelCreateCallback("cbHandler3", cbHandler, (void *)0x8901);
	// Intentionally notifies cb2.
	sceKernelNotifyCallback(cb2, 0x23);
	sceKernelCheckCallback();
	sceKernelDelayThreadCB(100000);
	schedf("thread3   -> awake\n");

	return 0;
}

void attempt_sceKernelLockMutexCB(const char *name, SceUID mutex, int n, SceUInt *timeout) {
	// Notify each and see which ones go through.
	sceKernelNotifyCallback(cb1, 0x11);
	sceKernelNotifyCallback(cb2, 0x21);
	sceKernelNotifyCallback(cb3, 0x31);

	int result = sceKernelLockMutexCB(mutex, n, timeout);
	schedf("%s: %08X\n", name, result);
}

int main(int argc, char **argv) {
	thread1 = sceKernelGetThreadId();

	SceUID sleeperThread = sceKernelCreateThread("sleeperFunc", sleeperFunc, 0x20, 0x1000, 0, 0);
	sceKernelStartThread(sleeperThread, 0, 0);
	sceKernelDelayThread(10000);

	SceUID sleeperThread3 = sceKernelCreateThread("sleeperFunc3", sleeperFunc3, 0x70, 0x1000, 0, 0);
	sceKernelStartThread(sleeperThread3, 0, 0);
	sceKernelDelayThread(10000);

	schedf("thread1   -> awake\n");

	SceUID mutex = sceKernelCreateMutex("lock", 0, 0, 0);
	cb1 = sceKernelCreateCallback("cbHandler1", cbHandler, (void *)0x1234);

	attempt_sceKernelLockMutexCB("thread1   Lock 0 => 5", mutex, 5, NULL);
	attempt_sceKernelLockMutexCB("thread1   Lock 0 => 1", mutex, 1, NULL);
	attempt_sceKernelLockMutexCB("thread1   Lock 1 => 1", mutex, 1, NULL);

	schedf("thread1   Forcing a resched...\n");
	sceKernelNotifyCallback(cb2, 0x21);
	sceKernelDelayThread(10);
	schedf("thread1   Forcing a CB resched...\n");
	sceKernelDelayThreadCB(10);

	sceKernelNotifyCallback(cb2, 0x21);
	schedf("thread1   Check callbacks on thread1 with pending on thread2...\n");
	sceKernelCheckCallback();
	schedf("thread1   Forcing a CB resched...\n");
	sceKernelDelayThreadCB(10);

	SceUInt timeout = 1000000;
	sceKernelWakeupThread(sleeperThread);
	sceKernelWaitThreadEnd(sleeperThread, &timeout);
	sceKernelWaitThreadEnd(sleeperThread3, &timeout);

	return 0;
}
