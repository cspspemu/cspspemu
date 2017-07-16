#include "shared.h"

#define CANCEL_TEST(title, mutex, count) { \
	int result = sceKernelCancelMutex(mutex, count, NULL); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
	printfMutex(mutex); \
}

#define CANCEL_TEST_WITH_WAIT(title, mutex, count) { \
	int waitThreads = 99; \
	int result = sceKernelCancelMutex(mutex, count, &waitThreads); \
	if (result == 0) { \
		printf("%s: OK (%d waiting)\n", title, waitThreads); \
	} else { \
		printf("%s: Failed (%X, %d waiting)\n", title, result, waitThreads); \
	} \
	printfMutex(mutex); \
}

int main(int argc, char **argv) {
	SceUID mutex = sceKernelCreateMutex("cancel", 0, 0, NULL);

	CANCEL_TEST("Normal", mutex, 1);
	CANCEL_TEST("Greater than max", mutex, 3);
	CANCEL_TEST("Zero", mutex, 0);
	CANCEL_TEST("Negative -3", mutex, -3);
	CANCEL_TEST("Negative -1", mutex, -1);

	CANCEL_TEST_WITH_WAIT("Normal", mutex, 1);
	CANCEL_TEST_WITH_WAIT("Greater than max", mutex, 3);
	CANCEL_TEST_WITH_WAIT("Zero", mutex, 0);
	CANCEL_TEST_WITH_WAIT("Negative -3", mutex, -3);
	CANCEL_TEST_WITH_WAIT("Negative -1", mutex, -1);

	sceKernelDeleteMutex(mutex);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelCancelMutex(0, 0, NULL);
	);
	BASIC_SCHED_TEST("Cancel bad count",
		result = sceKernelCancelMutex(mutex2, 0, NULL);
	);
	BASIC_SCHED_TEST("Cancel other",
		result = sceKernelCancelMutex(mutex2, 1, NULL);
	);
	BASIC_SCHED_TEST("Cancel waited",
		result = sceKernelCancelMutex(mutex1, 1, NULL);
	);
	BASIC_SCHED_TEST("Cancel other with threads",
		int n;
		result = sceKernelCancelMutex(mutex2, 1, &n);
		schedf(" (wait: %d) ", n);
	);
	BASIC_SCHED_TEST("Cancel waited with threads",
		int n;
		result = sceKernelCancelMutex(mutex1, 1, &n);
		schedf(" (wait: %d) ", n);
	);

	CANCEL_TEST("NULL", 0, 0);
	CANCEL_TEST("Invalid", 0xDEADBEEF, 0);
	CANCEL_TEST("Deleted", mutex, 0);

	return 0;
}