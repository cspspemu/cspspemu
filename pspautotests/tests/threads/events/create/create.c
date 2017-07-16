#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define CREATE_TEST(title, name, attr, bits, options) { \
	flag = sceKernelCreateEventFlag(name, attr, bits, options); \
	if (flag > 0) { \
		printf("%s: OK\n", title); \
		PRINT_FLAG(flag); \
		sceKernelDeleteEventFlag(flag); \
	} else { \
		printf("%s: Failed (%X)\n", title, flag); \
	} \
}

int main(int argc, char **argv) {
	SceUID flag;

	CREATE_TEST("NULL name", NULL, 0, 0, NULL);
	CREATE_TEST("Blank name", "", 0, 0, NULL);
	CREATE_TEST("Long name", "1234567890123456789012345678901234567890123456789012345678901234", 0, 0, NULL);
	CREATE_TEST("Weird attr", "create", 1, 0, NULL);
	CREATE_TEST("0x10 attr", "create", 0x10, 0, NULL);
	CREATE_TEST("0x100 attr", "create", 0x100, 0, NULL);
	CREATE_TEST("0x122 attr", "create", 0x122, 0, NULL);
	CREATE_TEST("0x200 attr", "create", 0x200, 0, NULL);
	CREATE_TEST("0x222 attr", "create", 0x222, 0, NULL);
	CREATE_TEST("0x300 attr", "create", 0x300, 0, NULL);
	CREATE_TEST("0x900 attr", "create", 0x900, 0, NULL);
	CREATE_TEST("0x1200 attr", "create", 0x1200, 0, NULL);
	CREATE_TEST("All bits set", "create", 0, -1, NULL);
	CREATE_TEST("Some bits set", "create", 0, 1 | 8 | 32, NULL);

	SceUID flag1 = sceKernelCreateEventFlag("create", 0, 0, NULL);
	SceUID flag2 = sceKernelCreateEventFlag("create", 0, 0, NULL);
	PRINT_FLAG(flag1);
	PRINT_FLAG(flag2);
	sceKernelDeleteEventFlag(flag1);
	sceKernelDeleteEventFlag(flag2);

	BASIC_SCHED_TEST("NULL name",
		flag = sceKernelCreateEventFlag(NULL, 0, 0, NULL);
		result = flag > 0 ? 1 : flag;
	);
	BASIC_SCHED_TEST("Create some bits set",
		flag = sceKernelCreateEventFlag("create2", 0, 1 | 8 | 32, NULL);
		result = flag > 0 ? 1 : flag;
	);
	sceKernelDeleteEventFlag(flag);
	BASIC_SCHED_TEST("Create no bits set",
		flag = sceKernelCreateEventFlag("create2", 0, 0, NULL);
		result = flag > 0 ? 1 : flag;
	);
	sceKernelDeleteEventFlag(flag);

	LOCKED_SCHED_TEST("Initial no bits set", 0, 0, 0, 0,
		sceKernelDelayThread(1000);
		result = 0;
	);

	LOCKED_SCHED_TEST("Initial some bits set", 0, 1 | 8 | 32, 0, 0,
		sceKernelDelayThread(1000);
		result = 0;
	);

	SceUID flags[1024];
	int i, result = 0;
	for (i = 0; i < 1024; i++)
	{
		flags[i] = sceKernelCreateEventFlag("create", 0, 0, NULL);
		if (flags[i] < 0)
		{
			result = flags[i];
			break;
		}
	}

	if (result != 0)
		printf("Create 1024: Failed at %d (%08X)\n", i, result);
	else
		printf("Create 1024: OK\n");

	while (--i >= 0)
		sceKernelDeleteEventFlag(flags[i]);

	return 0;
}