#include "shared.h"

void testRefer(const char *title, SceUID mutex, SceKernelMutexInfo *info) {
	int result = sceKernelReferMutexStatus(mutex, info);
	if (result == 0) {
		schedf("%s: ", title);
		schedfMutexInfo(info);
	} else {
		schedf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceUID mutex = sceKernelCreateMutex("refer", 0, 0, NULL);
	SceKernelMutexInfo info;
	info.size = sizeof(info);

	// Crashes.
	//testRefer("NULL info", mutex, NULL);
	testRefer("Normal", mutex, &info);

	schedf("\nSizes:\n");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferMutexStatus(mutex, &info);
		schedf("  %08X => %08X (result=%08X)\n", sizes[i], info.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteMutex(mutex);

	testRefer("NULL", 0, &info);
	testRefer("Invalid", 0xDEADBEEF, &info);
	testRefer("Deleted", mutex, &info);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelReferMutexStatus(0, &info);
	);
	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferMutexStatus(mutex2, &info);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferMutexStatus(mutex1, &info);
	);

	flushschedf();
	return 0;
}