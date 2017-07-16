#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define DELETE_TEST(title, flag) { \
	int result = sceKernelDeleteEventFlag(flag); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
}

int main(int argc, char **argv) {
	SceUID flag = sceKernelCreateEventFlag("delete1", 0, 0, NULL);

	DELETE_TEST("Normal", flag);
	DELETE_TEST("NULL", 0);
	DELETE_TEST("Invalid", 0xDEADBEEF);
	DELETE_TEST("Deleted", flag);
	
	BASIC_SCHED_TEST("Delete other",
		result = sceKernelDeleteEventFlag(flag2);
	);
	BASIC_SCHED_TEST("Delete same",
		result = sceKernelDeleteEventFlag(flag1);
	);
	BASIC_SCHED_TEST("NULL",
		result = sceKernelDeleteEventFlag(0);
	);

	return 0;
	
}