#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define POLL_TEST(title, flag, bits, mode, get_bits) { \
	u32 result_bits = get_bits ? 0xDEADBEEF : 0; \
	int result = sceKernelPollEventFlag(flag, bits, mode, get_bits ? &result_bits : NULL); \
	if (result == 0) { \
		printf("%s: OK (bits=%08X) ", title, (uint) result_bits); \
		PRINT_FLAG(flag); \
	} else { \
		printf("%s: Failed (%08X, bits=%08X)\n", title, result, (uint) result_bits); \
	} \
}

int main(int argc, char **argv) {
	SceUID flag = sceKernelCreateEventFlag("poll1", 0, 0xFFFFFFFF, NULL);

	POLL_TEST("And 0x00000000", flag, 0, PSP_EVENT_WAITAND, 1);
	POLL_TEST("Or 0x00000000", flag, 0, PSP_EVENT_WAITOR, 1);
	POLL_TEST("Clear 0x00000000", flag, 0, PSP_EVENT_WAITCLEAR, 1);
	POLL_TEST("Wrong (0x04) 0x00000000", flag, 0, 0x04, 1);
	POLL_TEST("Wrong (0xFF) 0x00000000", flag, 0, 0xFF, 1);

	POLL_TEST("And 0x00000001", flag, 1, PSP_EVENT_WAITAND, 1);
	POLL_TEST("Or 0x00000001", flag, 1, PSP_EVENT_WAITOR, 1);
	POLL_TEST("Clear 0x00000001", flag, 1, PSP_EVENT_WAITCLEAR, 1);
	POLL_TEST("Wrong (0x02) 0x00000001", flag, 1, 0x02, 1);
	POLL_TEST("Wrong (0x04) 0x00000001", flag, 1, 0x04, 1);
	POLL_TEST("Wrong (0x08) 0x00000001", flag, 1, 0x08, 1);
	POLL_TEST("Wrong (0x40) 0x00000001", flag, 1, 0x40, 1);
	POLL_TEST("Wrong (0x80) 0x00000001", flag, 1, 0x80, 1);
	POLL_TEST("Wrong (0xFF) 0x00000001", flag, 1, 0xFF, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	POLL_TEST("Clear/Or 0x00000000", flag, 0, PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITOR, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	POLL_TEST("Clear/Or 0x00000001", flag, 1, PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITOR, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	POLL_TEST("Clear/Or 0x00000001 (no out bits)", flag, 1, PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITOR, 0);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	POLL_TEST("Clear all/Or 0x00000001", flag, 1, PSP_EVENT_WAITCLEARALL | PSP_EVENT_WAITOR, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	POLL_TEST("Clear all/Clear/And 0x00000001", flag, 1, PSP_EVENT_WAITCLEARALL | PSP_EVENT_WAITCLEAR, 1);

	sceKernelSetEventFlag(flag, 0xFFFFFFFF);
	POLL_TEST("0xFFFFFFFF & 0x00000000", flag, 0x00000000, PSP_EVENT_WAITAND, 1);
	POLL_TEST("0xFFFFFFFF & 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 1);
	POLL_TEST("0xFFFFFFFF | 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR, 1);
	sceKernelClearEventFlag(flag, 0x0000FFFF);
	POLL_TEST("0x0000FFFF & 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 1);
	POLL_TEST("0x0000FFFF | 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR, 1);
	POLL_TEST("0x0000FFFF & 0xFFFFFFFF with clear", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND | PSP_EVENT_WAITCLEAR, 1);
	POLL_TEST("0x0000FFFF | 0xFFFFFFFF with clear", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEAR, 1);
	sceKernelClearEventFlag(flag, 0x0000FFFF);
	POLL_TEST("0x0000FFFF & 0xFFFFFFFF with clear all", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND | PSP_EVENT_WAITCLEARALL, 1);
	POLL_TEST("0x0000FFFF | 0xFFFFFFFF with clear all", flag, 0xFFFFFFFF, PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEARALL, 1);
	sceKernelClearEventFlag(flag, 0x00000000);
	POLL_TEST("0x00000000 & 0xFFFFFFFF", flag, 0xFFFFFFFF, PSP_EVENT_WAITAND, 1);

	sceKernelDeleteEventFlag(flag);

	POLL_TEST("NULL", 0, 0, PSP_EVENT_WAITAND, 0);
	POLL_TEST("Invalid", 0xDEADBEEF, 0, PSP_EVENT_WAITAND, 0);
	POLL_TEST("Deleted", flag, 0, PSP_EVENT_WAITAND, 0);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelPollEventFlag(0, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL);
	);
	BASIC_SCHED_TEST("Wrong mode",
		result = sceKernelPollEventFlag(flag2, 0xFFFFFFFF, 3, NULL);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelPollEventFlag(flag2, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL);
	);
	BASIC_SCHED_TEST("Same",
		result = sceKernelPollEventFlag(flag1, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL);
	);
	LOCKED_SCHED_TEST("Same (multi)", 0x200, 0x00000000, 0x200, 0xFFFFFFFF,
		result = sceKernelPollEventFlag(flag1, 0xFFFFFFFF, PSP_EVENT_WAITAND, NULL);
	);

	return 0;
}