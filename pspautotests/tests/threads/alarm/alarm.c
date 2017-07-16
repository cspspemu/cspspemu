#include <common.h>
#include <stdarg.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspintrman.h>
#include <pspumd.h>
#include <psprtc.h>

int sceKernelCancelSema(SceUID uid, int count, int *numWaitingThreads);

char schedulingLog[65536];
char *schedulingLogPos;

int thread1, thread2;

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLogPos, format, args);
	// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
	//vprintf(format, args);
	va_end(args);
}

inline int whichThread() {
	if (sceKernelGetThreadId() == thread1) {
		return 1;
	} else if (sceKernelGetThreadId() == thread2) {
		return 2;
	} else {
		return -1;
	}
}

volatile int thread2DidRun = 0;

SceUInt alarmHandler(void *common) {
	schedf("alarmHandler called: %08x on thread %d\n", common, whichThread());
	return 100;
}

SceUInt alarmHandler2(void *common) {
	schedf("alarmHandler2 called on thread %d\n", whichThread());

	int result = sceKernelCancelSema(*(SceUID *) common, 0, NULL);
	schedf("sceKernelCancelSema: %08X\n", result);
	return 0;
}

int thread2Func(SceSize argc, void* argv) {
	thread2 = sceKernelGetThreadId();
	schedf("thread2 started\n");

	while (thread2DidRun < 10)
	{
		sceKernelDelayThread(100);
		thread2DidRun++;
		schedf("thread2 awake (%d)\n", thread2DidRun);
	}

	return 0;
}

int main(int argc, char **argv) {
	schedulingLogPos = schedulingLog;

	SceUID otherThread = sceKernelCreateThread("thread2", thread2Func, 0x1F, 0x1000, 0, 0);
	sceKernelStartThread(otherThread, 0, 0);

	thread1 = sceKernelGetThreadId();

	SceUID alarm = sceKernelSetAlarm(2500, &alarmHandler, (void*) 0x1234);
	if (alarm >= 0) {
		schedf("sceKernelSetAlarm: OK\n");
	} else {
		schedf("sceKernelSetAlarm: Failed (%08X)\n", alarm);
	}

	int blah = sceKernelCpuSuspendIntr();
	int i;
	for (i = 0; i < 800000; ++i) {
		continue;
	}
	sceKernelCpuResumeIntr(blah);

	int result = sceKernelCancelAlarm(alarm);
	schedf("sceKernelCancelAlarm: %08X\n", result);

	SceUID sema = sceKernelCreateSema("test", 0, 0, 1, NULL);
	alarm = sceKernelSetAlarm(2500, &alarmHandler2, &sema);
	if (alarm >= 0) {
		schedf("sceKernelSetAlarm: OK\n");
	} else {
		schedf("sceKernelSetAlarm: Failed (%08X)\n", alarm);
	}

	SceUInt timeout = 5000;
	result = sceKernelWaitSema(sema, 1, &timeout);

	sceKernelCancelAlarm(alarm);

	schedf("Canceled sema: %08X\n", result);

	schedf("thread2: %d\n", thread2DidRun);
	sceKernelTerminateDeleteThread(otherThread);

	printf("%s", schedulingLog);
	schedulingLogPos = schedulingLog;

	return 0;
}
