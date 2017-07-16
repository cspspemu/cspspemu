#include "shared.h"
#include <limits.h>

// Test sceKernelLockMutex with an exising mutex.
#define LOCK_TEST_SIMPLE(title, mutex, count) { \
	int result = sceKernelLockMutex(mutex, count, NULL); \
	if (result == 0) { \
		schedf("%s: ", title); \
		schedfMutex(mutex); \
	} else { \
		schedf("%s: Failed (%X)\n", title, result); \
	} \
}

// Test sceKernelLockMutex with NULL timeout.
#define LOCK_TEST(title, attr, initial, count) { \
	SceUID mutex = sceKernelCreateMutex("lock", attr, initial, NULL); \
	LOCK_TEST_SIMPLE(title, mutex, count); \
	sceKernelDeleteMutex(mutex); \
}

// Test sceKernelLockMutex with a timeout.
#define LOCK_TEST_TIMEOUT(title, attr, initial, count, initial_timeout) { \
	SceUID mutex = sceKernelCreateMutex("lock", attr, initial, NULL); \
	SceUInt timeout = initial_timeout; \
	int result = sceKernelLockMutex(mutex, count, &timeout); \
	if (result == 0) { \
		schedf("%s: OK (%dms left)\n", title, timeout); \
		schedfMutex(mutex); \
	} else { \
		schedf("%s: Failed (%X, %dms left)\n", title, result, timeout); \
	} \
	sceKernelDeleteMutex(mutex); \
}

// Test sceKernelLockMutex by locking on a separate thread first.
#define LOCK_TEST_TIMEOUT_THREAD(title, attr, initial, count, initial_timeout) { \
	schedf("%s: ", title); \
	schedulingResult = -1; \
	SceUID mutex = sceKernelCreateMutex("lock", attr, initial, NULL); \
	sceKernelStartThread(lockThread, sizeof(int), &mutex); \
	sceKernelDelayThread(400); \
	SceUInt timeout = initial_timeout; \
	int result = sceKernelLockMutex(mutex, count, &timeout); \
	schedf("L2 "); \
	sceKernelDelayThread(600); \
	sceKernelDeleteMutex(mutex); \
	sceKernelWaitThreadEnd(lockThread, NULL); \
	if (result == 0) { \
		schedf("OK (thread=%08X, %dms left)\n", schedulingResult, timeout); \
	} else { \
		schedf("Failed (thread=%08X, main=%08X, %dms left)\n", schedulingResult, result, timeout); \
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
	int result = sceKernelLockMutex(*(int*) argPointer, 1, NULL);
	schedf("After delete: %08X\n", result);
	return 0;
}

static int exitFunc(SceSize argSize, void* argPointer) {
	int result = sceKernelLockMutex(*(int*) argPointer, 1, NULL);
	schedf("Now exiting: %08X\n", result);
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

	LOCK_TEST_TIMEOUT("Lock 0 => 0", PSP_MUTEX_ATTR_FIFO, 0, 0, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => 1", PSP_MUTEX_ATTR_FIFO, 0, 1, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => 2", PSP_MUTEX_ATTR_FIFO, 0, 2, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => -1", PSP_MUTEX_ATTR_FIFO, 0, -1, 500);
	LOCK_TEST_TIMEOUT("Lock 1 => 1", PSP_MUTEX_ATTR_FIFO, 1, 1, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => 2 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 2, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => -1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, -1, 500);
	LOCK_TEST_TIMEOUT("Lock 1 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, 1, 500);
	LOCK_TEST_TIMEOUT("Zero timeout", PSP_MUTEX_ATTR_FIFO, 0, 1, 0);

	SceUID lockThread = CREATE_SIMPLE_THREAD(lockFunc);
	LOCK_TEST_TIMEOUT_THREAD("Locked 1 => 1", PSP_MUTEX_ATTR_FIFO, 1, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked 0 => 1", PSP_MUTEX_ATTR_FIFO, 0, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked 1 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked 0 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked zero timeout", PSP_MUTEX_ATTR_FIFO, 0, 1, 0);

	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	SceUID mutex = sceKernelCreateMutex("lock", 0, 1, NULL);
	sceKernelStartThread(deleteThread, sizeof(int), &mutex);
	sceKernelDelayThread(500);
	sceKernelDeleteMutex(mutex);

	LOCK_TEST_SIMPLE("NULL => 0", 0, 0);
	LOCK_TEST_SIMPLE("NULL => 1", 0, 1);
	LOCK_TEST_SIMPLE("Invalid => 1", 0xDEADBEEF, 1);
	LOCK_TEST_SIMPLE("Deleted => 1", mutex, 1);
	
	BASIC_SCHED_TEST("NULL",
		result = sceKernelLockMutex(0, 1, NULL);
	);
	BASIC_SCHED_TEST("Zero",
		result = sceKernelLockMutex(mutex2, 0, NULL);
	);
	BASIC_SCHED_TEST("Lock same",
		result = sceKernelLockMutex(mutex1, 1, NULL);
	);
	BASIC_SCHED_TEST("Lock other",
		result = sceKernelLockMutex(mutex2, 1, NULL);
	);
	BASIC_SCHED_TEST("Lock same with timeout",
		SceUInt timeout = 100;
		result = sceKernelLockMutex(mutex1, 1, &timeout);
	);
	BASIC_SCHED_TEST("Lock other with timeout",
		SceUInt timeout = 100;
		result = sceKernelLockMutex(mutex2, 1, &timeout);
	);

	SceUID exitThread = CREATE_SIMPLE_THREAD(exitFunc);
	mutex = sceKernelCreateMutex("lock", 0, 0, NULL);
	sceKernelStartThread(exitThread, sizeof(int), &mutex);
	sceKernelDelayThread(500);
	sceKernelLockMutex(mutex, 1, NULL);
	sceKernelDeleteMutex(mutex);
	schedf("Woke up after other thread exited.\n");

	return 0;
}