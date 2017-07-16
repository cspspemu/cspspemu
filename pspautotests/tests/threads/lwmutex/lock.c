#include "shared.h"
#include <limits.h>

// Test sceKernelLockLwMutex with an exising mutex.
#define LOCK_TEST_SIMPLE(title, workareaPtr, count) { \
	int result = sceKernelLockLwMutex(workareaPtr, count, NULL); \
	if (result == 0) { \
		schedf("%s: OK\n", title); \
	} else { \
		schedf("%s: Failed (%X)\n", title, result); \
	} \
}

// Test sceKernelLockLwMutex with NULL timeout.
#define LOCK_TEST(title, attr, initial, count) { \
	SceLwMutexWorkarea workarea; \
	int result = sceKernelCreateLwMutex(&workarea, "lock", attr, initial, NULL); \
	if (result == 0) { \
		LOCK_TEST_SIMPLE(title, &workarea, count); \
	} else { \
		schedf("%s: Failed (%X)\n", title, result); \
	} \
	schedfLwMutex(&workarea); \
	sceKernelDeleteLwMutex(&workarea); \
	FAKE_LWMUTEX(workarea, attr, initial); \
	LOCK_TEST_SIMPLE(title " (fake)", &workarea, count); \
	schedfLwMutex(&workarea); \
}

// Test sceKernelLockLwMutex with a timeout.
#define LOCK_TEST_TIMEOUT(title, attr, initial, count, initial_timeout) { \
	SceLwMutexWorkarea workarea; \
	sceKernelCreateLwMutex(&workarea, "lock", attr, initial, NULL); \
	SceUInt timeout = initial_timeout; \
	int result = sceKernelLockLwMutex(&workarea, count, &timeout); \
	if (result == 0) { \
		schedf("%s: OK (%dms left)\n", title, timeout); \
	} else { \
		schedf("%s: Failed (%X, %dms left)\n", title, result, timeout); \
	} \
	schedfLwMutex(&workarea); \
	sceKernelDeleteLwMutex(&workarea); \
	FAKE_LWMUTEX(workarea, attr, initial); \
	timeout = initial_timeout; \
	result = sceKernelLockLwMutex(&workarea, count, &timeout); \
	if (result == 0) { \
		schedf("%s (fake): OK (%dms left)\n", title, timeout); \
	} else { \
		schedf("%s (fake): Failed (%X, %dms left)\n", title, result, timeout); \
	} \
	schedfLwMutex(&workarea); \
}

// Test sceKernelLockLwMutex by locking on a separate thread first.
#define LOCK_TEST_TIMEOUT_THREAD(title, attr, initial, count, initial_timeout) { \
	flushschedf(); \
	schedf("%s: ", title); \
	schedulingResult = -1; \
	SceLwMutexWorkarea workarea; \
	sceKernelCreateLwMutex(&workarea, "lock", attr, initial, NULL); \
	void *workareaPtr = &workarea; \
	sceKernelStartThread(lockThread, sizeof(void*), &workareaPtr); \
	sceKernelDelayThread(400); \
	SceUInt timeout = initial_timeout; \
	int result = sceKernelLockLwMutex(&workarea, count, &timeout); \
	schedf("L2 "); \
	sceKernelDelayThread(600); \
	sceKernelDeleteLwMutex(&workarea); \
	sceKernelWaitThreadEnd(lockThread, NULL); \
	flushschedf(); \
	if (result == 0) { \
		schedf("OK (thread=%08X, %dms left)\n", schedulingResult, timeout); \
	} else { \
		schedf("Failed (thread=%08X, main=%08X, %dms left)\n", schedulingResult, result, timeout); \
	} \
	sceKernelTerminateThread(lockThread); \
	\
	FAKE_LWMUTEX(workarea, attr, initial); \
	schedf("%s (fake): ", title); \
	flushschedf(); \
	schedulingResult = -1; \
	sceKernelStartThread(lockThread, sizeof(void*), &workareaPtr); \
	sceKernelDelayThread(400); \
	timeout = initial_timeout; \
	result = sceKernelLockLwMutex(&workarea, count, &timeout); \
	schedf("L2 "); \
	sceKernelDelayThread(600); \
	sceKernelWaitThreadEnd(lockThread, NULL); \
	flushschedf(); \
	if (result == 0) { \
		schedf("OK (thread=%08X)\n", schedulingResult); \
	} else { \
		schedf("Failed (thread=%08X, main=%08X)\n", schedulingResult, result); \
	} \
	sceKernelTerminateThread(lockThread); \
}

static int lockFunc(SceSize argSize, void* argPointer) {
	SceUInt timeout = 1000;
	schedulingResult = sceKernelLockLwMutex(*(void**) argPointer, 1, &timeout);
	schedf("L1 "); \
	sceKernelDelayThread(1000);
	if (schedulingResult == 0)
		sceKernelUnlockLwMutex(*(void**) argPointer, 1);
	return 0;
}

static int deleteMeFunc(SceSize argSize, void* argPointer) {
	int result = sceKernelLockLwMutex(*(void**) argPointer, 1, NULL);
	schedf("After delete: %08X\n", result);
	return 0;
}

static int exitFunc(SceSize argSize, void* argPointer) {
	int result = sceKernelLockLwMutex(*(void**) argPointer, 1, NULL);
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
	flushschedf();

	LOCK_TEST_TIMEOUT("Lock 0 => 0", PSP_MUTEX_ATTR_FIFO, 0, 0, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => 1", PSP_MUTEX_ATTR_FIFO, 0, 1, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => 2", PSP_MUTEX_ATTR_FIFO, 0, 2, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => -1", PSP_MUTEX_ATTR_FIFO, 0, -1, 500);
	LOCK_TEST_TIMEOUT("Lock 1 => 1", PSP_MUTEX_ATTR_FIFO, 1, 1, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => 2 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 2, 500);
	LOCK_TEST_TIMEOUT("Lock 0 => -1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, -1, 500);
	LOCK_TEST_TIMEOUT("Lock 1 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, 1, 500);
	LOCK_TEST_TIMEOUT("Zero timeout", PSP_MUTEX_ATTR_FIFO, 0, 1, 0);
	flushschedf();

	SceUID lockThread = CREATE_SIMPLE_THREAD(lockFunc);
	LOCK_TEST_TIMEOUT_THREAD("Locked 1 => 1", PSP_MUTEX_ATTR_FIFO, 1, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked 0 => 1", PSP_MUTEX_ATTR_FIFO, 0, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked 1 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 1, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked 0 => 1 (recursive)", PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 1, 500);
	LOCK_TEST_TIMEOUT_THREAD("Locked zero timeout", PSP_MUTEX_ATTR_FIFO, 0, 1, 0);
	flushschedf();

	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	SceLwMutexWorkarea workarea;
	void *workareaPtr = &workarea;
	sceKernelCreateLwMutex(&workarea, "lock", 0, 1, NULL);
	sceKernelStartThread(deleteThread, sizeof(void*), &workareaPtr);
	sceKernelDelayThread(500);
	sceKernelDeleteLwMutex(&workarea);
	flushschedf();

	// Crash.
	//LOCK_TEST_SIMPLE("NULL => 0", 0, 0);
	//LOCK_TEST_SIMPLE("NULL => 1", 0, 1);
	//LOCK_TEST_SIMPLE("Invalid => 1", (void*) 0xDEADBEEF, 1);
	LOCK_TEST_SIMPLE("Deleted => 1", &workarea, 1);

	BASIC_SCHED_TEST("Zero",
		result = sceKernelLockLwMutex(&workarea2, 0, NULL);
	);
	BASIC_SCHED_TEST("Lock same",
		result = sceKernelLockLwMutex(&workarea1, 1, NULL);
	);
	BASIC_SCHED_TEST("Lock other",
		result = sceKernelLockLwMutex(&workarea2, 1, NULL);
	);
	BASIC_SCHED_TEST("Lock same with timeout",
		SceUInt timeout = 100;
		result = sceKernelLockLwMutex(&workarea1, 1, &timeout);
	);
	BASIC_SCHED_TEST("Lock other with timeout",
		SceUInt timeout = 100;
		result = sceKernelLockLwMutex(&workarea2, 1, &timeout);
	);

	SceLwMutexWorkarea workarea1, workarea2;
	sceKernelCreateLwMutex(&workarea1, "lock", 0, 0, NULL);
	memcpy(&workarea2, &workarea1, sizeof(SceLwMutexWorkarea));
	LOCK_TEST_SIMPLE("Lock copy #1", &workarea1, 1);
	LOCK_TEST_SIMPLE("Lock copy #2", &workarea2, 1);
	sceKernelDeleteLwMutex(&workarea1);

	SceUID exitThread = CREATE_SIMPLE_THREAD(exitFunc);
	sceKernelCreateLwMutex(&workarea, "lock", 0, 0, NULL);
	sceKernelStartThread(exitThread, sizeof(void*), &workareaPtr);
	sceKernelDelayThread(500);
	SceUInt timeout = 50000;
	sceKernelLockLwMutex(&workarea, 1, &timeout);
	sceKernelDeleteLwMutex(&workarea);
	if (timeout == 0)
		schedf("Did not wake on thread exit: OK\n");
	else
		schedf("Did not wake on thread exit: Failed\n");

	return 0;
}