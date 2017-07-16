#include "shared.h"

SETUP_SCHED_TEST;

#define SIGNAL_TEST(title, sema, count) { \
	int result = sceKernelSignalSema(sema, count); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
	PRINT_SEMAPHORE(sema); \
}

int main(int argc, char **argv) {
	SceUID sema = sceKernelCreateSema("signal", 0, 0, 1, NULL);
	PRINT_SEMAPHORE(sema);

	SIGNAL_TEST("Basic +2", sema, 2);
	SIGNAL_TEST("Basic +1", sema, 1);
	SIGNAL_TEST("Negative - 1", sema, -1);
	SIGNAL_TEST("Negative - 2", sema, -2);
	SIGNAL_TEST("Zero", sema, 0);

	sceKernelDeleteSema(sema);

	sema = sceKernelCreateSema("signal", 0, -3, 3, NULL);
	PRINT_SEMAPHORE(sema);
	SIGNAL_TEST("Start negative", sema, 1);
	sceKernelDeleteSema(sema);

	SIGNAL_TEST("NULL", 0, 1);
	SIGNAL_TEST("Invalid", 0xDEADBEEF, 1);
	SIGNAL_TEST("Deleted", sema, 1);

	TWO_STEP_SCHED_TEST("Signal other then same", 0, 0,
		result = sceKernelSignalSema(sema2, 1);
	,
		result = sceKernelSignalSema(sema1, 1);
	);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelSignalSema(0, 0);
	);
	BASIC_SCHED_TEST("Other + 2",
		result = sceKernelSignalSema(sema2, 2);
	);
	BASIC_SCHED_TEST("Other + 1",
		result = sceKernelSignalSema(sema2, 1);
	);
	BASIC_SCHED_TEST("Other - 1",
		result = sceKernelSignalSema(sema2, -1);
	);
	BASIC_SCHED_TEST("Same + 2",
		result = sceKernelSignalSema(sema1, 2);
	);
	BASIC_SCHED_TEST("Same + 1",
		result = sceKernelSignalSema(sema1, 1);
	);
	BASIC_SCHED_TEST("Same - 1",
		result = sceKernelSignalSema(sema1, -1);
	);

	return 0;
}