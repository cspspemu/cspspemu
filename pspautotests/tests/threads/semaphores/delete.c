#include "shared.h"

SETUP_SCHED_TEST;

#define DELETE_TEST(title, sema) { \
	int result = sceKernelDeleteSema(sema); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
}

int main(int argc, char **argv) {
	SceUID sema = sceKernelCreateSema("delete1", 0, 0, 1, NULL);

	DELETE_TEST("Normal", sema);
	DELETE_TEST("NULL", 0);
	DELETE_TEST("Invalid", 0xDEADBEEF);
	DELETE_TEST("Deleted", sema);
	
	BASIC_SCHED_TEST("Delete other",
		result = sceKernelDeleteSema(sema2);
	);
	BASIC_SCHED_TEST("Delete same",
		result = sceKernelDeleteSema(sema1);
	);
	BASIC_SCHED_TEST("NULL",
		result = sceKernelDeleteSema(0);
	);

	return 0;
	
}