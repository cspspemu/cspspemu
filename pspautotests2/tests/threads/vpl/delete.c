#include "shared.h"

void testDelete(const char *title, SceUID vpl) {
	int result = sceKernelDeleteVpl(vpl);
	if (result == 0) {
		schedf("%s: OK\n", title);
	} else {
		schedf("%s: Failed (%X)\n", title, result);
	}
}

int main(int argc, char **argv) {
	SceUID vpl = sceKernelCreateVpl("vpl", PSP_MEMORY_PARTITION_USER, 0, 0x10000, NULL);

	testDelete("Normal", vpl);
	testDelete("NULL", 0);
	testDelete("Invalid", 0xDEADBEEF);
	testDelete("Deleted", vpl);
	
	BASIC_SCHED_TEST("Delete other",
		result = sceKernelDeleteVpl(vpl2);
	);
	BASIC_SCHED_TEST("Delete same",
		result = sceKernelDeleteVpl(vpl1);
	);
	BASIC_SCHED_TEST("NULL",
		result = sceKernelDeleteVpl(0);
	);

	return 0;
	
}