#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define WAIT_TEST_SIMPLE(title, flag, bits, wait_type, get_bits) { \
	u32 result_bits = get_bits ? 0xDEADBEEF : 0; \
	int result = sceKernelWaitEventFlag(flag, bits, wait_type, get_bits ? &result_bits : NULL, NULL); \
	if (result == 0) { \
		printf("%s: OK (bits=%08X) ", title, (uint) result_bits); \
		PRINT_FLAG(flag); \
	} else { \
		printf("%s: Failed (%08X, bits=%08X)\n", title, result, (uint) result_bits); \
	} \
}

#define WAIT_TEST_SIMPLE_TIMEOUT(title, flag, bits, wait_type, initial_timeout) { \
	u32 result_bits = 0xDEADBEEF; \
	SceUInt timeout = initial_timeout; \
	int result = sceKernelWaitEventFlag(flag, bits, wait_type, &result_bits, &timeout); \
	if (result == 0) { \
		printf("%s: OK (%dms left, bits=%08X) ", title, timeout, (uint) result_bits); \
		PRINT_FLAG(flag); \
	} else { \
		printf("%s: Failed (%X, %dms left, bits=%08X)\n", title, result, timeout, (uint) result_bits); \
	} \
}

static int waitTestFunc(SceSize argSize, void* argPointer) {
	sceKernelDelayThread(1000);
	SCHED_LOG(C, 2);
	schedulingResult = sceKernelSetEventFlag(*(int*) argPointer, 0xFFFFFFFF);
	SCHED_LOG(D, 2);
	return 0;
}

static int deleteMeFunc(SceSize argSize, void* argPointer) {
	u32 result_bits = 0xDEADBEEF;
	int result = sceKernelWaitEventFlag(*(int*) argPointer, 0xFFFFFFFF, PSP_EVENT_WAITAND, &result_bits, NULL);
	printf("After delete: %08X (bits=%08X)\n", result, (uint) result_bits);
	return 0;
}

int main(int argc, char **argv) {
	SceUID flag = sceKernelCreateEventFlag("wait1", 0, 0xFFFFFFFF, NULL);
	SceKernelEventFlagInfo flaginfo;

	// Something weird is happening: first time status is garbled?
	sceKernelReferEventFlagStatus(flag, &flaginfo);

	WAIT_TEST_SIMPLE("And 0x00000000", flag, 0, PSP_EVENT_WAITAND, 1);
	WAIT_TEST_SIMPLE("Or 0x00000000", flag, 0, PSP_EVENT_WAITOR, 1);
	WAIT_TEST_SIMPLE("Clear 0x00000000", flag, 0, PSP_EVENT_WAITCLEAR, 1);
	WAIT_TEST_SIMPLE("Wrong (0x04) 0x00000000", flag, 0, 0x04, 1);
	WAIT_TEST_SIMPLE("Wrong (0xFF) 0x00000000", flag, 0, 0xFF, 1);

	WAIT_TEST_SIMPLE("And 0x00000001", flag, 1, PSP_EVENT_WAITAND, 1);
	WAIT_TEST_SIMPLE("Or 0x00000001", flag, 1, PSP_EVENT_WAITOR, 1);
	WAIT_TEST_SIMPLE("Clear 0x00000001", flag, 1, PSP_EVENT_WAITCLEAR, 1);
	WAIT_TEST_SIMPLE("Wrong (0x02) 0x00000001", flag, 1, 0x02, 1);
	WAIT_TEST_SIMPLE("Wrong (0x04) 0x00000001", flag, 1, 0x04, 1);
	WAIT_TEST_SIMPLE("Wrong (0x08) 0x00000001", flag, 1, 0x08, 1);
	WAIT_TEST_SIMPLE("Wrong (0x40) 0x00000001", flag, 1, 0x40, 1);
	WAIT_TEST_SIMPLE("Wrong (0x80) 0x00000001", flag, 1, 0x80, 1);
	WAIT_TEST_SIMPLE("Wrong (0xFF) 0x00000001", flag, 1, 0xFF, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	WAIT_TEST_SIMPLE("Clear/Or 0x00000000", flag, 0, PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITOR, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	WAIT_TEST_SIMPLE("Clear/Or 0x00000001", flag, 1, PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITOR, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	WAIT_TEST_SIMPLE("Clear/Or 0x00000001 (no out bits)", flag, 1, PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITOR, 0);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	WAIT_TEST_SIMPLE_TIMEOUT("0xFFFFFFFF & 0x00000000", flag, 0x00000000, PSP_EVENT_WAITAND, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("0xFFFFFFFF & 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("0xFFFFFFFF | 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR, 500);
	sceKernelClearEventFlag(flag, 0x0000FFFF);
	WAIT_TEST_SIMPLE_TIMEOUT("0x0000FFFF & 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("0x0000FFFF | 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("0x0000FFFF & 0xFFFFFFFF with clear", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND | PSP_EVENT_WAITCLEAR, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("0x0000FFFF | 0xFFFFFFFF with clear", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEAR, 500);
	sceKernelClearEventFlag(flag, 0x0000FFFF);
	WAIT_TEST_SIMPLE_TIMEOUT("0x0000FFFF & 0xFFFFFFFF with clear all", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND | PSP_EVENT_WAITCLEARALL, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("0x0000FFFF | 0xFFFFFFFF with clear all", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEARALL, 500);
	sceKernelClearEventFlag(flag, 0x00000000);
	WAIT_TEST_SIMPLE_TIMEOUT("0x00000000 & 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 500);
	WAIT_TEST_SIMPLE_TIMEOUT("Zero timeout", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 0);
	WAIT_TEST_SIMPLE_TIMEOUT("5ms timeout", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 5);

	// Set off thread.
	schedulingLogPos = 0;
	schedulingPlacement = 1;
	SCHED_LOG(A, 1);
	SceUInt timeout = 5000;
	SceUID thread = sceKernelCreateThread("waitTest", (void *)&waitTestFunc, 0x12, 0x10000, 0, NULL);
	sceKernelStartThread(thread, sizeof(flag), &flag);
	SCHED_LOG(B, 1);
	int result = sceKernelWaitEventFlag(flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	SCHED_LOG(E, 1);
	printf("Wait timeout: %s (thread=%08X, main=%08X, remaining=%d)\n", schedulingLog, schedulingResult, result, (timeout + 5) / 1000);

	sceKernelDeleteEventFlag(flag);

	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	flag = sceKernelCreateEventFlag("wait1", 0, 0x00000000, NULL);
	sceKernelStartThread(deleteThread, sizeof(int), &flag);
	sceKernelDelayThread(500);
	sceKernelDeleteEventFlag(flag);

	WAIT_TEST_SIMPLE("NULL", 0, 0, PSP_EVENT_WAITAND, 0);
	WAIT_TEST_SIMPLE("Invalid", 0xDEADBEEF, 0, PSP_EVENT_WAITAND, 0);
	WAIT_TEST_SIMPLE("Deleted", flag, 0, PSP_EVENT_WAITAND, 0);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelWaitEventFlag(0, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, NULL);
	);
	BASIC_SCHED_TEST("Wrong mode",
		result = sceKernelWaitEventFlag(flag2, 0xFFFFFFFF, 3, NULL, NULL);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelWaitEventFlag(flag2, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, NULL);
	);
	BASIC_SCHED_TEST("Other timeout",
		timeout = 100;
		result = sceKernelWaitEventFlag(flag2, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);
	BASIC_SCHED_TEST("Same timeout",
		timeout = 100;
		result = sceKernelWaitEventFlag(flag1, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);
	LOCKED_SCHED_TEST("Same timeout (multi)", 0x200, 0x00000000, 0x200, 0xFFFFFFFF,
		timeout = 100;
		result = sceKernelWaitEventFlag(flag1, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);

	LOCKED_SCHED_TEST("Other timeout (2ms)", 0x200, 0x00000000, 0x200, 0xFFFFFFFF,
		timeout = 2;
		result = sceKernelWaitEventFlag(flag2, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);
	LOCKED_SCHED_TEST("Other timeout (5ms)", 0x200, 0x00000000, 0x200, 0xFFFFFFFF,
		timeout = 5;
		result = sceKernelWaitEventFlag(flag2, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);
	LOCKED_SCHED_TEST("Same timeout (2ms)", 0x200, 0x00000000, 0x200, 0xFFFFFFFF,
		timeout = 2;
		result = sceKernelWaitEventFlag(flag1, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);
	LOCKED_SCHED_TEST("Same timeout (5ms)", 0x200, 0x00000000, 0x200, 0xFFFFFFFF,
		timeout = 5;
		result = sceKernelWaitEventFlag(flag1, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL, &timeout);
	);

	return 0;
}