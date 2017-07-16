#include "shared.h"

inline void testFree(const char *title, SceUID fpl, void *data) {
	int result = sceKernelFreeFpl(fpl, data);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK ", title);
		schedfFpl(fpl);
	} else {
		checkpoint(NULL);
		schedf("%s: Failed (%08x) ", title, result);
		schedfFpl(fpl);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, NULL);
	void *temp;
	sceKernelAllocateFpl(fpl, &temp, NULL);

	checkpointNext("Objects:");
	testFree("  Normal", fpl, &temp);
	testFree("  NULL", 0, &temp);
	testFree("  Invalid", 0xDEADBEEF, &temp);
	sceKernelDeleteFpl(fpl);
	testFree("  Deleted", fpl, &temp);
	
	fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x10, 8, NULL);
	sceKernelAllocateFpl(fpl, &temp, NULL);
	checkpointNext("Pointers:");
	testFree("  Normal", fpl, temp);
	testFree("  NULL", fpl, NULL);
	testFree("  Invalid", fpl, (void *)0xDEADBEEF);
	testFree("  Already free", fpl, temp);
	sceKernelDeleteFpl(fpl);

	return 0;
}
