#include "shared.h"

static void testCreate(const char *title, const char *name, int part, unsigned int attr, unsigned int size, unsigned int blocks, SceKernelFplOptParam *opt) {
	SceUID fpl = sceKernelCreateFpl(name, part, attr, size, blocks, opt);
	if (fpl >= 0) {
		checkpoint(NULL);
		schedf("%s: OK ", title);
		schedfFpl(fpl);
		sceKernelDeleteVpl(fpl);
	} else {
		checkpoint("%s: Failed (%08x)", title, fpl);
	}
}

extern "C" int main(int argc, char **argv) {
	char temp[256];

	checkpointNext("Names:");
	testCreate("  NULL name", NULL, PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	testCreate("  Normal name", "test", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	testCreate("  Blank name", "", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	testCreate("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);

	SceUID dup = sceKernelCreateFpl("create", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	testCreate("  Two with same name", "create", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, NULL);
	sceKernelDeleteFpl(dup);

	checkpointNext("Partitions:");
	const static int parts[] = {-5, -1, 0, 1, 2, 3, 4, 6, 7, 8, 9, 10};
	for (size_t i = 0; i < ARRAY_SIZE(parts); ++i) {
		sprintf(temp, "  Partition %d", parts[i]);
		testCreate(temp, "create", parts[i], 0, 0x100, 0x10, NULL);
	}

	checkpointNext("Attributes:");
	const static u32 attrs[] = {1, 0x100, 0x200, 0x300, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x41FF, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000};
	for (size_t i = 0; i < ARRAY_SIZE(attrs); ++i) {
		sprintf(temp, "  0x%x attr", (unsigned int)attrs[i]);
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, attrs[i], 0x100, 0x10, NULL);
	}

	checkpointNext("Sizes:");
	const static unsigned int sizes[] = {
		-1, 0, 1, 0x10, 0x20, 0x2F, 0x30, 0x31, 0x32, 0x36, 0x38, 0x39, 0x3A,
		0x131, 0x136, 0x139, 0x1000, 0x10000, 0x800000, 0x4000000,
	};
	for (size_t i = 0; i < ARRAY_SIZE(sizes); ++i) {
		sprintf(temp, "  Size 0x%08X", sizes[i]);
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, 0, sizes[i], 0x10, NULL);
	}

	checkpointNext("Counts:");
	const static unsigned int counts[] = {
		-1, 0, 1, 0x10, 0x20, 0x2F, 0x30, 0x31, 0x32, 0x36, 0x38, 0x39, 0x3A,
		0x131, 0x136, 0x139, 0x800000, 0x4000000,
	};
	for (size_t i = 0; i < ARRAY_SIZE(counts); ++i) {
		sprintf(temp, "  Count 0x%08X", counts[i]);
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, 0, 0x100, counts[i], NULL);
	}

	checkpointNext("Option sizes:");
	u32 options[256];
	const static int optionSizes[] = {-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 16, 32, 64, 256, 0x7FFFFFFF};
	for (size_t i = 0; i < ARRAY_SIZE(optionSizes); ++i) {
		sprintf(temp, "  %d", optionSizes[i]);
		options[0] = optionSizes[i];
		options[1] = 4;
		options[2] = -1;
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, (SceKernelFplOptParam *)options);
	}

	checkpointNext("Alignment option:");
	options[0] = 8;
	const static int alignments[] = {-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 16, 32, 36, 64, 256, 0x1000, 0x7FFFFFFF};
	for (size_t i = 0; i < ARRAY_SIZE(alignments); ++i) {
		sprintf(temp, "  %d", alignments[i]);
		options[1] = alignments[i];
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, 0, 0x100, 0x10, (SceKernelFplOptParam *)options);
	}

	checkpointNext("Create 1024:");
	SceUID fpls[1024];
	int result;
	int i;
	for (i = 0; i < 1024; i++)
	{
		result = sceKernelCreateFpl("create", PSP_MEMORY_PARTITION_USER, 0, 0x10, 0x10, NULL);
		fpls[i] = result;
		if (fpls[i] < 0)
			break;
	}

	if (result < 0)
		checkpoint("  Failed at %d (%08x)", i, result);
	else
		checkpoint("  OK");

	while (--i >= 0)
		sceKernelDeleteFpl(fpls[i]);

	return 0;
}