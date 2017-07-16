#include "shared.h"

int cbFunc(int arg1, int arg2, void *arg) {
	return 0;
}

void testRefer(const char *title, SceUID cb, SceKernelCallbackInfo *info) {
	int result = sceKernelReferCallbackStatus(cb, info);
	if (result == 0 && info != NULL) {
		checkpoint(NULL);
		schedf("%s: ", title);
		schedfCallback(*info);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID cb = sceKernelCreateCallback("refer", &cbFunc, (void *)0xDEADBEEF);
	SceKernelCallbackInfo info;
	info.size = sizeof(info);

	// Crashes.
	//testRefer("NULL info", cb, NULL);
	testRefer("Normal", cb, &info);

	checkpointNext("Sizes:");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 60, 64, 68, 72, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferCallbackStatus(cb, &info);
		checkpoint("  %08x => %08x (result=%08x)", sizes[i], info.size, result);
	}

	sceKernelDeleteCallback(cb);

	info.size = sizeof(info);
	checkpointNext("Objects:");
	testRefer("NULL", 0, &info);
	testRefer("Invalid", 0xDEADBEEF, &info);
	testRefer("Deleted", cb, &info);

	return 0;
}