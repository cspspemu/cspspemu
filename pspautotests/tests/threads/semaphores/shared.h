#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

#define PRINT_SEMAPHORE(sema) { \
	schedfSema(sema); \
	flushschedf(); \
}

inline void schedfSema(SceUID sema) {
	if (sema > 0) {
		SceKernelSemaInfo semainfo;
		semainfo.size = sizeof(semainfo);
		int result = sceKernelReferSemaStatus(sema, &semainfo);
		if (result == 0) {
			schedf("Sema: OK (size=%d,name='%s',attr=%d,init=%d,cur=%d,max=%d,wait=%d)\n", semainfo.size, semainfo.name, semainfo.attr, semainfo.initCount, semainfo.currentCount, semainfo.maxCount, semainfo.numWaitThreads);
		} else {
			schedf("Sema: Invalid (%08X)\n", result);
		}
	} else {
		schedf("Sema: Failed (%08X)\n", sema);
	}
}

#define CREATE_PRIORITY_THREAD(func, priority) \
	sceKernelCreateThread(#func, &func, priority, 0x10000, 0, NULL)
#define CREATE_SIMPLE_THREAD(func) CREATE_PRIORITY_THREAD(func, 0x12)

// Log a checkpoint and which thread was last active.
#define SCHED_LOG(letter, placement) { \
	int old = schedulingPlacement; \
	schedulingPlacement = placement; \
	schedf(#letter "%d", old); \
}

// Avoid linking or other things.
#define SETUP_SCHED_TEST \
	/* Keep track of the last thread we saw here. */ \
	static volatile int schedulingPlacement = 0; \
	/* So we can log the result from the thread. */ \
	static int schedulingResult = -1; \
	\
	static int scheduleTestFunc(SceSize argSize, void* argPointer) { \
		int result = 0x800201A8; \
		SceUInt timeout; \
		schedulingResult = -1; \
		\
		SCHED_LOG(B, 2); \
		/* Constantly loop setting the placement to 2 whenever we're active. */ \
		while (result == 0x800201A8) { \
			schedulingPlacement = 2; \
			timeout = 1; \
			result = sceKernelWaitSemaCB(*(int*) argPointer, 1, &timeout); \
		} \
		SCHED_LOG(D, 2); \
		\
		schedulingResult = result; \
		return 0; \
	}

#define TWO_STEP_SCHED_TEST(title, init1, init2, x, y) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceUID sema1 = sceKernelCreateSema("schedTest1", 0, init1, 1, NULL); \
	SceUID sema2 = sceKernelCreateSema("schedTest2", 0, init2, 1, NULL); \
	int result = -1; \
	\
	schedulingPlacement = 1; \
	schedf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(int), &sema1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	y \
	SCHED_LOG(F, 1); \
	\
	schedf(" (thread=%08X, main=%08X)\n", schedulingResult, result); \
	sceKernelDeleteSema(sema1); \
	sceKernelDeleteSema(sema2); \
	flushschedf(); \
}
#define BASIC_SCHED_TEST(title, x) TWO_STEP_SCHED_TEST(title, 0, 1, x, sceKernelDeleteSema(sema1);)

int sceKernelCancelSema(SceUID semaId, int count, int *waitThreads);