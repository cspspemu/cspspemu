#include "shared.h"

#define DELETE_TEST(title, workarea) { \
	int result = sceKernelDeleteLwMutex(workarea); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
}

int main(int argc, char **argv) {
	SceLwMutexWorkarea workarea, workarea1, workarea2;

	sceKernelCreateLwMutex(&workarea, "delete", 0, 0, NULL);

	DELETE_TEST("Normal", &workarea);
	// Crash.
	//DELETE_TEST("NULL", 0);
	DELETE_TEST("Invalid", (void*) 0xDEADBEEF);
	DELETE_TEST("Deleted", &workarea);

	sceKernelCreateLwMutex(&workarea1, "delete", 0, 0, NULL);
	memcpy(&workarea2, &workarea1, sizeof(SceLwMutexWorkarea));
	sceKernelDeleteLwMutex(&workarea1);
	DELETE_TEST("Copy", &workarea2);

	FAKE_LWMUTEX(workarea, 0, 0);
	DELETE_TEST("Fake", &workarea2);

	BASIC_SCHED_TEST("Delete other",
		result = sceKernelDeleteLwMutex(&workarea2);
	);
	BASIC_SCHED_TEST("Delete same",
		result = sceKernelDeleteLwMutex(&workarea1);
	);
	BASIC_SCHED_TEST("Invalid",
		result = sceKernelDeleteLwMutex((void*) 0xDEADBEEF);
	);

	return 0;
}