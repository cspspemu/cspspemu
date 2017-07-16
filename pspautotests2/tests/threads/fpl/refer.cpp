#include "shared.h"

void testRefer(const char *title, SceUID fpl, SceKernelFplInfo *info) {
	int result = sceKernelReferFplStatus(fpl, info);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: ", title);
		schedfFpl(*info);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char **argv) {
	SceUID fpl = sceKernelCreateFpl("refer", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	SceKernelFplInfo info;
	info.size = sizeof(info);

	// Crashes.
	//testRefer("NULL info", fpl, NULL);
	testRefer("Normal", fpl, &info);

	checkpointNext("Sizes:");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferFplStatus(fpl, &info);
		checkpoint("  %08x => %08x (result=%08x)", sizes[i], info.size, result);
	}

	sceKernelDeleteFpl(fpl);

	info.size = sizeof(info);
	checkpointNext("Objects:");
	testRefer("NULL", 0, &info);
	testRefer("Invalid", 0xDEADBEEF, &info);
	testRefer("Deleted", fpl, &info);

	return 0;
}