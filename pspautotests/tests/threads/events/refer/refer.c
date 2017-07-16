#include "../sub_shared.h"

SETUP_SCHED_TEST;

void testRefer(const char *title, SceUID flag, SceKernelEventFlagInfo *flaginfo) {
	int result = sceKernelReferEventFlagStatus(flag, flaginfo);
	if (result == 0) {
		schedf("%s: OK\n", title);
	} else {
		printf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceUID flag = sceKernelCreateEventFlag("refer1", 0, 0, NULL);
	SceKernelEventFlagInfo flaginfo;

	// Crashes.
	//testRefer("NULL info", flag, NULL);
	testRefer("Normal", flag, &flaginfo);

	schedf("\nSizes:\n");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		flaginfo.size = sizes[i];
		result = sceKernelReferEventFlagStatus(flag, &flaginfo);
		schedf("  %08X => %08X (result=%08X)\n", sizes[i], flaginfo.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteEventFlag(flag);

	testRefer("NULL", 0, &flaginfo);
	testRefer("Invalid", 0xDEADBEEF, &flaginfo);
	testRefer("Deleted", flag, &flaginfo);

	flushschedf();

	BASIC_SCHED_TEST("NULL",
		result = sceKernelReferEventFlagStatus(NULL, &flaginfo);
	);
	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferEventFlagStatus(flag2, &flaginfo);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferEventFlagStatus(flag1, &flaginfo);
	);

	return 0;
}