#include "shared.h"
#include <psprtc.h>

int fiveFunc(SceSize argc, void *argv) {
	checkpoint("  * fiveFunc");
	return 5;
}

int delayFunc(SceSize argc, void *argv) {
	checkpoint("  * delayFunc");

	sceKernelDelayThread(500);

	return 7;
}

int neverFunc(SceSize argc, void *argv) {
	sceKernelSleepThread();
	return 8;
}

int suicideFunc(SceSize argc, void *argv) {
	sceKernelExitDeleteThread(9);
	return 9;
}

int deleteFunc(SceSize argc, void *argv) {
	sceKernelDelayThread(100);
	int result = sceKernelTerminateDeleteThread(*(SceUID *) argv);
	checkpoint("  * Delete thread: %08x", result);
	return 1;
}

int cbFunc(int arg1, int arg2, void *arg3) {
	checkpoint("  * cbFunc");
	return 0;
}

int waitEndFunc(SceSize argc, void *argv) {
	int result = sceKernelWaitThreadEnd(*(SceUID *) argv, NULL);
	u32 flag = sceKernelCpuSuspendIntr();
	checkpoint("  * waitEndFunc: %08x", result);
	sceKernelCpuResumeIntr(flag);
	return 0;
}

void testWaitEnd(const char *title, SceUID thread, SceUInt timeout) {
	int result = sceKernelWaitThreadEnd(thread, timeout == -1 ? NULL : &timeout);
	checkpoint("%s: %08x, timeout: %d", title, result, timeout);
}

void testWaitEndCB(const char *title, SceUID thread, SceUInt timeout) {
	int result = sceKernelWaitThreadEndCB(thread, timeout == -1 ? NULL : &timeout);
	checkpoint("%s: %08x, timeout: %d", title, result, timeout);
}

int main(int argc, char **argv) {
	SceUID cb = sceKernelCreateCallback("cbFunc", cbFunc, (void *) 0x1234);
	SceUID fiveThread = sceKernelCreateThread("fiveThread", fiveFunc, 0x1F, 0x1000, 0, 0);
	SceUID delayThread = sceKernelCreateThread("delayThread", delayFunc, 0x1F, 0x1000, 0, 0);
	SceUID neverThread = sceKernelCreateThread("neverThread", neverFunc, 0x1F, 0x1000, 0, 0);
	SceUID deleteThread = sceKernelCreateThread("deleteThread", deleteFunc, 0x1F, 0x1000, 0, 0);
	SceUID suicideThread = sceKernelCreateThread("suicideThread", suicideFunc, 0x1F, 0x1000, 0, 0);
	SceUID suspendedThread = sceKernelCreateThread("suspended", fiveFunc, 0x30, 0x1000, 0, NULL);
	sceKernelStartThread(suspendedThread, 4, &argc);
	sceKernelSuspendThread(suspendedThread);

	checkpointNext("Statuses:");
	testWaitEnd("  Not started", fiveThread, 500);
	testWaitEnd("  Suspended", suspendedThread, 500);

	checkpointNext("Twice:");
	sceKernelStartThread(fiveThread, 0, NULL);
	testWaitEnd("  Already ended", fiveThread, 500);
	testWaitEnd("  Already waited", fiveThread, 500);

	sceKernelStartThread(delayThread, 0, NULL);
	sceKernelTerminateThread(delayThread);
	testWaitEnd("  Terminated thread", delayThread, 500);

	checkpointNext("Status change:");
	SceKernelThreadInfo info;
	info.size = sizeof(info);
	sceKernelReferThreadStatus(neverThread, &info);
	checkpoint("  * before start exit=%08x, status=%08x", info.exitStatus, info.status);
	sceKernelStartThread(neverThread, 0, NULL);
	sceKernelReferThreadStatus(neverThread, &info);
	checkpoint("  * after start exit=%08x, status=%08x", info.exitStatus, info.status);

	checkpointNext("Timeouts:");
	sceKernelStartThread(delayThread, 0, NULL);
	testWaitEnd("  Short timeout", delayThread, 100);

	sceKernelStartThread(neverThread, 0, NULL);
	testWaitEnd("  Never wakes", neverThread, 1000);

	sceKernelStartThread(deleteThread, 4, &neverThread);
	sceKernelStartThread(neverThread, 0, NULL);
	testWaitEnd("  Thread deleted", neverThread, -1);
	delayThread = sceKernelCreateThread("delayThread", delayFunc, 0x1F, 0x1000, 0, 0);

	checkpointNext("Callbacks:");
	sceKernelNotifyCallback(cb, 1);
	sceKernelStartThread(fiveThread, 0, NULL);
	testWaitEnd("  Non-CB", fiveThread, 500);
	testWaitEndCB("  With CB, already ended", fiveThread, 500);
	
	sceKernelNotifyCallback(cb, 1);
	sceKernelStartThread(delayThread, 0, NULL);
	testWaitEndCB("  With CB, short timeout", delayThread, 100);

	checkpointNext("Objects:");
	testWaitEnd("  Invalid", 0xDEADBEEF, 1000);
	testWaitEnd("  Zero", 0, 1000);
	testWaitEnd("  Self", sceKernelGetThreadId(), 1000);
	testWaitEnd("  -1", -1, 1000);

	sceKernelTerminateDeleteThread(delayThread);
	testWaitEnd("  Terminated/deleted", delayThread, 1000);
	sceKernelDeleteThread(delayThread);
	testWaitEnd("  Deleted", delayThread, 1000);
	sceKernelStartThread(suicideThread, 0, NULL);
	sceKernelDelayThread(500);
	testWaitEnd("  Exit deleted", delayThread, 1000);

	checkpointNext("Scheduling");
	BASIC_SCHED_TEST("Normal",
		result = sceKernelWaitThreadEnd(fiveThread, NULL);
	);
	BASIC_SCHED_TEST("Zero",
		result = sceKernelWaitThreadEnd(0, NULL);
	);

	return 0;
}