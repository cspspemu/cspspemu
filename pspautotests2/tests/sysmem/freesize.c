#include <common.h>

void tryAllocate(const char *title, SceSize size) {
	SceUID alloc = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "test", PSP_SMEM_Low, size, NULL);
	if (alloc >= 0) {
		checkpoint("%s: OK", title);
		sceKernelFreePartitionMemory(alloc);
	} else {
		checkpoint("%s: %08x", title, alloc);
	}
}

int main(int argc, char *argv[]) {
	checkpointNext("Establishing base values:");
	SceSize baseMaxFree = sceKernelMaxFreeMemSize();
	if (baseMaxFree < 0) {
		checkpoint("sceKernelMaxFreeMemSize: %08x", baseMaxFree);
	} else {
		checkpoint("sceKernelMaxFreeMemSize: OK");
	}

	SceSize baseTotal = sceKernelTotalFreeMemSize();
	if (baseTotal < 0) {
		checkpoint("sceKernelTotalFreeMemSize: %08x", baseTotal);
	} else {
		checkpoint("sceKernelTotalFreeMemSize: OK");
	}

	checkpointNext("After allocating:");
	SceUID blocks[8];
	int i;
	for (i = 0; i < 8; ++i) {
		blocks[i] = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "test", PSP_SMEM_Low, 0x8000, NULL);
	}

	checkpoint("sceKernelMaxFreeMemSize: at base - %d", baseMaxFree - sceKernelMaxFreeMemSize());
	checkpoint("sceKernelTotalFreeMemSize: at base - %d", baseTotal - sceKernelTotalFreeMemSize());
	
	checkpointNext("After fragmenting:");
	sceKernelFreePartitionMemory(blocks[5]);
	blocks[5] = -1;

	checkpoint("sceKernelMaxFreeMemSize: at base - %d", baseMaxFree - sceKernelMaxFreeMemSize());
	checkpoint("sceKernelTotalFreeMemSize: at base - %d", baseTotal - sceKernelTotalFreeMemSize());

	checkpointNext("After free again:");
	for (i = 0; i < 8; ++i) {
		if (blocks[i] >= 0) {
			sceKernelFreePartitionMemory(blocks[i]);
		}
	}

	checkpoint("sceKernelMaxFreeMemSize: at base - %d", baseMaxFree - sceKernelMaxFreeMemSize());
	checkpoint("sceKernelTotalFreeMemSize: at base - %d", baseTotal - sceKernelTotalFreeMemSize());

	checkpointNext("Allocate near limits:");
	tryAllocate("Allocate sceKernelMaxFreeMemSize", sceKernelMaxFreeMemSize());
	tryAllocate("Allocate sceKernelMaxFreeMemSize + 0x100", sceKernelMaxFreeMemSize() + 0x100);
	tryAllocate("Allocate sceKernelTotalFreeMemSize", sceKernelTotalFreeMemSize());

	return 0;
}