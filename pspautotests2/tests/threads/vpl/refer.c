#include "shared.h"

void testRefer(const char *title, SceUID vpl, SceKernelVplInfo *info) {
	int result = sceKernelReferVplStatus(vpl, info);
	if (result == 0) {
		schedf("%s: ", title);
		schedVplInfo(info);
	} else {
		schedf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceUID vpl = sceKernelCreateVpl("refer", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);
	SceKernelVplInfo info;
	info.size = sizeof(info);

	// Crashes.
	//testRefer("NULL info", vpl, NULL);
	testRefer("Normal", vpl, &info);

	schedf("\nSizes:\n");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferVplStatus(vpl, &info);
		schedf("  %08X => %08X (result=%08X)\n", sizes[i], info.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteVpl(vpl);

	testRefer("NULL", 0, &info);
	testRefer("Invalid", 0xDEADBEEF, &info);
	testRefer("Deleted", vpl, &info);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelReferVplStatus(0, &info);
	);
	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferVplStatus(vpl2, &info);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferVplStatus(vpl1, &info);
	);

	flushschedf();
	return 0;
}