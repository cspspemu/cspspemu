#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define CANCEL_TEST(title, flag, pattern) { \
	int result = sceKernelCancelEventFlag(flag, pattern, NULL); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
	PRINT_FLAG(flag); \
}

#define CANCEL_TEST_WITH_WAIT(title, flag, pattern) { \
	int waitThreads = 99; \
	int result = sceKernelCancelEventFlag(flag, pattern, &waitThreads); \
	if (result == 0) { \
		printf("%s: OK (%d waiting)\n", title, waitThreads); \
	} else { \
		printf("%s: Failed (%X, %d waiting)\n", title, result, waitThreads); \
	} \
	PRINT_FLAG(flag); \
}

void foo() {
}

int main(int argc, char **argv) {
	SceUID flag = sceKernelCreateEventFlag("cancel", 0, 0, NULL);
	SceKernelEventFlagInfo flaginfo;

	// Something weird is happening: first time status is garbled?
	sceKernelReferEventFlagStatus(flag, &flaginfo);

	CANCEL_TEST("To all set", flag, 0xFFFFFFFF);
	CANCEL_TEST("To all clear", flag, 0x00000000);
	CANCEL_TEST("Some pattern", flag, 0xABCDEF45);

	CANCEL_TEST_WITH_WAIT("To all set", flag, 0xFFFFFFFF);
	CANCEL_TEST_WITH_WAIT("To all clear", flag, 0x00000000);
	CANCEL_TEST_WITH_WAIT("Some pattern", flag, 0xABCDEF45);

	sceKernelDeleteEventFlag(flag);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelCancelEventFlag(0, 0x00000000, NULL);
	);
	BASIC_SCHED_TEST("Other unset",
		result = sceKernelCancelEventFlag(flag2, 0x00000000, NULL);
	);
	BASIC_SCHED_TEST("Other set",
		result = sceKernelCancelEventFlag(flag2, 0xFFFFFFFF, NULL);
	);
	BASIC_SCHED_TEST("Same unset",
		result = sceKernelCancelEventFlag(flag1, 0x00000000, NULL);
	);
	BASIC_SCHED_TEST("Same set",
		result = sceKernelCancelEventFlag(flag1, 0xFFFFFFFF, NULL);
	);
	BASIC_SCHED_TEST("Cancel other with threads",
		int n;
		result = sceKernelCancelEventFlag(flag2, 0xFFFFFFFF, &n);
	);
	BASIC_SCHED_TEST("Cancel waited with threads",
		int n;
		result = sceKernelCancelEventFlag(flag1, 0xFFFFFFFF, &n);
	);

	CANCEL_TEST("NULL", 0, 0);
	CANCEL_TEST("Invalid", 0xDEADBEEF, 0);
	CANCEL_TEST("Deleted", flag, 0);

	return 0;
}