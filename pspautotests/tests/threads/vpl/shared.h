#include <common.h>
#include <stdarg.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <pspintrman.h>
#include <psploadexec.h>
#include <pspmodulemgr.h>
#include <psprtc.h>

enum SceKernelVplAttr {
	PSP_VPL_ATTR_FIFO = 0x0000,
	PSP_VPL_ATTR_PRIORITY = 0x0100,
	PSP_VPL_ATTR_SMALLEST = 0x0200,
	PSP_VPL_ATTR_HIGHMEM = 0x4000,
};

// Keep track of the last thread we saw here.
static volatile int schedulingPlacement = 0;
// So we can log the result from the thread.
static int schedulingResult = -1;

inline void schedVplInfo(SceKernelVplInfo *info) {
	schedf("VPL: OK (size=%d,name=%s,attr=%08X,poolSize=%08X,freeSize=%08X,wait=%d)\n", info->size, info->name, info->attr, info->poolSize, info->freeSize, info->numWaitThreads);
}

inline void schedfVpl(SceUID vpl) {
	if (vpl > 0) {
		SceKernelVplInfo info;
		info.size = sizeof(info);

		int result = sceKernelReferVplStatus(vpl, &info);
		if (result == 0) {
			schedVplInfo(&info);
		} else {
			schedf("VPL: Invalid (%08X)\n", result);
		}
	} else {
		schedf("VPL: Failed (%08X)\n", vpl);
	}
}

inline void printfVpl(SceUID vpl) {
	schedfVpl(vpl);
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
	void *data;
	schedulingResult = -1;

	SCHED_LOG(B, 2);
	// Constantly loop setting the placement to 2 whenever we're active.
	while (result == 0x800201A8) {
		schedulingPlacement = 2;
		timeout = 5;
		result = sceKernelAllocateVplCB(*(SceUID*) argPointer, 0xF000, &data, &timeout);
	}
	SCHED_LOG(D, 2);

	schedulingResult = result;
	return 0;
}

#define BASIC_SCHED_TEST(title, x) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceUID vpl1 = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL); \
	SceUID vpl2 = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL); \
	void *data1, *data2; \
	sceKernelAllocateVpl(vpl1, 0x8000, &data1, NULL); \
	sceKernelAllocateVpl(vpl2, 0x8000, &data2, NULL); \
	int result = -1; \
	\
	schedulingPlacement = 1; \
	schedf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(SceUID), &vpl1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteVpl(vpl1); \
	SCHED_LOG(F, 1); \
	\
	schedf(" (thread=%08X, main=%08X)\n", schedulingResult, result); \
	sceKernelDeleteVpl(vpl2); \
	sceKernelTerminateThread(thread); \
}
