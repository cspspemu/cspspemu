#include "shared.h"
#include <limits.h>

#define LOCK_TEST_SIMPLE(title, mutex, count) { \
	int result = sceKernelTryLockMutex(mutex, count); \
	if (result == 0) { \
		schedf("%s: ", title); \
		schedfMutex(mutex); \
	} else { \
		schedf("%s: Failed (%X)\n", title, result); \
	} \
}

#define LOCK_TEST(title, attr, initial, count) { \
	SceUID mutex = sceKernelCreateMutex("lock", attr, initial, NULL); \
	LOCK_TEST_SIMPLE(title, mutex, count); \
	sceKernelDeleteMutex(mutex); \
}

#define LOCK_TEST_THREAD(title, attr, initial, count) { \
	schedf("%s: ", title); \
	schedulingResult = -1; \
	SceUID mutex = sceKernelCreateMutex("lock", attr, initial, NULL); \
	sceKernelStartThread(lockThread, sizeof(int), &mutex); \
	sceKernelDelayThread(500); \
	int result = sceKernelTryLockMutex(mutex, count); \
	schedf("L2 "); \
	sceKernelDelayThread(500); \
	sceKernelDeleteMutex(mutex); \
	sceKernelWaitThreadEnd(lockThread, NULL); \
	if (result == 0) { \
		schedf("OK (thread=%08X)\n", schedulingResult); \
	} else { \
		schedf("Failed (thread=%08X, main=%08X)\n", schedulingResult, result); \
	} \
	sceKernelTerminateThread(lockThread); \
}

static int lockFunc(SceSize argSize, void* argPointer) {
	SceUInt timeout = 1000;
	schedulingResult = sceKernelLockMutex(*(int*) argPointer, 1, &timeout);
	schedf("L1 ");
	sceKernelDelayThread(1000);
	return 0;
}

static int deleteMeFunc(SceSize argSize, void* argPointer) {
	int result = sceKernelTryLockMutex(*(int*) argPointer, 1);
	schedf("After delete: %08X\n", result);
	return 0;
}

int main(int argc, char **argv) {
	LOCK_TEST("Lock 0 => 0", PSP_MUTEX_ATTR_FIFO, 0, 0);
	LOCK_TEST("Lock 0 => 1", PSP_MUTEX_ATTR_FIFO, 0, 1);
	LOCK_TEST("Lock 0 => 2", PSP_MUTEX_ATTR_FIFO, 0, 2);
	LOCK_TEST("Lock 0 => -1", PSP_MUTEX_ATTR_FIFO, 0, -1);
	LOCK_TEST("Lock 1 => 1", PSP_MUTEX_ATTR_FIFO, 1, 1);
	LOCK_TEST("Lock 0 => 2 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 2);
	LOCK_TEST("Lock 0 => -1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, -1);
	LOCK_TEST("Lock 1 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, 1);
	LOCK_TEST("Lock 1 => INT_MAX - 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, INT_MAX - 1);
	LOCK_TEST("Lock 1 => INT_MAX (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, INT_MAX);
	LOCK_TEST("Lock INT_MAX => INT_MAX (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, INT_MAX, INT_MAX);

	SceUID lockThread = CREATE_SIMPLE_THREAD(lockFunc);
	LOCK_TEST_THREAD("Locked 1 => 1", PSP_MUTEX_ATTR_FIFO, 1, 1);
	LOCK_TEST_THREAD("Locked 0 => 1", PSP_MUTEX_ATTR_FIFO, 0, 1);
	LOCK_TEST_THREAD("Locked 1 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, 1);
	LOCK_TEST_THREAD("Locked 0 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 1);

	// Probably we can't manage to delete it at the same time.
	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	SceUID mutex = sceKernelCreateMutex("lock", 0, 1, NULL);
	sceKernelStartThread(deleteThread, sizeof(int), &mutex);
	sceKernelDeleteMutex(mutex);

	LOCK_TEST_SIMPLE("NULL => 0", 0, 0);
	LOCK_TEST_SIMPLE("NULL => 1", 0, 1);
	LOCK_TEST_SIMPLE("Invalid => 1", 0xDEADBEEF, 1);
	LOCK_TEST_SIMPLE("Deleted => 1", mutex, 1);
	
	BASIC_SCHED_TEST("NULL",
		result = sceKernelTryLockMutex(0, 1);
	);
	BASIC_SCHED_TEST("Zero",
		result = sceKernelTryLockMutex(mutex2, 0);
	);
	BASIC_SCHED_TEST("Lock same",
		result = sceKernelTryLockMutex(mutex1, 1);
	);
	BASIC_SCHED_TEST("Lock other",
		result = sceKernelTryLockMutex(mutex2, 1);
	);

	return 0;
}