#include "shared.h"
#include <limits.h>

typedef struct SceKernelVplOptParam SceKernelVplOptParam;

static void testCreate(const char *title, const char *name, int part, unsigned int attr, unsigned int size, SceKernelVplOptParam *opt) {
	SceUID vpl = sceKernelCreateVpl(name, part, attr, size, opt);
	schedf("%s: ", title);
	schedfVpl(vpl);
	if (vpl > 0) {
		sceKernelDeleteVpl(vpl);
	}
}

int main(int argc, char **argv) {
	int i;
	char temp[128];

	testCreate("Normal", "vpl", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);

	schedf("\nNames:\n");
	testCreate("  NULL name", NULL, PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	testCreate("  Blank name", "", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	testCreate("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);

	// 5 = crash
	schedf("\nPartitions:\n");
	int parts[] = {-5, -1, 0, 1, 2, 3, 4, 6, 7, 8, 9, 10};
	for (i = 0; i < sizeof(parts) / sizeof(parts[0]); ++i) {
		sprintf(temp, "  Partition %d", parts[i]);
		testCreate(temp, "vpl", parts[i], 0, 0x1000, NULL);
	}
	flushschedf();

	schedf("\nAttributes:\n");
	unsigned int attrs[] = {1, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000};
	for (i = 0; i < sizeof(attrs) / sizeof(attrs[0]); ++i) {
		sprintf(temp, "  0x%x attr", attrs[i]);
		testCreate(temp, "vpl", PSP_MEMORY_PARTITION_USER, attrs[i], 0x1000, NULL);
	}
	flushschedf();

	schedf("\nSizes:\n");
	unsigned int sizes[] = {
		-1, 0, 1, 0x10, 0x20, 0x2F, 0x30, 0x31, 0x32, 0x36, 0x38, 0x39, 0x3A,
		0x131, 0x136, 0x139, 0x1000, 0x10000, 0x100000, 0x1000000, 0x10000000,
		0x1800000, 0x2000000,
	};
	for (i = 0; i < sizeof(sizes) / sizeof(sizes[0]); ++i) {
		sprintf(temp, "  Size 0x%08X", sizes[i]);
		testCreate(temp, "vpl", PSP_MEMORY_PARTITION_USER, 0, sizes[i], NULL);
	}
	flushschedf();

	printf("\n");
	SceUID vpl1 = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	SceUID vpl2 = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	if (vpl1 > 0 && vpl2 > 0) {
		printf("Two with same name: OK\n");
	} else {
		printf("Two with same name: Failed (%08X, %08X)\n", vpl1, vpl2);
	}
	sceKernelDeleteVpl(vpl1);
	sceKernelDeleteVpl(vpl2);

	SceUID vpl;
	BASIC_SCHED_TEST("NULL name",
		vpl = sceKernelCreateVpl(NULL, PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
		result = vpl > 0 ? 1 : vpl;
	);
	BASIC_SCHED_TEST("Normal",
		vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
		result = vpl > 0 ? 1 : vpl;
	);
	sceKernelDeleteVpl(vpl);

	SceUID vpls[1024];
	int result = 0;
	for (i = 0; i < 1024; i++)
	{
		vpls[i] = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
		if (vpls[i] < 0)
		{
			result = vpls[i];
			break;
		}
	}

	if (result != 0)
		printf("Create 1024: Failed at %d (%08X)\n", i, result);
	else
		printf("Create 1024: OK\n");

	while (--i >= 0)
		sceKernelDeleteVpl(vpls[i]);

	return 0;
}