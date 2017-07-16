#include "shared.h"

inline void createTest(const char *title, const char *name, u32 attr, int count, void *options) {
	SceUID mutex = sceKernelCreateMutex(name, attr, count, options);
	if (mutex > 0) {
		checkpoint(NULL);
		schedf("%s: ", title);
		schedfMutex(mutex);
		sceKernelDeleteMutex(mutex);
	} else {
		checkpoint("%s: Failed (%X)", title, mutex);
	}
}

int main(int argc, char **argv) {
	SceUID mutex;
	int i;
	u32 options[32];
	memset(options, 0, sizeof(options));
	
	checkpointNext("Names:");
	createTest("  NULL name", NULL, 0, 0, NULL);
	createTest("  Blank name", "", 0, 0, NULL);
	createTest("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", 0, 0, NULL);

	SceUID mutex1 = sceKernelCreateMutex("create", 0, 0, NULL);
	SceUID mutex2 = sceKernelCreateMutex("create", 0, 0, NULL);
	if (mutex1 > 0 && mutex2 > 0) {
		checkpoint("  Two with same name: OK");
	} else {
		checkpoint("  Two with same name: Failed (%X, %X)", mutex1, mutex2);
	}
	sceKernelDeleteMutex(mutex1);
	sceKernelDeleteMutex(mutex2);

	checkpointNext("Attributes:");
	const static int attrs[] = {1, 0x100, 0x200, 0x400, 0x800, 0xB00, 0xBFF, 0xC00, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000};
	for (i = 0; i < ARRAY_SIZE(attrs); ++i) {
		char temp[32];
		sprintf(temp, "  %x", attrs[i]);
		createTest(temp, "create", attrs[i], 0, NULL);
	}

	checkpointNext("Counts:");
	createTest("  Negative count", "create", 0, -1, NULL);
	createTest("  Positive count", "create", 0, 1, NULL);
	createTest("  Large count", "create", 0, 65537, NULL);

	checkpointNext("Option sizes:");
	const static int optionSizes[] = {-1, 0, 1, 4, 8, 16, 32, 64, 256, 0x7FFFFFFF};
	for (i = 0; i < ARRAY_SIZE(optionSizes); ++i) {
		char temp[32];
		sprintf(temp, "  %d", optionSizes[i]);
		options[0] = optionSizes[i];
		createTest(temp, "create", 0, 1, options);
	}

	checkpointNext("Scheduling:");
	BASIC_SCHED_TEST("NULL name",
		mutex = sceKernelCreateMutex(NULL, 0, 0, NULL);
		result = mutex > 0 ? 1 : mutex;
	);
	BASIC_SCHED_TEST("Create locked",
		mutex = sceKernelCreateMutex("create2", 0, 1, NULL);
		result = mutex > 0 ? 1 : mutex;
	);
	sceKernelDeleteMutex(mutex);
	BASIC_SCHED_TEST("Create not locked",
		mutex = sceKernelCreateMutex("create2", 0, 0, NULL);
		result = mutex > 0 ? 1 : mutex;
	);
	sceKernelDeleteMutex(mutex);

	LOCKED_SCHED_TEST("Initial not locked", 0, 0,
		sceKernelDelayThread(1000);
		result = 0;
	);

	LOCKED_SCHED_TEST("Initial locked", 1, 0,
		sceKernelDelayThread(1000);
		result = 0;
	);

	SceUID mutexes[1024];
	int result = 0;
	for (i = 0; i < 1024; i++)
	{
		mutexes[i] = sceKernelCreateMutex("create", 0, 0, NULL);
		if (mutexes[i] < 0)
		{
			result = mutexes[i];
			break;
		}
	}

	if (result != 0)
		schedf("Create 1024: Failed at %d (%08X)\n", i, result);
	else
		schedf("Create 1024: OK\n");

	while (--i >= 0)
		sceKernelDeleteMutex(mutexes[i]);

	return 0;
}