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

#define CREATE_PRIORITY_THREAD(func, priority) \
	sceKernelCreateThread(#func, &func, priority, 0x10000, 0, NULL)
#define CREATE_SIMPLE_THREAD(func) CREATE_PRIORITY_THREAD(func, 0x12)

// Log a checkpoint and which thread was last active.
#define SCHED_LOG(letter, placement) { \
	int old = schedulingPlacement; \
	schedulingPlacement = placement; \
	schedf("%s%d", #letter, old); \
}

struct mem_entry {
	u32 start;
	u32 size;
	const char *name;
};

static struct mem_entry g_memareas[] = {
	{0x08800000, (24 * 1024 * 1024), "USER"},
	{0x48800000, (24 * 1024 * 1024), "USER XC"},
	{0x88000000, (4 * 1024 * 1024), "KERNEL LOW"},
	{0xA8000000, (4 * 1024 * 1024), "KERNEL LOW XC"},
	{0x88400000, (4 * 1024 * 1024), "KERNEL MID"},
	{0xC8400000, (4 * 1024 * 1024), "KERNEL MID XC"},
	{0x88800000, (24 * 1024 * 1024), "KERNEL HIGH"},
	{0xA8800000, (24 * 1024 * 1024), "KERNEL HIGH XC"},
	{0x04000000, (2 * 1024 * 1024), "VRAM"},
	{0x44000000, (2 * 1024 * 1024), "VRAM XC"},
	{0x00010000, (16 * 1024), "SCRATCHPAD"},
	{0x40010000, (16 * 1024), "SCRATCHPAD XC"},
	{0xBFC00000, (1 * 1024 * 1024), "INTERNAL"},
};

const char *ptrDesc(void *ptr) {
	u32 p = (u32) ptr;

	if (p == 0) {
		return "NULL";
	} else if (p == 0xDEADBEEF) {
		return "DEADBEEF";
	}

	int i;
	for (i = 0; i < sizeof(g_memareas) / sizeof(g_memareas[0]); ++i) {
		if (p >= g_memareas[i].start && p < g_memareas[i].start + g_memareas[i].size) {
			return g_memareas[i].name;
		}
	}

	return "UNKNOWN";
}

inline void schedfThreadStatus(SceUID thread) {
	SceKernelThreadInfo info;
	info.size = sizeof(info);

	int result = sceKernelReferThreadStatus(thread, &info);
	if (result >= 0) {
		schedf("OK (size=%d, name=%s, attr=%08x, status=%d, entry=%s, stack=%s, stackSize=%x,\n", info.size, info.name, info.attr, info.status, ptrDesc(info.entry), ptrDesc(info.stack), info.stackSize);
		schedf("        gpReg=%s, initPrio=%x, currPrio=%x, waitType=%d, waitId=%d, exit=%08X\n", ptrDesc(info.gpReg), info.initPriority, info.currentPriority, info.waitType, info.waitId, info.exitStatus);
		schedf("        run=%lld, intrPreempt=%u, threadPreempt=%u, release=%u\n", *(u64 *) &info.runClocks, info.intrPreemptCount, info.threadPreemptCount, info.releaseCount);
	} else {
		schedf("Invalid (%08X)\n", result);
	}
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
	schedulingPlacement = 1; \
	schedf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(int), &sema1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteSema(sema1); \
	SCHED_LOG(F, 1); \
	\
	schedf(" (thread=%08X, main=%08X)\n", schedulingResult, result); \
	sceKernelTerminateThread(thread); \
}
