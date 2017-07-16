#include "shared.h"

SETUP_SCHED_TEST;

#define CANCEL_TEST(title, sema, count) { \
	int result = sceKernelCancelSema(sema, count, NULL); \
	if (result == 0) { \
		schedf("%s: OK\n", title); \
	} else { \
		schedf("%s: Failed (%X)\n", title, result); \
	} \
	PRINT_SEMAPHORE(sema); \
}

#define CANCEL_TEST_WITH_WAIT(title, sema, count) { \
	int waitThreads = 99; \
	int result = sceKernelCancelSema(sema, count, &waitThreads); \
	if (result == 0) { \
		schedf("%s: OK (%d waiting)\n", title, waitThreads); \
	} else { \
		schedf("%s: Failed (%X, %d waiting)\n", title, result, waitThreads); \
	} \
	PRINT_SEMAPHORE(sema); \
}

void foo() {
}

int main(int argc, char **argv) {
	SceUID sema = sceKernelCreateSema("cancel", 0, 0, 1, NULL);

	CANCEL_TEST("Normal", sema, 1);
	CANCEL_TEST("Greater than max", sema, 3);
	CANCEL_TEST("Zero", sema, 0);
	CANCEL_TEST("Negative -3", sema, -3);
	CANCEL_TEST("Negative -1", sema, -1);

	CANCEL_TEST_WITH_WAIT("Normal", sema, 1);
	CANCEL_TEST_WITH_WAIT("Greater than max", sema, 3);
	CANCEL_TEST_WITH_WAIT("Zero", sema, 0);
	CANCEL_TEST_WITH_WAIT("Negative -3", sema, -3);
	CANCEL_TEST_WITH_WAIT("Negative -1", sema, -1);

	sceKernelDeleteSema(sema);

	TWO_STEP_SCHED_TEST("Cancel waited 0 then 1", 0, 1,
		result = 0;
		CANCEL_TEST_WITH_WAIT("To 0", sema1, 0);
	,
		CANCEL_TEST_WITH_WAIT("To 1", sema1, 1);
	);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelCancelSema(0, 0, NULL);
	);
	BASIC_SCHED_TEST("Cancel bad count",
		result = sceKernelCancelSema(sema2, 0, NULL);
	);
	BASIC_SCHED_TEST("Cancel other",
		result = sceKernelCancelSema(sema2, 1, NULL);
	);
	BASIC_SCHED_TEST("Cancel waited",
		result = sceKernelCancelSema(sema1, 1, NULL);
	);
	BASIC_SCHED_TEST("Cancel other with threads",
		int n;
		result = sceKernelCancelSema(sema2, 1, &n);
	);
	BASIC_SCHED_TEST("Cancel waited with threads",
		int n;
		result = sceKernelCancelSema(sema1, 1, &n);
	);

	CANCEL_TEST("NULL", 0, 0);
	CANCEL_TEST("Invalid", 0xDEADBEEF, 0);
	CANCEL_TEST("Deleted", sema, 0);

	return 0;
}