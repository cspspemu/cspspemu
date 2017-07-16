#include <common.h>

static char *low, *high;

void setup() {
	SceUID lowID = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "low", PSP_SMEM_Low, 0x10, NULL);
	SceUID highID = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "high", PSP_SMEM_High, 0x10, NULL);
	if (lowID < 0 || highID < 0) {
		printf("Test failure: can't allocate two 0x10 chunks\n");
	}

	low = (char *)sceKernelGetBlockHeadAddr(lowID);
	high = (char *)sceKernelGetBlockHeadAddr(highID) + 0x10;

	sceKernelFreePartitionMemory(lowID);
	sceKernelFreePartitionMemory(highID);
}

const char *allocatedPos(char *pos, SceSize size) {
	if (pos <= low) {
		return "low";
	} else if (pos >= high - size) {
		return "high";
	} else if (pos > low && pos < high) {
		return "middle";
	} else {
		return "fail";
	}
}

char *testAlloc(const char *title, int partition, const char *name, int type, SceSize size, char *pos) {
	int result = sceKernelAllocPartitionMemory(partition, name, type, size, pos);
	if (result >= 0) {
		char *addr = (char *)sceKernelGetBlockHeadAddr(result);
		sceKernelFreePartitionMemory(result);

		printf("%s: OK (%s)\n", title, allocatedPos(addr, size));
		return addr;
	} else {
		printf("%s: Failed (%08X)\n", title, result);
		return NULL;
	}
}

char *testAllocDiff(const char *title, int partition, const char *name, int type, SceSize size, char *pos, int diffHigh, char *diff) {
	int result = sceKernelAllocPartitionMemory(partition, name, type, size, pos);
	if (result >= 0) {
		char *addr = (char *)sceKernelGetBlockHeadAddr(result);
		sceKernelFreePartitionMemory(result);

		if ((diffHigh ? diff - addr : addr - diff) == 0x1800) {
			printf("That's strange: addr: %08x, diff: %08x\n", (unsigned int)addr, (unsigned int)diff);
		}

		printf("%s: OK (%s, difference %x)\n", title, allocatedPos(addr, size), diffHigh ? diff - addr : addr - diff);
		return addr;
	} else {
		printf("%s: Failed (%08X)\n", title, result);
		return NULL;
	}
}

volatile int didResched = 0;
int reschedFunc(SceSize argc, void *argp) {
	didResched = 1;
	return 0;
}

int main(int argc, char *argv[]) {
	int i;
	char temp[128];

	setup();

	testAlloc("Normal", PSP_MEMORY_PARTITION_USER, "test", 0, 0x10, NULL);

	printf("\nNames:\n");
	testAlloc("  NULL name", PSP_MEMORY_PARTITION_USER, NULL, 0, 0x1000, NULL);
	testAlloc("  Blank name", PSP_MEMORY_PARTITION_USER, "", 0, 0x1000, NULL);
	testAlloc("  Long name", PSP_MEMORY_PARTITION_USER, "1234567890123456789012345678901234567890123456789012345678901234", 0, 0x1000, NULL);

	printf("\nPartitions:\n");
	int parts[] = {-5, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
	for (i = 0; i < sizeof(parts) / sizeof(parts[0]); ++i) {
		sprintf(temp, "  Partition %d", parts[i]);
		testAlloc(temp, parts[i], "part", 0, 0x1000, NULL);
	}

	printf("\nTypes:\n");
	unsigned int types[] = {-5, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
	for (i = 0; i < sizeof(types) / sizeof(types[0]); ++i) {
		sprintf(temp, "  Type %d", types[i]);
		testAlloc(temp, PSP_MEMORY_PARTITION_USER, "type", types[i], 0x1000, NULL);
	}

	printf("\nSizes:\n");
	unsigned int sizes[] = {
		-1, 0, 1, 0x10, 0x20, 0x2F, 0x30, 0x31, 0x32, 0x36, 0x38, 0x39, 0x3A,
		0x131, 0x136, 0x139, 0x1000, 0x10000, 0x100000, 0x1000000, 0x10000000,
		0x1800000, 0x2000000,
	};
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		sprintf(temp, "  Size 0x%08X", sizes[i]);
		testAlloc(temp, PSP_MEMORY_PARTITION_USER, "size", 0, sizes[i], NULL);
	}
	
	printf("\nPositions:\n");
	testAlloc("  Wrong type", PSP_MEMORY_PARTITION_USER, "pos", 0, 0x1000, high - 0x1000);
	testAlloc("  Below low", PSP_MEMORY_PARTITION_USER, "pos", 2, 0x1000, low - 0x1000);
	testAlloc("  At low", PSP_MEMORY_PARTITION_USER, "pos", 2, 0x1000, low);
	testAlloc("  Above low", PSP_MEMORY_PARTITION_USER, "pos", 2, 0x1000, low + 0x1000);
	testAlloc("  Near high", PSP_MEMORY_PARTITION_USER, "pos", 2, 0x1000, high - 0x1000);
	testAlloc("  At high", PSP_MEMORY_PARTITION_USER, "pos", 2, 0x1000, high);
	testAlloc("  Above high", PSP_MEMORY_PARTITION_USER, "pos", 2, 0x1000, high + 0x1000);

	SceUID posPart1 = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "part1", 2, 0x1000, low);
	char *pos1 = (char *)sceKernelGetBlockHeadAddr(posPart1);
	testAlloc("  Second at low", PSP_MEMORY_PARTITION_USER, "part2", 2, 0x1000, low);
	char *pos2 = testAlloc("  Second type low", PSP_MEMORY_PARTITION_USER, "part2", 0, 0x1000, low);
	printf("    Difference: %X\n", pos2 - pos1);
	sceKernelFreePartitionMemory(posPart1);

	printf("\nAlignment:\n");
	SceUID alignbase[0x2000];
	int alignbaseCountLow, alignbaseCountHigh;
	for (alignbaseCountLow = 0; alignbaseCountLow < 0x1000; ++alignbaseCountLow) {
		alignbase[alignbaseCountLow] = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "alignbase", 0, 0x1, NULL);
		char *base = (char *)sceKernelGetBlockHeadAddr(alignbase[alignbaseCountLow]);
		if (((u32)base & 0xFFFF) == 0xF000)
			break;
	}
	for (alignbaseCountHigh = 0; alignbaseCountHigh < 0x1000; ++alignbaseCountHigh) {
		alignbase[0x1000 + alignbaseCountHigh] = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "alignbaseTop", 1, 0x1, NULL);
		char *base = (char *)sceKernelGetBlockHeadAddr(alignbase[0x1000 + alignbaseCountHigh]);
		if (((u32)base & 0xFFFF) == 0xF000)
			break;
	}
	SceUID alignLowID = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "alignLow", 3, 0x1000, (void *)0x1000);
	SceUID alignHighID = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "alignHigh", 4, 0x1000, (void *)0x1000);
	char *alignLow = (char *)sceKernelGetBlockHeadAddr(alignLowID);
	char *alignHigh = (char *)sceKernelGetBlockHeadAddr(alignHighID);

	unsigned int aligns[] = {
		-5, -1, 0, 1, 2, 3, 4, 7, 8, 0x10, 0x11, 0x20, 0x2F, 0x40, 0x60, 0x80, 0x100,
		0x1000, 0x2000, 0x4000, 0x1000000, 0x4000000, 0x40000000, 0x80000000,
	};
	for (i = 0; i < sizeof(aligns) / sizeof(aligns[0]); ++i) {
		char *addr;
		sprintf(temp, "  Align 0x%08X low", aligns[i]);
		addr = testAllocDiff(temp, PSP_MEMORY_PARTITION_USER, "part2", 3, 0x1000, (char *)aligns[i], 0, alignLow);
		if (aligns[i] != 0 && ((u32)addr % aligns[i]) != 0) {
			printf("    ACK: Not actually aligned: %08x\n", (unsigned int)addr);
		}

		sprintf(temp, "  Align 0x%08X high", aligns[i]);
		addr = testAllocDiff(temp, PSP_MEMORY_PARTITION_USER, "part2", 4, 0x1000, (char *)aligns[i], 1, alignHigh);
		if (aligns[i] != 0 && ((u32)addr % aligns[i]) != 0) {
			printf("%s: not actually aligned, %08x\n", temp, (unsigned int)addr);
		}
	}
	sceKernelFreePartitionMemory(alignLowID);
	while (alignbaseCountLow >= 0) {
		sceKernelFreePartitionMemory(alignbase[alignbaseCountLow--]);
	}
	while (alignbaseCountHigh >= 0) {
		sceKernelFreePartitionMemory(alignbase[0x1000 + alignbaseCountHigh--]);
	}
	sceKernelFreePartitionMemory(alignHighID);

	printf("\n");
	SceUID part1 = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "part1", 0, 0x1, NULL);
	SceUID part2 = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "part2", 0, 0x1, NULL);
	if (part1 > 0 && part2 > 0) {
		printf("Two with same name: OK\n");
	} else {
		printf("Two with same name: Failed (%08X, %08X)\n", part1, part2);
	}
	char *part1Pos = (char *)sceKernelGetBlockHeadAddr(part1);
	char *part2Pos = (char *)sceKernelGetBlockHeadAddr(part2);
	printf("Minimum difference: %x\n", part2Pos - part1Pos);
	sceKernelFreePartitionMemory(part1);
	sceKernelFreePartitionMemory(part2);

	part1 = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "part1", 3, 0x101, (void *)1);
	part2 = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "part2", 3, 0x101, (void *)1);
	part1Pos = (char *)sceKernelGetBlockHeadAddr(part1);
	part2Pos = (char *)sceKernelGetBlockHeadAddr(part2);
	printf("Offset difference: %x\n", part2Pos - part1Pos);
	sceKernelFreePartitionMemory(part1);
	sceKernelFreePartitionMemory(part2);

	SceUID reschedThread = sceKernelCreateThread("resched", &reschedFunc, sceKernelGetThreadCurrentPriority(), 0x1000, 0, NULL);
	sceKernelStartThread(reschedThread, 0, NULL);
	SceUID reschedPart = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "part", 0, 0x1000, NULL);
	sceKernelGetBlockHeadAddr(reschedPart);
	sceKernelFreePartitionMemory(reschedPart);
	sceKernelTerminateDeleteThread(reschedThread);
	printf("Reschedule: %s\n", didResched ? "yes" : "no");

	SceUID allocs[1024];
	int result = 0;
	for (i = 0; i < 1024; i++)
	{
		allocs[i] = sceKernelAllocPartitionMemory(PSP_MEMORY_PARTITION_USER, "create", 0, 0x100, NULL);
		if (allocs[i] < 0)
		{
			result = allocs[i];
			break;
		}
	}

	if (result != 0)
		printf("Create 1024: Failed at %d (%08X)\n", i, result);
	else
		printf("Create 1024: OK\n");

	while (--i >= 0)
		sceKernelFreePartitionMemory(allocs[i]);

	printf("Get deleted: %08X\n", (unsigned int)sceKernelGetBlockHeadAddr(reschedPart));
	printf("Get NULL: %08X\n", (unsigned int)sceKernelGetBlockHeadAddr(0));
	printf("Get invalid: %08X\n", (unsigned int)sceKernelGetBlockHeadAddr(0xDEADBEEF));
	printf("Free deleted: %08X\n", sceKernelFreePartitionMemory(reschedPart));
	printf("Free NULL: %08X\n", sceKernelFreePartitionMemory(0));
	printf("Free invalid: %08X\n", sceKernelFreePartitionMemory(0xDEADBEEF));

	return 0;
}
