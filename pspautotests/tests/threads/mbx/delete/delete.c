#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define DELETE_TEST(title, mbx) { \
	int result = sceKernelDeleteMbx(mbx); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
}

int main(int argc, char **argv) {
	SceUID mbx = sceKernelCreateMbx("delete1", 0, NULL);

	DELETE_TEST("Normal", mbx);
	DELETE_TEST("NULL", 0);
	DELETE_TEST("Invalid", 0xDEADBEEF);
	DELETE_TEST("Deleted", mbx);

	BASIC_SCHED_TEST("Delete other",
		result = sceKernelDeleteMbx(mbx2);
	);
	BASIC_SCHED_TEST("Delete same",
		result = sceKernelDeleteMbx(mbx1);
	);
	BASIC_SCHED_TEST("NULL",
		result = sceKernelDeleteMbx(0);
	);

	return 0;
}