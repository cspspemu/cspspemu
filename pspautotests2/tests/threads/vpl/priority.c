#include "shared.h"

SceUID vpl;

int threadFunction(SceSize argc, void *argv) {
	int n = *(int *) argv;
	void *data;

	schedf("  [%d] Waiting...\n", n);
	int result = sceKernelAllocateVpl(vpl, 0x900 - n * 0x10, &data, NULL);
	schedf("  [%d] Allocated.\n", n);

	schedf("    ");
	schedfVpl(vpl);

	sceKernelFreeVpl(vpl, data);
	return 0;
}

void testPriority(u32 attr) {
	schedf("Attr 0x%x:\n", attr);
	vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, attr, 0x1000 + 0x20, NULL);

	SceUID threads[7];
	int numbers[7] = {1, 2, 3, 4, 5, 6, 7};

	void *data;
	sceKernelAllocateVpl(vpl, 0x900, &data, NULL);

	int i;
	for (i = 0; i < 7; i++) {
		threads[i] = CREATE_PRIORITY_THREAD(threadFunction, 0x18 - i);
		sceKernelStartThread(threads[i], sizeof(int), (void *) &numbers[i]);
	}

	sceKernelDelayThread(1000);
	schedf("  [0] Ready.\n");
	sceKernelFreeVpl(vpl, data);

	sceKernelDelayThread(20 * 1000);

	sceKernelDeleteVpl(vpl);

	for (i = 0; i < 7; i++) {
		sceKernelTerminateDeleteThread(threads[i]);
	}

	flushschedf();
}

int main(int argc, char **argv) {
	testPriority(PSP_VPL_ATTR_FIFO);
	schedf("\n");
	testPriority(PSP_VPL_ATTR_PRIORITY);
	schedf("\n");
	// Does nothing?
	testPriority(PSP_VPL_ATTR_SMALLEST);
	return 0;
}