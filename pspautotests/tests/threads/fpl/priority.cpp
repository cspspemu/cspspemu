#include "shared.h"

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("Normal (FIFO):");
	{
		SceUID fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, 0, 0x100, 1, NULL);
		void *data;
		sceKernelAllocateFpl(fpl, &data, NULL);
		FplWaitThread wait1("waiting thread 1", fpl, NO_TIMEOUT, 300, 0x30);
		FplWaitThread wait2("waiting thread 2", fpl, NO_TIMEOUT, 300, 0x34);
		FplWaitThread wait3("waiting thread 3", fpl, NO_TIMEOUT, 300, 0x31);
		sceKernelFreeFpl(fpl, data);
		schedfFpl(fpl);
		sceKernelDelayThread(200000);
		sceKernelDeleteFpl(fpl);
	}

	checkpointNext("Priority:");
	{
		SceUID fpl = sceKernelCreateFpl("fpl", PSP_MEMORY_PARTITION_USER, PSP_FPL_ATTR_PRIORITY, 0x100, 1, NULL);
		void *data;
		sceKernelAllocateFpl(fpl, &data, NULL);
		FplWaitThread wait1("waiting thread 1", fpl, NO_TIMEOUT, 300, 0x30);
		FplWaitThread wait2("waiting thread 2", fpl, NO_TIMEOUT, 300, 0x34);
		FplWaitThread wait3("waiting thread 3", fpl, NO_TIMEOUT, 300, 0x31);
		sceKernelFreeFpl(fpl, data);
		schedfFpl(fpl);
		sceKernelDelayThread(200000);
		sceKernelDeleteFpl(fpl);
	}
	return 0;
}