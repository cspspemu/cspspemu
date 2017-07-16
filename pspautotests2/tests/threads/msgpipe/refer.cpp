#include "shared.h"

void testRefer(const char *title, SceUID msgpipe, SceKernelMppInfo *info) {
	int result = sceKernelReferMsgPipeStatus(msgpipe, info);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: ", title);
		schedfMsgPipe(info);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char **argv) {
	SceUID msgpipe = sceKernelCreateMsgPipe("refer", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
	SceKernelMppInfo info;
	info.size = sizeof(info);

	// Crashes.
	//testRefer("NULL info", msgpipe, NULL);
	testRefer("Normal", msgpipe, &info);

	checkpointNext("Sizes:");
	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int i, result;
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		info.size = sizes[i];
		result = sceKernelReferMsgPipeStatus(msgpipe, &info);
		checkpoint("  %08X => %08X (result=%08X)", sizes[i], info.size, result);
	}
	schedf("\n");
	flushschedf();

	sceKernelDeleteMsgPipe(msgpipe);

	info.size = sizeof(info);
	checkpointNext("Objects:");
	testRefer("NULL", 0, &info);
	testRefer("Invalid", 0xDEADBEEF, &info);
	testRefer("Deleted", msgpipe, &info);

	flushschedf();
	return 0;
}