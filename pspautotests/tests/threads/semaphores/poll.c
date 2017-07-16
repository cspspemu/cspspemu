#include "shared.h"

SETUP_SCHED_TEST;

#define POLL_TEST(title, sema, count) { \
	int result = sceKernelPollSema(sema, count); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
}

int main(int argc, char **argv) {
	SceUID sema = sceKernelCreateSema("poll1", 0, 1, 1, NULL);

	POLL_TEST("Signaled", sema, 1);
	POLL_TEST("Not signaled", sema, 1);
	POLL_TEST("Zero (while not signaled)", sema, 1);
	sceKernelSignalSema(sema, 1);
	POLL_TEST("Zero (while signaled)", sema, 0);
	POLL_TEST("Negative", sema, -1);

	sceKernelDeleteSema(sema);

	POLL_TEST("NULL", 0, 1);
	POLL_TEST("Invalid", 0xDEADBEEF, 1);
	POLL_TEST("Deleted", sema, 1);
	
	BASIC_SCHED_TEST("NULL",
		result = sceKernelPollSema(NULL, 0);
	);
	BASIC_SCHED_TEST("Zero other",
		result = sceKernelPollSema(sema2, 0);
	);
	BASIC_SCHED_TEST("Zero same",
		result = sceKernelPollSema(sema1, 0);
	);
	BASIC_SCHED_TEST("Poll other",
		result = sceKernelPollSema(sema2, 1);
	);
	BASIC_SCHED_TEST("Poll same",
		result = sceKernelPollSema(sema1, 1);
	);

	return 0;
}