#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

typedef struct {
	int a[128];
} TestStruct;

void testSimpleFpl() {
	int n;
	SceUID fpl;
	TestStruct *item;
	TestStruct *items[3];
	int fplCount = 2;
	int testBlockId;
	uint base = 0;
	
	printf("testSimpleFpl:\n");

	testBlockId = sceKernelAllocPartitionMemory(2, "TEST", PSP_SMEM_Low, fplCount * sizeof(TestStruct), 0);
	base = (uint)sceKernelGetBlockHeadAddr(testBlockId);
	sceKernelFreePartitionMemory(testBlockId);

	fpl = sceKernelCreateFpl("FPL", 2, 0, sizeof(TestStruct), fplCount, NULL);
	for (n = 0; n < 3; n++) {
		items[n] = NULL;
		printf("[%d]:%08X: ", n, sceKernelTryAllocateFpl(fpl, (void **)&items[n]));
		if (items[n] == NULL) {
			printf("%08X\n", -1);
		} else {
			printf("%08X\n", (int)(items[n]) - base);
		}
	}
	
	printf("FREE:%08X\n", sceKernelFreeFpl(fpl, items[1]));
	
	item = NULL;
	printf("[%d]:%08X: ", 4, sceKernelTryAllocateFpl(fpl, (void **)&item));
	if (items[n] == NULL) {
		printf("%08X\n", -1);
	} else {
		printf("%08X\n", (int)(item) - base);
	}

	printf("FREE_INVALID:%08X\n", sceKernelFreeFpl(fpl, (void *)3));

	//printf("%08X\n", (int)base);
	
	sceKernelDeleteFpl(fpl);
}

int main(int argc, char **argv) {
	testSimpleFpl();

	return 0;
}