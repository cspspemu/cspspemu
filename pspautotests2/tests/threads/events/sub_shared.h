#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

#define PRINT_FLAG(flag) { \
	if (flag > 0) { \
		SceKernelEventFlagInfo flaginfo; \
		flaginfo.size = sizeof(flaginfo); \
		int result = sceKernelReferEventFlagStatus(flag, &flaginfo); \
		if (result == 0) { \
			printf("Event flag: OK (size=%d,name='%s',attr=%d,init=%08X,cur=%08X,wait=%d)\n", flaginfo.size, flaginfo.name, flaginfo.attr, flaginfo.initPattern, flaginfo.currentPattern, flaginfo.numWaitThreads); \
		} else { \
			printf("Event flag: Invalid (%08X)\n", result); \
		} \
	} else { \
		printf("Event flag: Failed (%08X)\n", flag); \
	} \
}

#define PSP_EVENT_ATTR_ALLOW_WAITMULTIPLE 0x200
#define PSP_EVENT_WAITCLEARALL 0x10

#define CREATE_PRIORITY_THREAD(func, priority) \
	sceKernelCreateThread(#func, &func, priority, 0x10000, 0, NULL)
#define CREATE_SIMPLE_THREAD(func) CREATE_PRIORITY_THREAD(func, 0x12)

// Log a checkpoint and which thread was last active.
#define SCHED_LOG(letter, placement) { \
	int old = schedulingPlacement; \
	schedulingPlacement = placement; \
	schedulingLogPos += sprintf(schedulingLog + schedulingLogPos, #letter "%d", old); \
}

// Avoid linking or other things.
#define SETUP_SCHED_TEST \
	/* Keep track of the last thread we saw here. */ \
	static volatile int schedulingPlacement = 0; \
	/* So we can log the result from the thread. */ \
	static int schedulingResult = -1; \
	static u32 schedulingResultBits = 0xDEADBEEF; \
	/* printf() seems to reschedule, so can't use it. */ \
	static char schedulingLog[8192]; \
	static volatile int schedulingLogPos = 0; \
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
			result = sceKernelWaitEventFlagCB(*(int*) argPointer, 0xFFFFFF, PSP_EVENT_WAITOR, &schedulingResultBits, &timeout); \
		} \
		SCHED_LOG(D, 2); \
		\
		schedulingResult = result; \
		return 0; \
	}

#define LOCKED_SCHED_TEST(title, attr1, init1, attr2, init2, x) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceUID flag1 = sceKernelCreateEventFlag("schedTest1", attr1, init1, NULL); \
	SceUID flag2 = sceKernelCreateEventFlag("schedTest2", attr2, init2, NULL); \
	int result = -1; \
	\
	schedulingLogPos = 0; \
	schedulingPlacement = 1; \
	printf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(int), &flag1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteEventFlag(flag1); \
	SCHED_LOG(F, 1); \
	\
	schedulingLog[schedulingLogPos] = 0; \
	schedulingLogPos = 0; \
	printf("%s (thread=%08X, thread bits=%08X, main=%08X)\n", schedulingLog, schedulingResult, (unsigned int) schedulingResultBits, result); \
	sceKernelDeleteEventFlag(flag2); \
}
#define BASIC_SCHED_TEST(title, x) LOCKED_SCHED_TEST(title, 0, 0x00000000, 0, 0xFFFFFFFF, x);

int sceKernelCancelEventFlag(SceUID flag, u32 pattern, int *numWaitThreads);