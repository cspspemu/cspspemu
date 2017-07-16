#include "shared.h"

SETUP_SCHED_TEST;

#define WAIT_TEST_SIMPLE(title, sema, count) { \
	int result = sceKernelWaitSema(sema, count, NULL); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
	PRINT_SEMAPHORE(sema); \
}

#define WAIT_TEST_SIMPLE_TIMEOUT(title, sema, count, initial_timeout) { \
	SceUInt timeout = initial_timeout; \
	int result = sceKernelWaitSema(sema, count, &timeout); \
	if (result == 0) { \
		printf("%s: OK (%dms left)\n", title, timeout); \
	} else { \
		printf("%s: Failed (%X, %dms left)\n", title, result, timeout); \
	} \
	PRINT_SEMAPHORE(sema); \
}

static int waitTestFunc(SceSize argSize, void* argPointer) {
	sceKernelDelayThread(1000);
	SCHED_LOG(C, 2);
	schedulingResult = sceKernelSignalSema(*(int*) argPointer, 1);
	SCHED_LOG(D, 2);
	return 0;
}

static int deleteMeFunc(SceSize argSize, void* argPointer) {
	int result = sceKernelWaitSema(*(int*) argPointer, 1, NULL);
	schedf("After delete: %08X\n", result);
	return 0;
}

int main(int argc, char **argv) {
	SceUID sema = sceKernelCreateSema("wait1", 0, 1, 1, NULL);

	WAIT_TEST_SIMPLE("Signaled", sema, 1);
	WAIT_TEST_SIMPLE("Greater than max", sema, 100);
	WAIT_TEST_SIMPLE("Negative", sema, -1);

	sceKernelSignalSema(sema, 1);
	WAIT_TEST_SIMPLE_TIMEOUT("Signaled", sema, 1, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("Never signaled", sema, 1, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("Greater than max", sema, 100, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("Zero", sema, 0, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("Negative", sema, -1, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("Zero timeout", sema, 1, 0);
	
	// Signaled off thread.
	printf("Wait timeout: ");
	schedulingPlacement = 1;
	SCHED_LOG(A, 1);
	SceUInt timeout = 5000;
	SceUID thread = sceKernelCreateThread("waitTest", (void *)&waitTestFunc, 0x12, 0x10000, 0, NULL);
	sceKernelStartThread(thread, sizeof(int), &sema);
	SCHED_LOG(B, 1);
	int result = sceKernelWaitSema(sema, 1, &timeout);
	SCHED_LOG(E, 1);
	schedf(" (thread=%08X, main=%08X, remaining=%d)\n", schedulingResult, result, timeout / 1000);
	flushschedf();

	sceKernelDeleteSema(sema);

	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	sema = sceKernelCreateSema("wait1", 0, 0, 1, NULL);
	sceKernelStartThread(deleteThread, sizeof(int), &sema);
	sceKernelDelayThread(500);
	sceKernelDeleteSema(sema);
	flushschedf();

	WAIT_TEST_SIMPLE("NULL", 0, 1);
	WAIT_TEST_SIMPLE("Invalid", 0xDEADBEEF, 1);
	WAIT_TEST_SIMPLE("Deleted", sema, 1);
	
	BASIC_SCHED_TEST("NULL",
		result = sceKernelWaitSema(0, 1, NULL);
	);
	BASIC_SCHED_TEST("Greater than max",
		result = sceKernelWaitSema(sema2, 2, NULL);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelWaitSema(sema2, 1, NULL);
	);
	BASIC_SCHED_TEST("Other timeout",
		timeout = 100;
		result = sceKernelWaitSema(sema2, 1, &timeout);
	);
	BASIC_SCHED_TEST("Same timeout",
		timeout = 100;
		result = sceKernelWaitSema(sema1, 1, &timeout);
	);

	return 0;
}