#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define CANCEL_TEST(title, mbx) { \
	int result = sceKernelCancelReceiveMbx(mbx, NULL); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
	PRINT_MBX(mbx); \
}

#define CANCEL_TEST_WITH_WAIT(title, mbx) { \
	int waitThreads = 99; \
	int result = sceKernelCancelReceiveMbx(mbx, &waitThreads); \
	if (result == 0) { \
		printf("%s: OK (%d waiting)\n", title, waitThreads); \
	} else { \
		printf("%s: Failed (%X, %d waiting)\n", title, result, waitThreads); \
	} \
	PRINT_MBX(mbx); \
}

int main(int argc, char **argv) {
	SceUID mbx = sceKernelCreateMbx("cancel", 0, NULL);

	CANCEL_TEST("Without any messages", mbx);
	sendMbx(mbx, 0x15);
	CANCEL_TEST("With messages", mbx);

	CANCEL_TEST_WITH_WAIT("Without any messages", mbx);
	sendMbx(mbx, 0x15);
	CANCEL_TEST_WITH_WAIT("With messages", mbx);

	sceKernelDeleteMbx(mbx);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelCancelReceiveMbx(0, NULL);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelCancelReceiveMbx(mbx2, NULL);
	);
	BASIC_SCHED_TEST("Same",
		result = sceKernelCancelReceiveMbx(mbx1, NULL);
	);
	BASIC_SCHED_TEST("Cancel other with threads",
		int n;
		result = sceKernelCancelReceiveMbx(mbx2, &n);
	);
	BASIC_SCHED_TEST("Cancel waited with threads",
		int n;
		result = sceKernelCancelReceiveMbx(mbx1, &n);
	);

	mbx = sceKernelCreateMbx("cancel", 0, NULL);
	sendMbx(mbx, 0x15);
	BASIC_SCHED_TEST("Other with messages",
		result = sceKernelCancelReceiveMbx(mbx2, NULL);
	);
	sceKernelDeleteMbx(mbx);

	CANCEL_TEST("NULL", 0);
	CANCEL_TEST("Invalid", 0xDEADBEEF);
	CANCEL_TEST("Deleted", mbx);

	return 0;
}