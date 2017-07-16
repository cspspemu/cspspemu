#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

#define byte unsigned char

typedef struct {
	// +0000
	int unk[48];
	// +00C0
	SceUID threadId;
	// +00C4
	uint unk1;
	// +00C8
	void* stackAddr;
	// +00CC
	int unk3[11];
	// +00F8
	int f1;
	// +00FC
	int f2;
} K0STRUCT;

K0STRUCT* get_k0()
{
	uint ret = 0;
	asm volatile (
		"addi %0, $k0, 0\n"
		: "=r"(ret)
	);
	
	return (K0STRUCT *)ret;
}

/*
unsigned int fixed_ror(unsigned int value) {
	int ret;
	asm volatile (
		"ror %0, $a0, 4\n"
		: "=r"(ret)
	);
	return ret;
}
*/

void threadFunction(int argSize, void* argPointer)
{
	SceKernelThreadInfo th_stat;
	K0STRUCT* k0 = get_k0();
	
	memset(&th_stat, 0, sizeof(th_stat));
	th_stat.size = sizeof(th_stat);
	sceKernelReferThreadStatus(-1, &th_stat);
	
	printf("K0: %d\n", ((byte*)k0 - (byte*)k0->stackAddr));
	fflush(stdout);
	printf("Low:%d\n", ((byte*)th_stat.stack == (byte*)k0->stackAddr));
	fflush(stdout);
	printf("ThreadID[1]: %d\n", (uint)(k0->threadId == sceKernelGetThreadId()));
	fflush(stdout);
	printf("ThreadID[2]: %d\n", (uint)(*(SceUID *)(k0->stackAddr) == sceKernelGetThreadId()));
	fflush(stdout);
	printf("%08X\n", (uint)k0->f1);
	fflush(stdout);
	printf("%08X\n", (uint)k0->f2);
	fflush(stdout);
}

int main(int argc, char **argv) {
	int thid = 0;
	sceKernelStartThread(
		thid = sceKernelCreateThread("Test Thread", (void *)&threadFunction, 0x12, 0x10000, 0, NULL),
		0, NULL
	);
	
	sceKernelWaitThreadEnd(thid, NULL);

	return 0;
}
