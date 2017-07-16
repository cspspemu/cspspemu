#include "shared.h"

SETUP_SCHED_TEST;

#define CREATE_TEST(title, name, attr, count, max, options) { \
	sema = sceKernelCreateSema(name, attr, count, max, options); \
	if (sema > 0) { \
		checkpoint(NULL); \
		schedf("%s: OK ", title); \
		schedfSema(sema); \
		sceKernelDeleteSema(sema); \
	} else { \
		checkpoint("%s: Failed (%X)", title, sema); \
	} \
}

int main(int argc, char **argv) {
	SceUID sema;
	int i;
	u32 options[32];
	memset(options, 0, sizeof(options));

	checkpointNext("Names:");
	CREATE_TEST("  NULL name", NULL, 0, 0, 2, NULL);
	CREATE_TEST("  Blank name", "", 0, 0, 2, NULL);
	CREATE_TEST("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", 0, 0, 2, NULL);

	// Two with the same name?
	SceUID sema1 = sceKernelCreateSema("create", 0, 0, 2, NULL);
	SceUID sema2 = sceKernelCreateSema("create", 0, 0, 2, NULL);
	checkpoint(NULL);
	schedf("  Two with same name #1: ");
	schedfSema(sema1);
	checkpoint(NULL);
	schedf("  Two with same name #2: ");
	schedfSema(sema2);
	sceKernelDeleteSema(sema1);
	sceKernelDeleteSema(sema2);

	checkpointNext("Attributes:");
	const static int attrs[] = {1, 0x100, 0x1FF, 0x200, 0x400, 0x800, 0x900, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000};
	for (i = 0; i < ARRAY_SIZE(attrs); ++i) {
		char temp[32];
		sprintf(temp, "  %x", attrs[i]);
		CREATE_TEST(temp, "create", attrs[i], 0, 2, NULL);
	}

	checkpointNext("Counts:");
	CREATE_TEST("  Negative initial count", "create", 0, -1, 2, NULL);
	CREATE_TEST("  Positive initial count", "create", 0, 1, 2, NULL);
	CREATE_TEST("  Initial count above max", "create", 0, 3, 2, NULL);
	CREATE_TEST("  Negative max count", "create", 0, 0, -1, NULL);
	CREATE_TEST("  Large initial count", "create", 0, 65537, 0, NULL);
	CREATE_TEST("  Large max count", "create", 0, 0, 65537, NULL);

	checkpointNext("Option sizes:");
	const static int optionSizes[] = {-1, 0, 1, 4, 8, 16, 32, 64, 256, 0x7FFFFFFF};
	for (i = 0; i < ARRAY_SIZE(optionSizes); ++i) {
		char temp[32];
		sprintf(temp, "  %d", optionSizes[i]);
		options[0] = optionSizes[i];
		CREATE_TEST(temp, "create", 0, 0, 2, options);
	}

	checkpointNext("Scheduling:");
	BASIC_SCHED_TEST("NULL name",
		sema = sceKernelCreateSema(NULL, 0, 0, 1, NULL);
		result = sema > 0 ? 1 : sema;
	);
	BASIC_SCHED_TEST("Create signaled",
		sema = sceKernelCreateSema("create", 0, 1, 1, NULL);
		result = sema > 0 ? 1 : sema;
	);
	sceKernelDeleteSema(sema);
	BASIC_SCHED_TEST("Create not signaled",
		sema = sceKernelCreateSema("create", 0, 0, 1, NULL);
		result = sema > 0 ? 1 : sema;
	);
	sceKernelDeleteSema(sema);

	TWO_STEP_SCHED_TEST("Initial not signaled", 0, 1,
		sceKernelDelayThread(1000);
	,
		result = sceKernelSignalSema(sema1, 1);
	);
	TWO_STEP_SCHED_TEST("Initial signaled", 1, 1,
		sceKernelDelayThread(1000);
	,
		result = sceKernelSignalSema(sema1, 1);
	);

	SceUID semas[1024];
	int result = 0;
	for (i = 0; i < 1024; i++)
	{
		semas[i] = sceKernelCreateSema("create", 0, 0, 2, NULL);
		if (semas[i] < 0)
		{
			result = semas[i];
			break;
		}
	}

	if (result != 0)
		printf("Create 1024: Failed at %d (%08X)\n", i, result);
	else
		printf("Create 1024: OK\n");

	while (--i >= 0)
		sceKernelDeleteSema(semas[i]);

	return 0;
}