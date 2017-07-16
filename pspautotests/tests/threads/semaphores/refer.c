#include "shared.h"

SETUP_SCHED_TEST;

void testRefer(const char *title, SceUID sema, SceKernelSemaInfo *info) {
	int result = sceKernelReferSemaStatus(sema, info);
	if (result == 0) {
		schedf("%s: OK\n", title);
	} else {
		schedf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceUID sema = sceKernelCreateSema("refer1", 0, 0, 1, NULL);
	SceKernelSemaInfo semainfo;

	// Crashes.
	//testRefer("NULL info", sema, NULL);
	testRefer("Normal", sema, &semainfo);

	schedf("\nSizes:\n");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		semainfo.size = sizes[i];
		result = sceKernelReferSemaStatus(sema, &semainfo);
		schedf("  %08X => %08X (result=%08X)\n", sizes[i], semainfo.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteSema(sema);

	testRefer("NULL", 0, &semainfo);
	testRefer("Invalid", 0xDEADBEEF, &semainfo);
	testRefer("Deleted", sema, &semainfo);
	
	flushschedf();

	BASIC_SCHED_TEST("NULL",
		result = sceKernelReferSemaStatus(NULL, &semainfo);
	);
	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferSemaStatus(sema2, &semainfo);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferSemaStatus(sema1, &semainfo);
	);

	return 0;
}