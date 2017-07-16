#include <common.h>
#include <stdarg.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <pspintrman.h>
#include <psploadexec.h>
#include <psprtc.h>

// Keep track of the last thread we saw here.
static volatile int schedulingPlacement = 0;
// So we can log the result from the thread.
static int schedulingResult = -1;
// printf() seems to reschedule, so can't use it.
static char schedulingLog[65536];
static volatile int schedulingLogPos = 0;

inline void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLog + schedulingLogPos, format, args);
	// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
	//vprintf(format, args);
	va_end(args);
}

inline void flushschedf() {
	printf("%s", schedulingLog);
	schedulingLogPos = 0;
}

inline void schedfAlarm(SceUID alarm, s64 currentTime) {
	if (alarm > 0) {
		SceKernelAlarmInfo info;
		info.size = sizeof(info);

		int result = sceKernelReferAlarmStatus(alarm, &info);
		if (result == 0) {
			schedf("Alarm: OK (size=%d,schedule=%lld##,handler=%d,common=%d)\n", info.size, (*(u64 *) &info.schedule - currentTime + 10LL) / 100LL, info.handler != NULL, info.common != NULL);
		} else {
			schedf("Alarm: Invalid (%08X)\n", result);
		}
	} else {
		schedf("Alarm: Failed (%08X)\n", alarm);
	}
}

inline void printfAlarm(SceUID alarm, s64 currentTime) {
	schedfAlarm(alarm, currentTime);
	flushschedf();
}

#define CREATE_PRIORITY_THREAD(func, priority) \
	sceKernelCreateThread(#func, &func, priority, 0x10000, 0, NULL)
#define CREATE_SIMPLE_THREAD(func) CREATE_PRIORITY_THREAD(func, 0x12)

// Log a checkpoint and which thread was last active.
#define SCHED_LOG(letter, placement) { \
	int old = schedulingPlacement; \
	schedulingPlacement = placement; \
	schedulingLogPos += sprintf(schedulingLog + schedulingLogPos, #letter "%d", old); \
}

static int scheduleTestFunc(SceSize argSize, void* argPointer) {
	int result = 0x800201A8;
	SceUInt timeout;
	schedulingResult = -1;

	SCHED_LOG(B, 2);
	// Constantly loop setting the placement to 2 whenever we're active.
	while (result == 0x800201A8) {
		schedulingPlacement = 2;
		timeout = 1;
		result = sceKernelWaitSemaCB(*(SceUID*) argPointer, 1, &timeout);
	}
	SCHED_LOG(D, 2);

	schedulingResult = result;
	return 0;
}

#define BASIC_SCHED_TEST(title, x) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceUID sema1 = sceKernelCreateSema("schedTest1", 0, 0, 1, NULL); \
	int result = -1; \
	\
	schedulingLogPos = 0; \
	schedulingPlacement = 1; \
	printf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(int), &sema1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteSema(sema1); \
	SCHED_LOG(F, 1); \
	\
	schedulingLogPos = 0; \
	printf("%s (thread=%08X, main=%08X)\n", schedulingLog, schedulingResult, result); \
	sceKernelTerminateThread(thread); \
}
