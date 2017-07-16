#include "shared.h"

void testCreate(const char *title, const char *name, int part, u32 attr, SceSize size, void *opt) {
	SceUID result = sceKernelCreateMsgPipe(name, part, attr, size, opt);
	if (result >= 0) {
		checkpoint(NULL);
		schedf("%s: OK ", title);
		schedfMsgPipe(result);
		sceKernelDeleteMsgPipe(result);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char *argv[]) {
	char temp[256];

	checkpointNext("Names:");
	testCreate("  NULL name", NULL, PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	testCreate("  Normal name", "test", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	testCreate("  Blank name", "", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	testCreate("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);

	SceUID dup = sceKernelCreateMsgPipe("create", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	testCreate("  Two with same name", "test", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	sceKernelDeleteMsgPipe(dup);

	checkpointNext("Partitions:");
	const static int parts[] = {-5, -1, 0, 1, 2, 3, 4, 6, 7, 8, 9, 10};
	for (size_t i = 0; i < ARRAY_SIZE(parts); ++i) {
		sprintf(temp, "  Partition %d", parts[i]);
		testCreate(temp, "create", parts[i], 0, 0x000, NULL);
	}

	checkpointNext("Attributes:");
	const static u32 attrs[] = {1, 0x100, 0x200, 0x300, 0x3FF, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x51FF, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000};
	for (size_t i = 0; i < ARRAY_SIZE(attrs); ++i) {
		sprintf(temp, "  0x%x attr", (unsigned int)attrs[i]);
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, attrs[i], 0x100, NULL);
	}

	checkpointNext("Sizes:");
	const static unsigned int sizes[] = {
		-1, 0, 1, 0x10, 0x20, 0x2F, 0x30, 0x31, 0x32, 0x36, 0x38, 0x39, 0x3A,
		0x131, 0x136, 0x139, 0x1000, 0x10000, 0x100000, 0x4000000,
	};
	for (size_t i = 0; i < ARRAY_SIZE(sizes); ++i) {
		sprintf(temp, "  Size 0x%08X", sizes[i]);
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, 0, sizes[i], NULL);
	}

	checkpointNext("Option sizes:");
	u32 options[256];
	const static int optionSizes[] = {-1, 0, 1, 4, 8, 16, 32, 64, 256, 0x7FFFFFFF};
	for (size_t i = 0; i < ARRAY_SIZE(optionSizes); ++i) {
		sprintf(temp, "  %d", optionSizes[i]);
		options[0] = optionSizes[i];
		testCreate(temp, "create", PSP_MEMORY_PARTITION_USER, 0, 0x100, options);
	}

	checkpointNext("Create 1024:");
	SceUID msgpipes[1024];
	int result;
	int i;
	for (i = 0; i < 1024; i++)
	{
		result = sceKernelCreateMsgPipe("create", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		msgpipes[i] = result;
		if (msgpipes[i] < 0)
			break;
	}

	if (result < 0)
		checkpoint("  Failed at %d (%08x)", i, result);
	else
		checkpoint("  OK");

	while (--i >= 0)
		sceKernelDeleteMsgPipe(msgpipes[i]);

	return 0;
}
