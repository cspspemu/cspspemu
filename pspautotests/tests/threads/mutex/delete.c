#include "shared.h"

inline void deleteTest(const char *title, SceUID mutex) {
	int result = sceKernelDeleteMutex(mutex);
	if (result == 0) {
		printf("%s: OK\n", title);
	} else {
		printf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceUID mutex = sceKernelCreateMutex("delete", 0, 0, NULL);

	deleteTest("Normal", mutex);
	deleteTest("NULL", 0);
	deleteTest("Invalid", 0xDEADBEEF);
	deleteTest("Deleted", mutex);

	BASIC_SCHED_TEST("Delete other",
		result = sceKernelDeleteMutex(mutex2);
	);
	BASIC_SCHED_TEST("Delete same",
		result = sceKernelDeleteMutex(mutex1);
	);
	BASIC_SCHED_TEST("NULL",
		result = sceKernelDeleteMutex(0);
	);

	return 0;
}