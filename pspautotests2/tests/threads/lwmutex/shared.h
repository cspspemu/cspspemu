#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

typedef struct {
	int count;
	SceUID thread;
	int attr;
	int numWaitThreads;
	SceUID uid;
	int pad[3];
} SceLwMutexWorkarea;

typedef struct {
	SceSize size;
	char name[32];
	SceUInt attr;
	SceUID uid;
	void *workarea;
	int initCount;
	int currentCount;
	SceUID lockThread;
	int numWaitThreads;
} SceKernelLwMutexInfo;

int sceKernelCreateLwMutex(void *workarea, const char *name, uint attr, int count, void *options);
int sceKernelDeleteLwMutex(void *workarea);
int sceKernelTryLockLwMutex(void *workarea, int count);
int sceKernelTryLockLwMutex_600(void *workarea, int count);
int sceKernelLockLwMutex(void *workarea, int count, SceUInt *timeout);
int sceKernelLockLwMutexCB(void *workarea, int count, SceUInt *timeout);
int sceKernelUnlockLwMutex(void *workarea, int count);
int sceKernelReferLwMutexStatus(void *workarea, SceKernelLwMutexInfo *status);
int sceKernelReferLwMutexStatusByID(SceUID mutexID, SceKernelLwMutexInfo *status);

#define PSP_MUTEX_ATTR_FIFO 0
#define PSP_MUTEX_ATTR_PRIORITY 0x100
#define PSP_MUTEX_ATTR_ALLOW_RECURSIVE 0x200

// Keep track of the last thread we saw here.
static volatile int schedulingPlacement = 0;
// So we can log the result from the thread.
static int schedulingResult = -1;

#define CREATE_PRIORITY_THREAD(func, priority) \
	sceKernelCreateThread(#func, &func, priority, 0x10000, 0, NULL)
#define CREATE_SIMPLE_THREAD(func) CREATE_PRIORITY_THREAD(func, 0x12)

static inline void schedfLwMutexWorkarea(SceLwMutexWorkarea *workarea) {
	schedf("LwMutexWorkarea (count=%d, thread=%08X, attr=%03X, waiting=%d, uid=%d, %08X, %08X, %08X)\n", workarea->count, workarea->thread > 0 ? 1 : workarea->thread, workarea->attr, workarea->numWaitThreads, workarea->uid > 0 ? 1 : workarea->uid, workarea->pad[0], workarea->pad[1], workarea->pad[2]);
}

static inline void printfLwMutexWorkarea(SceLwMutexWorkarea *workarea) {
	schedfLwMutexWorkarea(workarea);
}

inline void schedfLwMutexInfo(SceLwMutexWorkarea *workarea, SceKernelLwMutexInfo *info) {
	schedf("LwMutex: OK (size=%d,name=%s,attr=%08x,uid=%d,workarea=%d,init=%d,current=%d,lockThread=%d)\n", info->size, info->name, info->attr, info->uid == workarea->uid ? 1 : 0, info->workarea == workarea ? 1 : 0, info->initCount, info->currentCount, info->lockThread == -1 ? 0 : 1);
	schedfLwMutexWorkarea(workarea);
}

inline void schedfLwMutex(SceLwMutexWorkarea *workarea) {
	if (workarea != NULL && workarea != (void *)0xDEADBEEF) {
		SceKernelLwMutexInfo info, info2;
		// Avoid garbage after the name.
		memset(&info, 0, sizeof(info));
		memset(&info2, 0, sizeof(info2));
		info.size = sizeof(info);

		int result = sceKernelReferLwMutexStatus(workarea, &info);
		if (result == 0) {
			result = sceKernelReferLwMutexStatusByID(workarea->uid, &info2);
			if (result == 0 && memcmp(&info, &info2, sizeof(info)) != 0) {
				schedfLwMutexInfo(workarea, &info);
			} else if (result != 0) {
				schedf("LwMutex: Failed by id (%08X)\n", result);
			} else {
				schedf("LwMutex: Status info differed (%08X)\n", result);
			}
		} else {
			schedf("LwMutex: Invalid (%08X)\n", result);
		}
	} else {
		schedf("LwMutex: Failed\n");
	}
}

inline void printfLwMutex(SceLwMutexWorkarea *workarea) {
	schedfLwMutex(workarea);
	flushschedf();
}

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
		result = sceKernelLockLwMutexCB(*(void**) argPointer, 1, &timeout);
	}
	SCHED_LOG(D, 2);

	schedulingResult = result;
	if (result == 0)
		sceKernelUnlockLwMutex(*(void**) argPointer, 1);
	return 0;
}

#define LOCKED_SCHED_TEST(title, init1, init2, x) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceLwMutexWorkarea workarea1, workarea2; \
	sceKernelCreateLwMutex(&workarea1, "schedTest1", 0, init1, NULL); \
	sceKernelCreateLwMutex(&workarea2, "schedTest2", 0, init2, NULL); \
	int result = -1; \
	\
	flushschedf(); \
	schedulingPlacement = 1; \
	schedf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	void *workarea1Ptr = &workarea1; \
	sceKernelStartThread(thread, sizeof(int), &workarea1Ptr); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteLwMutex(&workarea1); \
	SCHED_LOG(F, 1); \
	\
	flushschedf(); \
	schedf(" (thread=%08X, main=%08X)\n", schedulingResult, result); \
	sceKernelDeleteLwMutex(&workarea2); \
}
#define BASIC_SCHED_TEST(title, x) LOCKED_SCHED_TEST(title, 1, 0, x);

#define FAKE_LWMUTEX(workarea, attrib, init) { \
	workarea.count = init; \
	workarea.thread = init > 0 ? sceKernelGetThreadId() : 0; \
	workarea.attr = attrib; \
	workarea.numWaitThreads = 0; \
	workarea.uid = 0; \
	workarea.pad[0] = 0xDEADBEEF; \
	workarea.pad[1] = 0xDEADBEEF; \
	workarea.pad[2] = 0xDEADBEEF; \
}
