#define sceKernelCpuResumeIntr sceKernelCpuResumeIntr_WRONG

#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspthreadman.h>

#undef sceKernelCpuResumeIntr
int sceKernelCpuResumeIntr(int state);

SceUID sceKernelCreateMutex(const char *name, uint attributes, int initial_count, void *options);
int sceKernelDeleteMutex(SceUID mutexId);
int sceKernelLockMutex(SceUID mutexId, int count, SceUInt *timeout);
int sceKernelLockMutexCB(SceUID mutexId, int count, SceUInt *timeout);
int sceKernelTryLockMutex(SceUID mutexId, int count);
int sceKernelUnlockMutex(SceUID mutexId, int count);

int sceKernelCreateLwMutex(void *workarea, const char *name, uint attr, int count, void *options);
int sceKernelDeleteLwMutex(void *workarea);
int sceKernelTryLockLwMutex(void *workarea, int count);
int sceKernelTryLockLwMutex_600(void *workarea, int count);
int sceKernelLockLwMutex(void *workarea, int count, SceUInt *timeout);
int sceKernelLockLwMutexCB(void *workarea, int count, SceUInt *timeout);
int sceKernelUnlockLwMutex(void *workarea, int count);

typedef struct {
	int count;
	SceUID thread;
	int attr;
	int numWaitThreads;
	SceUID uid;
	int pad[3];
} SceLwMutexWorkarea;

extern SceUID reschedThread;
extern volatile int didResched;
int ignoreResched = 0;

void dispatchCheckpoint(const char *format, ...) {
	int state = sceKernelSuspendDispatchThread();
	sceKernelResumeDispatchThread(state);

	schedf("[%s/%s] ", ignoreResched == 0 ? (didResched ? "r" : "x") : "?", state == 1 ? "y" : "n");

	if (ignoreResched == 0) {
		sceKernelTerminateThread(reschedThread);
	}

	va_list args;
	va_start(args, format);
	schedfBufferPos += vsprintf(schedfBuffer + schedfBufferPos, format, args);
	// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
	//vprintf(format, args);
	va_end(args);

	schedf("\n");
	didResched = 0;
	if (ignoreResched == 0) {
		sceKernelStartThread(reschedThread, 0, NULL);
	}
}

SceUID lockThread;

int lockThreadSema(SceSize argc, void *argp) {
	SceUID sema = *(SceUID *)argp;
	dispatchCheckpoint("T sceKernelWaitSema: %08x", sceKernelWaitSema(sema, 2, NULL));
	dispatchCheckpoint("T sceKernelDelayThread: %08x", sceKernelDelayThread(3000));
	dispatchCheckpoint("T sceKernelSignalSema: %08x", sceKernelSignalSema(sema, 1));
	return 0;
}

void startLockThreadSema(SceUID sema) {
	lockThread = sceKernelCreateThread("sema lock", &lockThreadSema, sceKernelGetThreadCurrentPriority() - 1, 0x1000, 0, NULL);
	dispatchCheckpoint("S sceKernelCreateThread: %08x", lockThread >= 0 ? 1 : lockThread);
	dispatchCheckpoint("S sceKernelStartThread: %08x", sceKernelStartThread(lockThread, 4, &sema));
}

void endLockThreadSema(SceUID sema) {
	SceUInt timeout = 10000;
	dispatchCheckpoint("E sceKernelWaitThreadEnd: %08x", sceKernelWaitThreadEnd(lockThread, &timeout));
	dispatchCheckpoint("E sceKernelTerminateDeleteThread: %08x", sceKernelTerminateDeleteThread(lockThread));
}

void checkSema(int doDispatch) {
	SceUID sema = sceKernelCreateSema("sema", 0, 0, 1, NULL);
	dispatchCheckpoint("sceKernelCreateSema: %08x", sema >= 0 ? 1 : sema);
	dispatchCheckpoint("sceKernelSignalSema: %08x", sceKernelSignalSema(sema, 1));
	dispatchCheckpoint("sceKernelWaitSema: %08x", sceKernelWaitSema(sema, 1, NULL));
	dispatchCheckpoint("sceKernelWaitSema too much: %08x", sceKernelWaitSema(sema, 9, NULL));
	dispatchCheckpoint("sceKernelDeleteSema: %08x", sceKernelDeleteSema(sema));
	sema = sceKernelCreateSema("test", 0, 1, 2, NULL);
	dispatchCheckpoint("sceKernelCreateSema: %08x", sema >= 0 ? 1 : sema);
	startLockThreadSema(sema);
	int state;
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	SceUInt timeout = 300;
	dispatchCheckpoint("sceKernelWaitSema: %08x", sceKernelWaitSema(sema, 1, &timeout));
	dispatchCheckpoint("sceKernelSignalSema: %08x", sceKernelSignalSema(sema, 1));
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}
	endLockThreadSema(sema);
	dispatchCheckpoint("sceKernelPollSema: %08x", sceKernelPollSema(sema, 1));
	dispatchCheckpoint("sceKernelDeleteSema: %08x", sceKernelDeleteSema(sema));
}

int lockThreadMutex(SceSize argc, void *argp) {
	SceUID mutex = *(SceUID *)argp;
	dispatchCheckpoint("T sceKernelLockMutex: %08x", sceKernelLockMutex(mutex, 2, NULL));
	dispatchCheckpoint("T sceKernelDelayThread: %08x", sceKernelDelayThread(3000));
	dispatchCheckpoint("T sceKernelUnlockMutex: %08x", sceKernelUnlockMutex(mutex, 1));
	return 0;
}

void startLockThreadMutex(SceUID mutex) {
	lockThread = sceKernelCreateThread("mutex lock", &lockThreadMutex, sceKernelGetThreadCurrentPriority() - 1, 0x1000, 0, NULL);
	dispatchCheckpoint("S sceKernelCreateThread: %08x", lockThread >= 0 ? 1 : lockThread);
	dispatchCheckpoint("S sceKernelStartThread: %08x", sceKernelStartThread(lockThread, 4, &mutex));
}

void endLockThreadMutex(SceUID mutex) {
	SceUInt timeout = 10000;
	dispatchCheckpoint("E sceKernelWaitThreadEnd: %08x", sceKernelWaitThreadEnd(lockThread, &timeout));
	dispatchCheckpoint("E sceKernelTerminateDeleteThread: %08x", sceKernelTerminateDeleteThread(lockThread));
}

void checkMutex(int doDispatch) {
	SceUID mutex = sceKernelCreateMutex("mutex", 0, 1, NULL);
	dispatchCheckpoint("sceKernelCreateMutex: %08x", mutex >= 0 ? 1 : mutex);
	dispatchCheckpoint("sceKernelUnlockMutex: %08x", sceKernelUnlockMutex(mutex, 1));
	dispatchCheckpoint("sceKernelLockMutex: %08x", sceKernelLockMutex(mutex, 1, NULL));
	dispatchCheckpoint("sceKernelLockMutex invalid: %08x", sceKernelLockMutex(mutex, -1, NULL));
	dispatchCheckpoint("sceKernelDeleteMutex: %08x", sceKernelDeleteMutex(mutex));
	mutex = sceKernelCreateMutex("test", 0, 1, NULL);
	dispatchCheckpoint("sceKernelCreateMutex: %08x", mutex >= 0 ? 1 : mutex);
	startLockThreadMutex(mutex);
	int state;
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	SceUInt timeout = 300;
	dispatchCheckpoint("sceKernelLockMutex: %08x", sceKernelLockMutex(mutex, 1, &timeout));
	dispatchCheckpoint("sceKernelUnlockMutex: %08x", sceKernelUnlockMutex(mutex, 1));
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}
	endLockThreadMutex(mutex);
	dispatchCheckpoint("sceKernelTryLockMutex: %08x", sceKernelTryLockMutex(mutex, 1));
	dispatchCheckpoint("sceKernelDeleteMutex: %08x", sceKernelDeleteMutex(mutex));
}

int lockThreadLwMutex(SceSize argc, void *argp) {
	void *mutex = *(void **)argp;
	dispatchCheckpoint("T sceKernelLockLwMutex: %08x", sceKernelLockLwMutex(mutex, 2, NULL));
	dispatchCheckpoint("T sceKernelDelayThread: %08x", sceKernelDelayThread(3000));
	dispatchCheckpoint("T sceKernelUnlockLwMutex: %08x", sceKernelUnlockLwMutex(mutex, 1));
	return 0;
}

void startLockThreadLwMutex(void *mutex) {
	lockThread = sceKernelCreateThread("lwmutex lock", &lockThreadLwMutex, sceKernelGetThreadCurrentPriority() - 1, 0x1000, 0, NULL);
	dispatchCheckpoint("S sceKernelCreateThread: %08x", lockThread >= 0 ? 1 : lockThread);
	dispatchCheckpoint("S sceKernelStartThread: %08x", sceKernelStartThread(lockThread, 4, &mutex));
}

void endLockThreadLwMutex(void *mutex) {
	SceUInt timeout = 10000;
	dispatchCheckpoint("E sceKernelWaitThreadEnd: %08x", sceKernelWaitThreadEnd(lockThread, &timeout));
	dispatchCheckpoint("E sceKernelTerminateDeleteThread: %08x", sceKernelTerminateDeleteThread(lockThread));
}

void checkLwMutex(int doDispatch) {
	SceLwMutexWorkarea workarea;
	dispatchCheckpoint("sceKernelCreateLwMutex: %08x", sceKernelCreateLwMutex(&workarea, "lwmutex", 0, 1, NULL));
	dispatchCheckpoint("sceKernelUnlockLwMutex: %08x", sceKernelUnlockLwMutex(&workarea, 1));
	dispatchCheckpoint("sceKernelLockLwMutex: %08x", sceKernelLockLwMutex(&workarea, 1, NULL));
	dispatchCheckpoint("sceKernelLockLwMutex invalid: %08x", sceKernelLockLwMutex(&workarea, -1, NULL));
	dispatchCheckpoint("sceKernelDeleteLwMutex: %08x", sceKernelDeleteLwMutex(&workarea));
	dispatchCheckpoint("sceKernelCreateLwMutex: %08x", sceKernelCreateLwMutex(&workarea, "lwmutex", 0, 1, NULL));
	startLockThreadLwMutex(&workarea);
	int state;
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	SceUInt timeout = 300;
	dispatchCheckpoint("sceKernelLockLwMutex: %08x", sceKernelLockLwMutex(&workarea, 1, &timeout));
	dispatchCheckpoint("sceKernelUnlockLwMutex: %08x", sceKernelUnlockLwMutex(&workarea, 1));
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}
	endLockThreadLwMutex(&workarea);
	dispatchCheckpoint("sceKernelTryLockLwMutex: %08x", sceKernelTryLockLwMutex_600(&workarea, 1));
	dispatchCheckpoint("sceKernelDeleteLwMutex: %08x", sceKernelDeleteLwMutex(&workarea));
}

int lockThreadEventFlag(SceSize argc, void *argp) {
	SceUID flag = *(SceUID *)argp;
	dispatchCheckpoint("T sceKernelWaitEventFlag: %08x", sceKernelWaitEventFlag(flag, 3, PSP_EVENT_WAITAND, NULL, NULL));
	dispatchCheckpoint("T sceKernelDelayThread: %08x", sceKernelDelayThread(3000));
	dispatchCheckpoint("T sceKernelClearEventFlag: %08x", sceKernelClearEventFlag(flag, 1));
	return 0;
}

void startLockThreadEventFlag(SceUID flag) {
	lockThread = sceKernelCreateThread("eventflag lock", &lockThreadEventFlag, sceKernelGetThreadCurrentPriority() - 1, 0x1000, 0, NULL);
	dispatchCheckpoint("S sceKernelCreateThread: %08x", lockThread >= 0 ? 1 : lockThread);
	dispatchCheckpoint("S sceKernelStartThread: %08x", sceKernelStartThread(lockThread, 4, &flag));
}

void endLockThreadEventFlag(SceUID flag) {
	SceUInt timeout = 10000;
	dispatchCheckpoint("E sceKernelWaitThreadEnd: %08x", sceKernelWaitThreadEnd(lockThread, &timeout));
	dispatchCheckpoint("E sceKernelTerminateDeleteThread: %08x", sceKernelTerminateDeleteThread(lockThread));
}

void checkEventFlag(int doDispatch) {
	SceUID flag = sceKernelCreateEventFlag("eventflag", 0, 0xFFFFFFFF, NULL);
	dispatchCheckpoint("sceKernelCreateEventFlag: %08x", flag >= 0 ? 1 : flag);
	dispatchCheckpoint("sceKernelClearEventFlag: %08x", sceKernelClearEventFlag(flag, 1));
	dispatchCheckpoint("sceKernelWaitEventFlag: %08x", sceKernelWaitEventFlag(flag, 1, PSP_EVENT_WAITAND, NULL, NULL));
	dispatchCheckpoint("sceKernelWaitEventFlag invalid: %08x", sceKernelWaitEventFlag(flag, 0, 0, NULL, NULL));
	dispatchCheckpoint("sceKernelDeleteEventFlag: %08x", sceKernelDeleteEventFlag(flag));
	flag = sceKernelCreateEventFlag("test", 0, 0xFFFFFFFF, NULL);
	dispatchCheckpoint("sceKernelCreateEventFlag: %08x", flag >= 0 ? 1 : flag);
	startLockThreadEventFlag(flag);
	int state;
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	SceUInt timeout = 300;
	dispatchCheckpoint("sceKernelWaitEventFlag: %08x", sceKernelWaitEventFlag(flag, 1, PSP_EVENT_WAITAND, NULL, &timeout));
	dispatchCheckpoint("sceKernelClearEventFlag: %08x", sceKernelClearEventFlag(flag, 1));
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}
	endLockThreadEventFlag(flag);
	dispatchCheckpoint("sceKernelPollEventFlag: %08x", sceKernelPollEventFlag(flag, 1, PSP_EVENT_WAITAND, NULL));
	dispatchCheckpoint("sceKernelDeleteEventFlag: %08x", sceKernelDeleteEventFlag(flag));
}

void checkIo(int doDispatch) {
	char temp[128];
	SceUID fd = sceIoOpen("dispatch.prx", PSP_O_RDONLY, 0777);
	dispatchCheckpoint("sceIoOpen: %08x", fd >= 0 ? 1 : fd);
	dispatchCheckpoint("sceIoRead: %08x", sceIoRead(fd, temp, sizeof(temp)));
	dispatchCheckpoint("sceIoClose: %08x", sceIoClose(fd));

	int state;
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	fd = sceIoOpen("dispatch.prx", PSP_O_RDONLY, 0777);
	dispatchCheckpoint("sceIoOpen: %08x", fd >= 0 ? 1 : fd);
	dispatchCheckpoint("sceIoRead: %08x", sceIoRead(fd, temp, sizeof(temp)));
	dispatchCheckpoint("sceIoClose: %08x", sceIoClose(fd));
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}

	SceInt64 res = -1;
	int result = -1;
	fd = sceIoOpenAsync("dispatch.prx", PSP_O_RDONLY, 0777);
	dispatchCheckpoint("sceIoOpenAsync: %08x", fd >= 0 ? 1 : fd);
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	result = sceIoPollAsync(fd, &res);
	dispatchCheckpoint("sceIoPollAsync: %08x / %016llx", result, res >= 0 ? 1LL : res);
	result = sceIoGetAsyncStat(fd, 1, &res);
	dispatchCheckpoint("sceIoGetAsyncStat: %08x / %016llx", result, res >= 0 ? 1LL : res);
	result = sceIoGetAsyncStat(fd, 0, &res);
	dispatchCheckpoint("sceIoGetAsyncStat: %08x / %016llx", result, res >= 0 ? 1LL : res);
	result = sceIoWaitAsync(fd, &res);
	dispatchCheckpoint("sceIoWaitAsync: %08x / %016llx", result, res >= 0 ? 1LL : res);
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}
	result = sceIoWaitAsync(fd, &res);
	dispatchCheckpoint("sceIoWaitAsync: %08x / %016llx", result, res >= 0 ? 1LL : res);
	if (doDispatch) {
		++ignoreResched;
		state = sceKernelSuspendDispatchThread();
		dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	}
	dispatchCheckpoint("sceIoRead: %08x", sceIoRead(fd, temp, sizeof(temp)));
	dispatchCheckpoint("sceIoWrite: %08x", sceIoWrite(1, "Hello.", sizeof("Hello.")));
	if (doDispatch) {
		dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
		--ignoreResched;
	}
	dispatchCheckpoint("sceIoCloseAsync: %08x", sceIoCloseAsync(fd));
	result = sceIoWaitAsync(fd, &res);
	dispatchCheckpoint("sceIoWaitAsync: %08x / %016llx", result, res);
}

void checkDispatchCases(const char *name, void (*testfunc)(int)) {
	int state;

	dispatchCheckpoint("%s without changes:", name);
	testfunc(0);
	flushschedf();
	
	didResched = 0;
	schedf("\n");
	dispatchCheckpoint("%s with short dispatch suspend:", name);
	testfunc(1);
	flushschedf();

	didResched = 0;
	schedf("\n");
	dispatchCheckpoint("%s while dispatch suspended:", name);
	// Starting a thread apparently resumes the dispatch thread.
	++ignoreResched;
	state = sceKernelSuspendDispatchThread();
	dispatchCheckpoint("sceKernelSuspendDispatchThread: %08x", state);
	testfunc(0);
	dispatchCheckpoint("sceKernelResumeDispatchThread: %08x", sceKernelResumeDispatchThread(state));
	--ignoreResched;
	flushschedf();

	didResched = 0;
	schedf("\n");
	dispatchCheckpoint("%s while intr suspended:", name);
	state = sceKernelCpuSuspendIntr();
	dispatchCheckpoint("sceKernelCpuSuspendIntr: %08x", state);
	testfunc(1);
	dispatchCheckpoint("sceKernelCpuResumeIntr: %08x", sceKernelCpuResumeIntr(state));
	flushschedf();
}

void vblankCallback(int no, void *value) {
	dispatchCheckpoint("vblankCallback");
}

void checkDispatchInterrupt() {
	dispatchCheckpoint("Interrupts while dispatch disabled:");

	sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 0, &vblankCallback, NULL);
	sceKernelEnableSubIntr(PSP_VBLANK_INT, 0);

	++ignoreResched;
	int state = sceKernelSuspendDispatchThread();

	int base = sceDisplayGetVcount();
	int i, j;
	for (i = 0; i < 1000; ++i) {
		if (sceDisplayGetVcount() > base + 3) {
			break;
		}
		for (j = 0; j < 10000; ++j)
			continue;
	}

	dispatchCheckpoint("vblanks=%d", sceDisplayGetVcount() - base);

	sceKernelResumeDispatchThread(state);
	--ignoreResched;

	base = sceDisplayGetVcount();
	for (i = 0; i < 1000; ++i) {
		if (sceDisplayGetVcount() > base + 3) {
			break;
		}
		for (j = 0; j < 10000; ++j)
			continue;
	}
	
	dispatchCheckpoint("vblanks=%d", sceDisplayGetVcount() - base);

	sceKernelDisableSubIntr(PSP_VBLANK_INT, 0);
	sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 0);
	flushschedf();
}

int main(int argc, char *argv[]) {
	reschedThread = sceKernelCreateThread("resched", &reschedFunc, sceKernelGetThreadCurrentPriority(), 0x1000, 0, NULL);

	checkDispatchCases("Semas", &checkSema);
	
	didResched = 0;
	schedf("\n\n");
	checkDispatchCases("Mutexes", &checkMutex);
	
	didResched = 0;
	schedf("\n\n");
	checkDispatchCases("LwMutexes", &checkLwMutex);
	
	didResched = 0;
	schedf("\n\n");
	checkDispatchCases("EventFlags", &checkEventFlag);
	
	didResched = 0;
	schedf("\n\n");
	checkDispatchCases("Io", &checkIo);
	
	didResched = 0;
	schedf("\n\n");
	checkDispatchInterrupt();

	return 0;
}