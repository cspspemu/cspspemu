#include <common.h>

#include <pspthreadman.h>
#include <pspintrman.h>
#include <pspdisplay.h>
#include <pspmodulemgr.h>
#include <pspaudio.h>
#include <pspge.h>
#include <pspiofilemgr.h>
#include <pspsuspend.h>
#include <pspumd.h>
#include <stdarg.h>

typedef struct SceKernelTlsOptParam {
	SceSize size;
	u32 alignment;
} SceKernelTlsOptParam;

extern "C" {
SceUID sceKernelCreateTls(const char *name, u32 partitionid, u32 attr, u32 blockSize, u32 count, SceKernelTlsOptParam *options);
int sceKernelDeleteTls(SceUID uid);
int sceKernelAllocateTls(SceUID uid);
int sceKernelFreeTls(SceUID uid);
int sceKernelReferTlsStatus(SceUID uid, void *info);

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

int sceDisplayWaitVblankStartMulti(int vblanks);
int sceDisplayWaitVblankStartMultiCB(int vblanks);
}

unsigned int __attribute__((aligned(16))) dlist[] = {
	0x00000000, // 0x00 NOP
	0x0F000000, // 0x0A FINISH
	0x0C000000, // 0x0B END
	0x00000000, // 0x00 NOP
	0x00000000, // 0x00 NOP
	0x0F000000, // 0x0E FINISH
	0x0C000000, // 0x0F END
	0x00000000, // 0x00 NOP
};

int sceKernelAllocateTlsHelper(SceUID uid) {
	int result = sceKernelAllocateTls(uid);
	if (result > 0) {
		return 0x1337;
	}
	return result;
}

extern SceUID reschedThread;
extern volatile int didResched;

extern u64 lastCheckpoint;
void safeCheckpoint(const char *format, ...) {
	int state = sceKernelSuspendDispatchThread();
	sceKernelResumeDispatchThread(state);

	const char *reschedState = didResched ? "?" : "x";
	if (didResched == 1) {
		reschedState = "r";
	}
	didResched = 2;

	u64 currentCheckpoint = sceKernelGetSystemTimeWide();
	if (CHECKPOINT_ENABLE_TIME) {
		schedf("[%s/%lld] ", reschedState, currentCheckpoint - lastCheckpoint);
	} else {
		schedf("[%s] ", reschedState);
	}

	if (state == 1) {
		sceKernelTerminateThread(reschedThread);
	}

	if (format != NULL) {
		va_list args;
		va_start(args, format);
		schedfBufferPos += vsprintf(schedfBuffer + schedfBufferPos, format, args);
		// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
		//vprintf(format, args);
		va_end(args);
	}

	if (state == 1) {
		didResched = 0;
		sceKernelStartThread(reschedThread, 0, NULL);
	}

	if (format != NULL) {
		schedf("\n");
	}

	lastCheckpoint = currentCheckpoint;
}

#define INTR_DISPATCH(expr) \
	{ \
		state = sceKernelCpuSuspendIntr(); \
		safeCheckpoint("  Interrupts disabled: %08x", expr); \
		if (sceKernelIsCpuIntrEnable()) { \
			safeCheckpoint("  Interrupts were re-enabled?"); \
		} \
		sceKernelCpuResumeIntr(state); \
		state = sceKernelSuspendDispatchThread(); \
		safeCheckpoint("  Dispatch disabled: %08x", expr); \
		int state2 = sceKernelSuspendDispatchThread(); \
		if (state2 == state) { \
			safeCheckpoint("  Dispatch was re-enabled?"); \
		} \
		sceKernelResumeDispatchThread(state); \
	}

#define INTR_DISPATCH_TITLE(title, expr) \
	{ \
		state = sceKernelCpuSuspendIntr(); \
		safeCheckpoint("  %s - interrupts disabled: %08x", title, expr); \
		if (sceKernelIsCpuIntrEnable()) { \
			safeCheckpoint("  Interrupts were re-enabled?"); \
		} \
		sceKernelCpuResumeIntr(state); \
		state = sceKernelSuspendDispatchThread(); \
		safeCheckpoint("  %s - dispatch disabled: %08x", title, expr); \
		int state2 = sceKernelSuspendDispatchThread(); \
		if (state2 == state) { \
			safeCheckpoint("  Dispatch was re-enabled?"); \
		} \
		sceKernelResumeDispatchThread(state); \
	}

bool intrRan = false;
SceUID intrSema;
SceUID intrFlag;
SceUID intrMbx;
SceUID intrFpl;
SceUID intrVpl;
SceUID intrTls;
SceUID intrMsgPipe;
SceUID intrMutex;
char intrLwMutex[256];
SceUID intrModule;
SceUID intrThread;
int intrDlist;
int intrFd;

extern "C" void interruptFunc(int no, void *arg) {
	void *ptr;
	char buf[1024];

	if (intrRan) {
		return;
	}
	intrRan = true;
	checkpoint("  ** Inside interrupt");
	
	SceKernelSysClock clock = {200};
	safeCheckpoint("  sceKernelDelayThread: %08x", sceKernelDelayThread(200));
	safeCheckpoint("  sceKernelDelayThreadCB: %08x", sceKernelDelayThreadCB(200));
	safeCheckpoint("  sceKernelDelaySysClockThread: %08x", sceKernelDelaySysClockThread(&clock));
	safeCheckpoint("  sceKernelDelaySysClockThreadCB: %08x", sceKernelDelaySysClockThreadCB(&clock));
	safeCheckpoint("  sceKernelSleepThread: %08x", sceKernelSleepThread());
	safeCheckpoint("  sceKernelSleepThreadCB: %08x", sceKernelSleepThreadCB());

	safeCheckpoint("  sceDisplayWaitVblank: %08x", sceDisplayWaitVblank());
	safeCheckpoint("  sceDisplayWaitVblankCB: %08x", sceDisplayWaitVblankCB());
	safeCheckpoint("  sceDisplayWaitVblankStart: %08x", sceDisplayWaitVblankStart());
	safeCheckpoint("  sceDisplayWaitVblankStartCB: %08x", sceDisplayWaitVblankStartCB());
	safeCheckpoint("  sceDisplayWaitVblankStartMulti - invalid count: %08x", sceDisplayWaitVblankStartMulti(0));
	safeCheckpoint("  sceDisplayWaitVblankStartMulti - valid count: %08x", sceDisplayWaitVblankStartMulti(1));
	safeCheckpoint("  sceDisplayWaitVblankStartMultiCB - invalid count: %08x", sceDisplayWaitVblankStartMultiCB(0));
	safeCheckpoint("  sceDisplayWaitVblankStartMultiCB - valid count: %08x", sceDisplayWaitVblankStartMultiCB(1));

	safeCheckpoint("  sceKernelWaitSema - bad sema: %08x", sceKernelWaitSema(0, 1, NULL));
	safeCheckpoint("  sceKernelWaitSema - invalid count: %08x", sceKernelWaitSema(intrSema, 9, NULL));
	safeCheckpoint("  sceKernelWaitSema - valid: %08x", sceKernelWaitSema(intrSema, 1, NULL));
	safeCheckpoint("  sceKernelWaitSemaCB - bad sema: %08x", sceKernelWaitSemaCB(0, 1, NULL));
	safeCheckpoint("  sceKernelWaitSemaCB - invalid count: %08x", sceKernelWaitSemaCB(intrSema, 9, NULL));
	safeCheckpoint("  sceKernelWaitSemaCB - valid: %08x", sceKernelWaitSemaCB(intrSema, 1, NULL));

	safeCheckpoint("  sceKernelWaitEventFlag - bad flag: %08x", sceKernelWaitEventFlag(0, 1, PSP_EVENT_WAITAND, NULL, NULL));
	safeCheckpoint("  sceKernelWaitEventFlag - invalid mode: %08x", sceKernelWaitEventFlag(intrFlag, 1, 0xFF, NULL, NULL));
	safeCheckpoint("  sceKernelWaitEventFlag - valid flag: %08x", sceKernelWaitEventFlag(intrFlag, 1, PSP_EVENT_WAITAND, NULL, NULL));
	safeCheckpoint("  sceKernelWaitEventFlagCB - bad flag: %08x", sceKernelWaitEventFlagCB(0, 1, PSP_EVENT_WAITAND, NULL, NULL));
	safeCheckpoint("  sceKernelWaitEventFlagCB - invalid mode: %08x", sceKernelWaitEventFlagCB(intrFlag, 1, 0xFF, NULL, NULL));
	safeCheckpoint("  sceKernelWaitEventFlagCB - valid flag: %08x", sceKernelWaitEventFlagCB(intrFlag, 1, PSP_EVENT_WAITAND, NULL, NULL));

	safeCheckpoint("  sceKernelReceiveMbx - bad mbx: %08x", sceKernelReceiveMbx(0, &ptr, NULL));
	safeCheckpoint("  sceKernelReceiveMbx - valid mbx: %08x", sceKernelReceiveMbx(intrMbx, &ptr, NULL));
	safeCheckpoint("  sceKernelReceiveMbxCB - bad mbx: %08x", sceKernelReceiveMbxCB(0, &ptr, NULL));
	safeCheckpoint("  sceKernelReceiveMbxCB - valid mbx: %08x", sceKernelReceiveMbxCB(intrMbx, &ptr, NULL));

	safeCheckpoint("  sceKernelAllocateFpl - bad fpl: %08x", sceKernelAllocateFpl(0, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateFpl - valid fpl: %08x", sceKernelAllocateFpl(intrFpl, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateFplCB - bad fpl: %08x", sceKernelAllocateFplCB(0, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateFplCB - valid fpl: %08x", sceKernelAllocateFplCB(intrFpl, &ptr, NULL));

	safeCheckpoint("  sceKernelAllocateVpl - bad vpl: %08x", sceKernelAllocateVpl(0, 0x10, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateVpl - bad size: %08x", sceKernelAllocateVpl(intrVpl, 0, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateVpl - valid vpl: %08x", sceKernelAllocateVpl(intrVpl, 0x10, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateVplCB - bad vpl: %08x", sceKernelAllocateVplCB(0, 0x10, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateVplCB - bad size: %08x", sceKernelAllocateVplCB(intrVpl, 0, &ptr, NULL));
	safeCheckpoint("  sceKernelAllocateVplCB - valid vpl: %08x", sceKernelAllocateVplCB(intrVpl, 0x10, &ptr, NULL));

	safeCheckpoint("  sceKernelAllocateTls - bad tls: %08x", sceKernelAllocateTlsHelper(0));
	safeCheckpoint("  sceKernelAllocateTls - valid tls: %08x", sceKernelAllocateTlsHelper(intrTls));

	safeCheckpoint("  sceKernelReceiveMsgPipe - bad msgpipe: %08x", sceKernelReceiveMsgPipe(0, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelReceiveMsgPipe - bad size: %08x", sceKernelReceiveMsgPipe(intrMsgPipe, buf, -1, 0, NULL, NULL));
	safeCheckpoint("  sceKernelReceiveMsgPipe - valid msgpipe: %08x", sceKernelReceiveMsgPipe(intrMsgPipe, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelReceiveMsgPipeCB - bad msgpipe: %08x", sceKernelReceiveMsgPipeCB(0, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelReceiveMsgPipeCB - bad size: %08x", sceKernelReceiveMsgPipeCB(intrMsgPipe, buf, -1, 0, NULL, NULL));
	safeCheckpoint("  sceKernelReceiveMsgPipeCB - valid msgpipe: %08x", sceKernelReceiveMsgPipeCB(intrMsgPipe, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelSendMsgPipe - bad msgpipe: %08x", sceKernelSendMsgPipe(0, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelSendMsgPipe - bad size: %08x", sceKernelSendMsgPipe(intrMsgPipe, buf, -1, 0, NULL, NULL));
	safeCheckpoint("  sceKernelSendMsgPipe - valid msgpipe: %08x", sceKernelSendMsgPipe(intrMsgPipe, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelSendMsgPipeCB - bad msgpipe: %08x", sceKernelSendMsgPipeCB(0, buf, 256, 0, NULL, NULL));
	safeCheckpoint("  sceKernelSendMsgPipeCB - bad size: %08x", sceKernelSendMsgPipeCB(intrMsgPipe, buf, -1, 0, NULL, NULL));
	safeCheckpoint("  sceKernelSendMsgPipeCB - valid msgpipe: %08x", sceKernelSendMsgPipeCB(intrMsgPipe, buf, 256, 0, NULL, NULL));

	safeCheckpoint("  sceKernelLockMutex - bad mutex: %08x", sceKernelLockMutex(0, 1, NULL));
	safeCheckpoint("  sceKernelLockMutex - bad count: %08x", sceKernelLockMutex(intrMutex, 9, NULL));
	safeCheckpoint("  sceKernelLockMutex - valid mutex: %08x", sceKernelLockMutex(intrMutex, 1, NULL));
	safeCheckpoint("  sceKernelLockMutexCB - bad mutex: %08x", sceKernelLockMutexCB(0, 1, NULL));
	safeCheckpoint("  sceKernelLockMutexCB - bad count: %08x", sceKernelLockMutexCB(intrMutex, 9, NULL));
	safeCheckpoint("  sceKernelLockMutexCB - valid mutex: %08x", sceKernelLockMutexCB(intrMutex, 1, NULL));

	safeCheckpoint("  sceKernelLockLwMutex - bad count: %08x", sceKernelLockLwMutex(intrLwMutex, 9, NULL));
	safeCheckpoint("  sceKernelLockLwMutex - valid mutex: %08x", sceKernelLockLwMutex(intrLwMutex, 1, NULL));
	safeCheckpoint("  sceKernelLockLwMutexCB - bad count: %08x", sceKernelLockLwMutexCB(intrLwMutex, 9, NULL));
	safeCheckpoint("  sceKernelLockLwMutexCB - valid mutex: %08x", sceKernelLockLwMutexCB(intrLwMutex, 1, NULL));

	safeCheckpoint("  sceKernelStartModule: %08x", sceKernelStartModule(sceKernelGetModuleId(), 0, NULL, NULL, NULL));
	safeCheckpoint("  sceKernelStopModule: %08x", sceKernelStopModule(sceKernelGetModuleId(), 0, NULL, NULL, NULL));

	safeCheckpoint("  sceKernelWaitThreadEnd - bad thread: %08x", sceKernelWaitThreadEnd(0, NULL));
	safeCheckpoint("  sceKernelWaitThreadEnd - not running: %08x", sceKernelWaitThreadEnd(intrThread, NULL));
	safeCheckpoint("  sceKernelWaitThreadEnd - running: %08x", sceKernelWaitThreadEnd(reschedThread, NULL));
	safeCheckpoint("  sceKernelWaitThreadEndCB - bad thread: %08x", sceKernelWaitThreadEndCB(0, NULL));
	safeCheckpoint("  sceKernelWaitThreadEndCB - not running: %08x", sceKernelWaitThreadEndCB(intrThread, NULL));
	safeCheckpoint("  sceKernelWaitThreadEndCB - running: %08x", sceKernelWaitThreadEndCB(reschedThread, NULL));

	SceCtrlData pad[64];
	sceCtrlReadBufferPositive(pad, 64);
	safeCheckpoint("  sceCtrlReadBufferPositive - bad count: %08x", sceCtrlReadBufferPositive(pad, 256));
	safeCheckpoint("  sceCtrlReadBufferPositive - valid: %08x", sceCtrlReadBufferPositive(pad, 64));

	safeCheckpoint("  sceAudioOutputBlocking - invalid channel: %08x", sceAudioOutputBlocking(1, 0, buf));
	safeCheckpoint("  sceAudioOutputBlocking - valid channel - 64: %08x", sceAudioOutputBlocking(0, 0, buf));
	safeCheckpoint("  sceAudioSRCOutputBlocking - valid channel - 64: %08x", sceAudioSRCOutputBlocking(0, buf));

	safeCheckpoint("  sceGeListSync - invalid list: %08x", sceGeListSync(9999, 0));
	safeCheckpoint("  sceGeListSync - mode 0: %08x", sceGeListSync(intrDlist, 0));
	safeCheckpoint("  sceGeListSync - mode 1: %08x", sceGeListSync(intrDlist, 1));
	safeCheckpoint("  sceGeListSync - mode 2: %08x", sceGeListSync(intrDlist, 2));

	safeCheckpoint("  sceGeDrawSync - mode 0: %08x", sceGeDrawSync(0));
	safeCheckpoint("  sceGeDrawSync - mode 1: %08x", sceGeDrawSync(1));
	safeCheckpoint("  sceGeDrawSync - mode 2: %08x", sceGeDrawSync(2));

	s64 iores;
	safeCheckpoint("  sceIoRead - bad file: %08x", sceIoRead(63, buf, 1));
	safeCheckpoint("  sceIoRead - valid: %08x", sceIoRead(intrFd, buf, 1));
	safeCheckpoint("  sceIoWrite - bad file: %08x", sceIoWrite(63, buf, 1));
	safeCheckpoint("  sceIoWrite - valid: %08x", sceIoWrite(intrFd, buf, 1));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoWaitAsync - bad file: %08x", sceIoWaitAsync(63, &iores));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoWaitAsync - valid: %08x", sceIoWaitAsync(intrFd, &iores));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoWaitAsyncCB - bad file: %08x", sceIoWaitAsyncCB(63, &iores));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoWaitAsyncCB - valid: %08x", sceIoWaitAsyncCB(intrFd, &iores));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoGetAsyncStat - bad file: %08x", sceIoGetAsyncStat(63, 0, &iores));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoGetAsyncStat - peek: %08x", sceIoGetAsyncStat(intrFd, 1, &iores));
	sceIoLseekAsync(intrFd, 0, PSP_SEEK_SET);
	safeCheckpoint("  sceIoGetAsyncStat - valid: %08x", sceIoGetAsyncStat(intrFd, 0, &iores));

	safeCheckpoint("  sceKernelVolatileMemLock: %08x", sceKernelVolatileMemLock(0, NULL, NULL));
	sceKernelVolatileMemUnlock(0);

	safeCheckpoint("  sceUmdWaitDriveStat - invalid type: %08x", sceUmdWaitDriveStat(0));
	safeCheckpoint("  sceUmdWaitDriveStat - valid type: %08x", sceUmdWaitDriveStat(0x20));
	safeCheckpoint("  sceUmdWaitDriveStatWithTimer - invalid type: %08x", sceUmdWaitDriveStatWithTimer(0, 100));
	safeCheckpoint("  sceUmdWaitDriveStatWithTimer - valid type: %08x", sceUmdWaitDriveStatWithTimer(0x20, 100));
	safeCheckpoint("  sceUmdWaitDriveStatCB - invalid type: %08x", sceUmdWaitDriveStatCB(0, 100));
	safeCheckpoint("  sceUmdWaitDriveStatCB - valid type: %08x", sceUmdWaitDriveStatCB(0x20, 100));
}

extern "C" int dummyThread(SceSize argc, void *argp) {
	return 0;
}

extern "C" int main(int argc, char *argv[]) {
	int state;
	void *ptr;
	char buf[1024];

	SceKernelSysClock clock = {200};
	checkpointNext("sceKernelDelayThread:");
	INTR_DISPATCH(sceKernelDelayThread(200));
	checkpointNext("sceKernelDelayThreadCB:");
	INTR_DISPATCH(sceKernelDelayThreadCB(200));
	checkpointNext("sceKernelDelaySysClockThread:");
	INTR_DISPATCH(sceKernelDelaySysClockThread(&clock));
	checkpointNext("sceKernelDelaySysClockThreadCB:");
	INTR_DISPATCH(sceKernelDelaySysClockThreadCB(&clock));
	checkpointNext("sceKernelSleepThread:");
	INTR_DISPATCH(sceKernelSleepThread());
	checkpointNext("sceKernelSleepThreadCB:");
	INTR_DISPATCH(sceKernelSleepThreadCB());

	checkpointNext("sceDisplayWaitVblank:");
	INTR_DISPATCH(sceDisplayWaitVblank());
	checkpointNext("sceDisplayWaitVblankCB:");
	INTR_DISPATCH(sceDisplayWaitVblankCB());
	checkpointNext("sceDisplayWaitVblankStart:");
	INTR_DISPATCH(sceDisplayWaitVblankStart());
	checkpointNext("sceDisplayWaitVblankStartCB:");
	INTR_DISPATCH(sceDisplayWaitVblankStartCB());
	checkpointNext("sceDisplayWaitVblankStartMulti:");
	INTR_DISPATCH_TITLE("Invalid count", sceDisplayWaitVblankStartMulti(0));
	INTR_DISPATCH_TITLE("Valid count", sceDisplayWaitVblankStartMulti(1));
	checkpointNext("sceDisplayWaitVblankStartMultiCB:");
	INTR_DISPATCH_TITLE("Invalid count", sceDisplayWaitVblankStartMultiCB(0));
	INTR_DISPATCH_TITLE("Valid count", sceDisplayWaitVblankStartMultiCB(1));

	SceUID sema = sceKernelCreateSema("sema", 0, 0, 1, NULL);
	checkpointNext("sceKernelWaitSema:");
	INTR_DISPATCH_TITLE("Bad sema", sceKernelWaitSema(0, 1, NULL));
	INTR_DISPATCH_TITLE("Invalid count", sceKernelWaitSema(sema, 9, NULL));
	INTR_DISPATCH_TITLE("Valid sema", sceKernelWaitSema(sema, 1, NULL));
	checkpointNext("sceKernelWaitSemaCB:");
	INTR_DISPATCH_TITLE("Bad sema", sceKernelWaitSemaCB(0, 1, NULL));
	INTR_DISPATCH_TITLE("Invalid count", sceKernelWaitSemaCB(sema, 9, NULL));
	INTR_DISPATCH_TITLE("Valid sema", sceKernelWaitSemaCB(sema, 1, NULL));
	sceKernelDeleteSema(sema);

	SceUID flag = sceKernelCreateEventFlag("flag", 0, 0, NULL);
	checkpointNext("sceKernelWaitEventFlag:");
	INTR_DISPATCH_TITLE("Bad flag", sceKernelWaitEventFlag(0, 1, PSP_EVENT_WAITAND, NULL, NULL));
	INTR_DISPATCH_TITLE("Invalid mode", sceKernelWaitEventFlag(flag, 1, 0xFF, NULL, NULL));
	INTR_DISPATCH_TITLE("Valid flag", sceKernelWaitEventFlag(flag, 1, PSP_EVENT_WAITAND, NULL, NULL));
	sceKernelSetEventFlag(flag, 1);
	INTR_DISPATCH_TITLE("Already set", sceKernelWaitEventFlag(flag, 1, PSP_EVENT_WAITAND, NULL, NULL));
	sceKernelDeleteEventFlag(flag);
	flag = sceKernelCreateEventFlag("flag", 0, 0, NULL);
	checkpointNext("sceKernelWaitEventFlagCB:");
	INTR_DISPATCH_TITLE("Bad flag", sceKernelWaitEventFlagCB(0, 1, PSP_EVENT_WAITAND, NULL, NULL));
	INTR_DISPATCH_TITLE("Invalid mode", sceKernelWaitEventFlagCB(flag, 1, 0xFF, NULL, NULL));
	INTR_DISPATCH_TITLE("Valid flag", sceKernelWaitEventFlagCB(flag, 1, PSP_EVENT_WAITAND, NULL, NULL));
	sceKernelSetEventFlag(flag, 1);
	INTR_DISPATCH_TITLE("Already set", sceKernelWaitEventFlagCB(flag, 1, PSP_EVENT_WAITAND, NULL, NULL));
	sceKernelDeleteEventFlag(flag);

	SceUID mbx = sceKernelCreateMbx("mbx", 0, NULL);
	checkpointNext("sceKernelReceiveMbx:");
	INTR_DISPATCH_TITLE("Bad mbx", sceKernelReceiveMbx(0, &ptr, NULL));
	INTR_DISPATCH_TITLE("Valid mbx", sceKernelReceiveMbx(mbx, &ptr, NULL));
	checkpointNext("sceKernelReceiveMbxCB:");
	INTR_DISPATCH_TITLE("Bad mbx", sceKernelReceiveMbxCB(0, &ptr, NULL));
	INTR_DISPATCH_TITLE("Valid mbx", sceKernelReceiveMbxCB(mbx, &ptr, NULL));
	sceKernelDeleteMbx(mbx);

	SceUID fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	checkpointNext("sceKernelAllocateFpl:");
	INTR_DISPATCH_TITLE("Bad fpl", sceKernelAllocateFpl(0, &ptr, NULL));
	INTR_DISPATCH_TITLE("Valid fpl", sceKernelAllocateFpl(fpl, &ptr, NULL));
	checkpointNext("sceKernelAllocateFplCB:");
	INTR_DISPATCH_TITLE("Bad fpl", sceKernelAllocateFplCB(0, &ptr, NULL));
	INTR_DISPATCH_TITLE("Valid fpl", sceKernelAllocateFplCB(fpl, &ptr, NULL));
	sceKernelDeleteFpl(fpl);

	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	checkpointNext("sceKernelAllocateVpl:");
	INTR_DISPATCH_TITLE("Bad vpl", sceKernelAllocateVpl(0, 0x10, &ptr, NULL));
	INTR_DISPATCH_TITLE("Bad size", sceKernelAllocateVpl(vpl, 0, &ptr, NULL));
	INTR_DISPATCH_TITLE("Valid vpl", sceKernelAllocateVpl(vpl, 0x10, &ptr, NULL));
	checkpointNext("sceKernelAllocateVplCB:");
	INTR_DISPATCH_TITLE("Bad vpl", sceKernelAllocateVplCB(0, 0x10, &ptr, NULL));
	INTR_DISPATCH_TITLE("Bad size", sceKernelAllocateVplCB(vpl, 0, &ptr, NULL));
	INTR_DISPATCH_TITLE("Valid vpl", sceKernelAllocateVplCB(vpl, 0x10, &ptr, NULL));
	sceKernelDeleteVpl(vpl);

	SceUID tls = sceKernelCreateTls("tls", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	checkpointNext("sceKernelAllocateTls:");
	INTR_DISPATCH_TITLE("Bad tls", sceKernelAllocateTlsHelper(0));
	INTR_DISPATCH_TITLE("Valid tls", sceKernelAllocateTlsHelper(tls));
	sceKernelDeleteTls(tls);

	SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, (void *)0, NULL);
	checkpointNext("sceKernelReceiveMsgPipe:");
	INTR_DISPATCH_TITLE("Bad msgpipe", sceKernelReceiveMsgPipe(0, buf, 256, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Bad size", sceKernelReceiveMsgPipe(msgpipe, buf, -1, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Valid msgpipe", sceKernelReceiveMsgPipe(msgpipe, buf, 256, 0, NULL, NULL));
	checkpointNext("sceKernelReceiveMsgPipeCB:");
	INTR_DISPATCH_TITLE("Bad msgpipe", sceKernelReceiveMsgPipeCB(0, buf, 256, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Bad size", sceKernelReceiveMsgPipeCB(msgpipe, buf, -1, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Valid msgpipe", sceKernelReceiveMsgPipeCB(msgpipe, buf, 256, 0, NULL, NULL));
	checkpointNext("sceKernelSendMsgPipe:");
	INTR_DISPATCH_TITLE("Bad msgpipe", sceKernelSendMsgPipe(0, buf, 256, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Bad size", sceKernelSendMsgPipe(msgpipe, buf, -1, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Valid msgpipe", sceKernelSendMsgPipe(msgpipe, buf, 256, 0, NULL, NULL));
	checkpointNext("sceKernelSendMsgPipeCB:");
	INTR_DISPATCH_TITLE("Bad msgpipe", sceKernelSendMsgPipeCB(0, buf, 256, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Bad size", sceKernelSendMsgPipeCB(msgpipe, buf, -1, 0, NULL, NULL));
	INTR_DISPATCH_TITLE("Valid msgpipe", sceKernelSendMsgPipeCB(msgpipe, buf, 256, 0, NULL, NULL));
	sceKernelDeleteMsgPipe(msgpipe);

	SceUID mutex = sceKernelCreateMutex("mutex", 0, 0, NULL);
	checkpointNext("sceKernelLockMutex:");
	INTR_DISPATCH_TITLE("Bad mutex", sceKernelLockMutex(0, 1, NULL));
	INTR_DISPATCH_TITLE("Bad count", sceKernelLockMutex(mutex, 9, NULL));
	INTR_DISPATCH_TITLE("Valid mutex", sceKernelLockMutex(mutex, 1, NULL));
	checkpointNext("sceKernelLockMutexCB:");
	INTR_DISPATCH_TITLE("Bad mutex", sceKernelLockMutexCB(0, 1, NULL));
	INTR_DISPATCH_TITLE("Bad count", sceKernelLockMutexCB(mutex, 9, NULL));
	INTR_DISPATCH_TITLE("Valid mutex", sceKernelLockMutexCB(mutex, 1, NULL));
	sceKernelDeleteMutex(mutex);

	sceKernelCreateLwMutex(buf, "lwmutex", 0, 0, NULL);
	checkpointNext("sceKernelLockLwMutex:");
	INTR_DISPATCH_TITLE("Bad count", sceKernelLockLwMutex(buf, 9, NULL));
	INTR_DISPATCH_TITLE("Valid mutex", sceKernelLockLwMutex(buf, 1, NULL));
	checkpointNext("sceKernelLockLwMutexCB:");
	INTR_DISPATCH_TITLE("Bad count", sceKernelLockLwMutexCB(buf, 9, NULL));
	INTR_DISPATCH_TITLE("Valid mutex", sceKernelLockLwMutexCB(buf, 1, NULL));
	sceKernelDeleteLwMutex(buf);

	checkpointNext("sceKernelStartModule:");
	INTR_DISPATCH(sceKernelStartModule(sceKernelGetModuleId(), 0, NULL, NULL, NULL));
	checkpointNext("sceKernelStopModule:");
	INTR_DISPATCH(sceKernelStopModule(sceKernelGetModuleId(), 0, NULL, NULL, NULL));

	SceUID notRunningThread = sceKernelCreateThread("notRunning", &dummyThread, 0x20, 0x1000, 0, NULL);
	checkpointNext("sceKernelWaitThreadEnd:");
	INTR_DISPATCH_TITLE("Bad thread", sceKernelWaitThreadEnd(0, NULL));
	INTR_DISPATCH_TITLE("Not running", sceKernelWaitThreadEnd(notRunningThread, NULL));
	INTR_DISPATCH_TITLE("Running", sceKernelWaitThreadEnd(reschedThread, NULL));
	checkpointNext("sceKernelWaitThreadEndCB:");
	INTR_DISPATCH_TITLE("Bad thread", sceKernelWaitThreadEndCB(0, NULL));
	INTR_DISPATCH_TITLE("Not running", sceKernelWaitThreadEndCB(notRunningThread, NULL));
	INTR_DISPATCH_TITLE("Running", sceKernelWaitThreadEndCB(reschedThread, NULL));
	sceKernelDeleteThread(notRunningThread);

	SceCtrlData pad[64];
	sceCtrlReadBufferPositive(pad, 64);
	checkpointNext("sceCtrlReadBufferPositive:");
	INTR_DISPATCH_TITLE("Bad count", sceCtrlReadBufferPositive(pad, 256));
	INTR_DISPATCH_TITLE("Valid", sceCtrlReadBufferPositive(pad, 64));

	sceAudioChReserve(0, 64, PSP_AUDIO_FORMAT_STEREO);
	checkpointNext("sceAudioOutputBlocking:");
	INTR_DISPATCH_TITLE("Invalid channel", sceAudioOutputBlocking(1, 0, buf));
	INTR_DISPATCH_TITLE("Valid channel - 64", sceAudioOutputBlocking(0, 0, buf));
	sceAudioSetChannelDataLen(0, 128);
	INTR_DISPATCH_TITLE("Valid channel - 128", sceAudioOutputBlocking(0, 0, buf));
	sceAudioChRelease(0);

	sceAudioSRCChReserve(64, 44100, 2);
	checkpointNext("sceAudioSRCOutputBlocking:");
	INTR_DISPATCH_TITLE("Valid channel - 64", sceAudioSRCOutputBlocking(0, buf));
	sceAudioSRCOutputBlocking(0, NULL);
	sceAudioSRCChRelease();
	sceAudioSRCChReserve(128, 44100, 2);
	INTR_DISPATCH_TITLE("Valid channel - 128", sceAudioSRCOutputBlocking(0, buf));
	sceAudioSRCChRelease();

	int dl = sceGeListEnQueue(dlist, NULL, -1, NULL);
	checkpointNext("sceGeListSync:");
	INTR_DISPATCH_TITLE("Invalid list", sceGeListSync(9999, 0));
	INTR_DISPATCH_TITLE("Mode 0", sceGeListSync(dl, 0));
	INTR_DISPATCH_TITLE("Mode 1", sceGeListSync(dl, 1));
	INTR_DISPATCH_TITLE("Mode 2", sceGeListSync(dl, 2));
	sceGeListDeQueue(dl);

	checkpointNext("sceGeDrawSync:");
	INTR_DISPATCH_TITLE("Mode 0", sceGeDrawSync(0));
	INTR_DISPATCH_TITLE("Mode 1", sceGeDrawSync(1));
	INTR_DISPATCH_TITLE("Mode 2", sceGeDrawSync(2));

	int fd = sceIoOpen("ms0:/_intr_waits_test.txt", PSP_O_CREAT | PSP_O_RDWR, 0777);
	s64 iores;
	checkpointNext("sceIoRead:");
	INTR_DISPATCH_TITLE("Bad file", sceIoRead(63, buf, 1));
	INTR_DISPATCH_TITLE("Valid", sceIoRead(fd, buf, 1));
	checkpointNext("sceIoWrite:");
	INTR_DISPATCH_TITLE("Bad file", sceIoWrite(63, buf, 1));
	INTR_DISPATCH_TITLE("Valid", sceIoWrite(fd, buf, 1));
	checkpointNext("sceIoWaitAsync:");
	sceIoLseekAsync(fd, 0, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Bad file", sceIoWaitAsync(63, &iores));
	sceIoLseekAsync(fd, 1, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Valid", sceIoWaitAsync(fd, &iores));
	checkpointNext("sceIoWaitAsyncCB:");
	sceIoLseekAsync(fd, 2, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Bad file", sceIoWaitAsyncCB(63, &iores));
	sceIoLseekAsync(fd, 3, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Valid", sceIoWaitAsyncCB(fd, &iores));
	checkpointNext("sceIoGetAsyncStat:");
	sceIoLseekAsync(fd, 4, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Bad file", sceIoGetAsyncStat(63, 0, &iores));
	sceIoLseekAsync(fd, 5, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Peek", sceIoGetAsyncStat(fd, 1, &iores));
	sceIoLseekAsync(fd, 6, PSP_SEEK_SET);
	INTR_DISPATCH_TITLE("Valid", sceIoGetAsyncStat(fd, 0, &iores));
	sceIoClose(fd);
	sceIoRemove("ms0:/_intr_waits_test.txt");

	checkpointNext("sceKernelVolatileMemLock:");
	INTR_DISPATCH_TITLE("While not locked", sceKernelVolatileMemLock(0, NULL, NULL));
	INTR_DISPATCH_TITLE("While locked", sceKernelVolatileMemLock(0, NULL, NULL));
	sceKernelVolatileMemUnlock(0);

	checkpointNext("sceUmdWaitDriveStat:");
	INTR_DISPATCH_TITLE("Invalid type", sceUmdWaitDriveStat(0));
	INTR_DISPATCH_TITLE("Valid type", sceUmdWaitDriveStat(0x20));

	checkpointNext("sceUmdWaitDriveStatWithTimer:");
	INTR_DISPATCH_TITLE("Invalid type", sceUmdWaitDriveStatWithTimer(0, 100));
	INTR_DISPATCH_TITLE("Valid type", sceUmdWaitDriveStatWithTimer(0x20, 100));

	checkpointNext("sceUmdWaitDriveStatCB:");
	INTR_DISPATCH_TITLE("Invalid type", sceUmdWaitDriveStatCB(0, 100));
	INTR_DISPATCH_TITLE("Valid type", sceUmdWaitDriveStatCB(0x20, 100));

	intrSema = sceKernelCreateSema("sema", 0, 0, 1, NULL);
	intrFlag = sceKernelCreateEventFlag("flag", 0, 0, NULL);
	intrMbx = sceKernelCreateMbx("mbx", 0, NULL);
	intrFpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	intrVpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	intrTls = sceKernelCreateTls("tls", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	intrMsgPipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, (void *)0, NULL);
	intrMutex = sceKernelCreateMutex("mutex", 0, 0, NULL);
	sceKernelCreateLwMutex(intrLwMutex, "lwmutex", 0, 0, NULL);
	intrThread = sceKernelCreateThread("notRunning", &dummyThread, 0x20, 0x1000, 0, NULL);
	sceAudioChReserve(0, 64, PSP_AUDIO_FORMAT_STEREO);
	sceAudioSRCChReserve(64, 44100, 2);
	intrDlist = sceGeListEnQueue(dlist, NULL, -1, NULL);
	intrFd = sceIoOpen("ms0:/_intr_waits_test.txt", PSP_O_CREAT | PSP_O_RDWR, 0777);

	checkpointNext("Inside interrupt:");
	checkpoint("sceKernelRegisterSubIntrHandler 1: %08x", sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 1, (void *)interruptFunc, NULL));
	checkpoint("sceKernelEnableSubIntr: %08x", sceKernelEnableSubIntr(PSP_VBLANK_INT, 1));
	checkpoint("sceKernelDelayThread: %08x", sceKernelDelayThread(30000));
	checkpoint("sceKernelDisableSubIntr: %08x", sceKernelDisableSubIntr(PSP_VBLANK_INT, 1));
	checkpoint("sceKernelReleaseSubIntrHandler: %08x", sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 1));

	sceKernelDeleteSema(intrSema);
	sceKernelDeleteEventFlag(intrFlag);
	sceKernelDeleteMbx(mbx);
	sceKernelDeleteFpl(fpl);
	sceKernelDeleteVpl(vpl);
	sceKernelDeleteTls(tls);
	sceKernelDeleteMsgPipe(intrMsgPipe);
	sceKernelDeleteMutex(mutex);
	sceKernelDeleteLwMutex(intrLwMutex);
	sceKernelDeleteThread(intrThread);
	sceAudioChRelease(0);
	sceAudioSRCChRelease();
	sceGeListDeQueue(intrDlist);
	sceIoClose(fd);
	sceIoRemove("ms0:/_intr_waits_test.txt");

	return 0;
}