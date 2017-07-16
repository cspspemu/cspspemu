#include "shared.h"

void testRefer(const char *title, SceLwMutexWorkarea *workarea, SceKernelLwMutexInfo *info) {
	int result = sceKernelReferLwMutexStatus(workarea, info);
	if (result == 0) {
		schedf("%s: ", title);
		schedfLwMutexInfo(workarea, info);
	} else {
		schedf("%s: Failed (%X)\n", title, result);
	}
}

void testReferByID(const char *title, SceUID uid, SceKernelLwMutexInfo *info) {
	int result = sceKernelReferLwMutexStatusByID(uid, info);
	if (result == 0) {
		schedf("%s: ", title);
		schedfLwMutexInfo((SceLwMutexWorkarea *)info->workarea, info);
	} else {
		schedf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceLwMutexWorkarea workarea;
	SceKernelLwMutexInfo info;

	schedf("sceKernelReferLwMutexStatus:\n");
	sceKernelCreateLwMutex(&workarea, "refer", 0, 0, NULL);
	info.size = sizeof(info);

	// Crashes.
	//testRefer("NULL info", &workarea, NULL);
	testRefer("Normal", &workarea, &info);

	schedf("\nSizes:\n");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferLwMutexStatus(&workarea, &info);
		schedf("  %08X => %08X (result=%08X)\n", sizes[i], info.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteLwMutex(&workarea);

	// Crashes.
	//testRefer("NULL", 0, &info);
	//testRefer("Invalid", (SceLwMutexWorkarea *)0xDEADBEEF, &info);
	testRefer("Deleted", &workarea, &info);

	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferLwMutexStatus(&workarea2, &info);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferLwMutexStatus(&workarea1, &info);
	);

	schedf("\n");
	schedf("sceKernelReferLwMutexStatusByID:\n");
	sceKernelCreateLwMutex(&workarea, "refer", 0, 0, NULL);
	info.size = sizeof(info);

	// Crashes.
	//testReferByID("NULL info", workarea.uid, NULL);
	testReferByID("Normal", workarea.uid, &info);

	schedf("\nSizes:\n");
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferLwMutexStatusByID(workarea.uid, &info);
		schedf("  %08X => %08X (result=%08X)\n", sizes[i], info.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteLwMutex(&workarea);

	testReferByID("NULL", 0, &info);
	testReferByID("Invalid", 0xDEADBEEF, &info);
	testReferByID("Deleted", workarea.uid, &info);

	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferLwMutexStatusByID(workarea2.uid, &info);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferLwMutexStatusByID(workarea1.uid, &info);
	);

	flushschedf();
	return 0;
}