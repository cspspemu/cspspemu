#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define CREATE_TEST(title, name, attr, options) { \
	mbx = sceKernelCreateMbx(name, attr, options); \
	if (mbx > 0) { \
		printf("%s: OK\n", title); \
		PRINT_MBX(mbx); \
		sceKernelDeleteMbx(mbx); \
	} else { \
		printf("%s: Failed (%X)\n", title, mbx); \
	} \
}

int main(int argc, char **argv) {
	SceUID mbx;

	CREATE_TEST("NULL name", NULL, 0, NULL);
	CREATE_TEST("Blank name", "", 0, NULL);
	CREATE_TEST("Long name", "1234567890123456789012345678901234567890123456789012345678901234", 0, NULL);
	CREATE_TEST("Weird attr", "create", 1, NULL);
	CREATE_TEST("0x10 attr", "create", 0x10, NULL);
	CREATE_TEST("0x100 attr", "create", 0x100, NULL);
	CREATE_TEST("0x122 attr", "create", 0x122, NULL);
	CREATE_TEST("0x200 attr", "create", 0x200, NULL);
	CREATE_TEST("0x300 attr", "create", 0x300, NULL);
	CREATE_TEST("0x400 attr", "create", 0x400, NULL);
	CREATE_TEST("0x422 attr", "create", 0x422, NULL);
	CREATE_TEST("0x900 attr", "create", 0x900, NULL);
	CREATE_TEST("0x1200 attr", "create", 0x1200, NULL);

	SceUID mbx1 = sceKernelCreateMbx("create", 0, NULL);
	SceUID mbx2 = sceKernelCreateMbx("create", 0, NULL);
	PRINT_MBX(mbx1);
	PRINT_MBX(mbx2);
	sceKernelDeleteMbx(mbx1);
	sceKernelDeleteMbx(mbx2);

	BASIC_SCHED_TEST("NULL name",
		mbx = sceKernelCreateMbx(NULL, 0, NULL);
		result = mbx > 0 ? 1 : mbx;
	);
	BASIC_SCHED_TEST("Normal",
		mbx = sceKernelCreateMbx("create2", 0, NULL);
		result = mbx > 0 ? 1 : mbx;
	);
	sceKernelDeleteMbx(mbx);

	SceUID mbxs[1024];
	int i, result = 0;
	for (i = 0; i < 1024; i++)
	{
		mbxs[i] = sceKernelCreateMbx("create", 0, NULL);
		if (mbxs[i] < 0)
		{
			result = mbxs[i];
			break;
		}
	}

	if (result != 0)
		printf("Create 1024: Failed at %d (%08X)\n", i, result);
	else
		printf("Create 1024: OK\n");

	while (--i >= 0)
		sceKernelDeleteMbx(mbxs[i]);

	return 0;
}