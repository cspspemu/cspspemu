#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

typedef struct {
	SceSize size;
	char name[32];
	u32 attr;
	int initCount;
	int currentCount;
	SceUID lockThread;
	int numWaitThreads;
} SceKernelMutexInfo;

SceUID sceKernelCreateMutex(const char *name, uint attributes, int initial_count, void *options);
int sceKernelDeleteMutex(SceUID mutexId);
int sceKernelLockMutex(SceUID mutexId, int count, SceUInt *timeout);
int sceKernelLockMutexCB(SceUID mutexId, int count, SceUInt *timeout);
int sceKernelTryLockMutex(SceUID mutexId, int count);
int sceKernelUnlockMutex(SceUID mutexId, int count);
int sceKernelReferMutexStatus(SceUID mutexId, SceKernelMutexInfo *status);
int sceKernelCancelMutex(SceUID mutexId, int count, int *numWaitingThreads);

#define PSP_MUTEX_ATTR_FIFO 0
#define PSP_MUTEX_ATTR_PRIORITY 0x100
#define PSP_MUTEX_ATTR_ALLOW_RECURSIVE 0x200

// Keep track of the last thread we saw here.
static volatile int schedulingPlacement = 0;
// So we can log the result from the thread.
static int schedulingResult = -1;

inline void schedfMutexInfo(SceKernelMutexInfo *info) {
	schedf("Mutex: OK (size=%d,name=%s,attr=%08x,init=%d,current=%d,lockThread=%d,waiting=%08x)\n", info->size, info->name, info->attr, info->initCount, info->currentCount, info->lockThread == -1 ? 0 : 1, info->numWaitThreads);
}

inline void schedfMutex(SceUID mutex) {
	if (mutex > 0) {
		SceKernelMutexInfo info;
		info.size = sizeof(info);

		int result = sceKernelReferMutexStatus(mutex, &info);
		if (result == 0) {
			schedfMutexInfo(&info);
		} else {
			schedf("Mutex: Invalid (%08X)\n", result);
		}
	} else {
		schedf("Mutex: Failed (%08X)\n", mutex);
	}
}

inline void printfMutex(SceUID mutex) {
	schedfMutex(mutex);
	flushschedf();
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

static int scheduleTestFunc(SceSize argSize, void* argPointer) {
	int result = 0x800201A8;
	SceUInt timeout;
	schedulingResult = -1;

	SCHED_LOG(B, 2);
	// Constantly loop setting the placement to 2 whenever we're active.
	while (result == 0x800201A8) {
		schedulingPlacement = 2;
		timeout = 1;
		result = sceKernelLockMutexCB(*(int*) argPointer, 1, &timeout);
	}
	SCHED_LOG(D, 2);

	schedulingResult = result;
	return 0;
}

#define LOCKED_SCHED_TEST(title, init1, init2, x) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceUID mutex1 = sceKernelCreateMutex("schedTest1", 0, init1, NULL); \
	SceUID mutex2 = sceKernelCreateMutex("schedTest2", 0, init2, NULL); \
	int result = -1; \
	\
	flushschedf(); \
	schedulingPlacement = 1; \
	printf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(int), &mutex1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteMutex(mutex1); \
	SCHED_LOG(F, 1); \
	\
	flushschedf(); \
	printf(" (thread=%08X, main=%08X)\n", schedulingResult, result); \
	sceKernelDeleteMutex(mutex2); \
}
#define BASIC_SCHED_TEST(title, x) LOCKED_SCHED_TEST(title, 1, 0, x);