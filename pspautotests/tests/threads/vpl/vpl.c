#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

#define PSP_VPL_ATTR_NONE 0 

void *pointer_base = NULL;

int pointer_relative_to_base(void *pointer) {
	if (pointer == NULL) return -1;
	return (int)pointer - (int)pointer_base;
}

void testSimpleVpl() {
	int vpl = -1;
	int result = -1;
	void *pointer1 = NULL;
	void *pointer2 = NULL;
	void *pointer3 = NULL;
	vpl = sceKernelCreateVpl("VPL", PSP_MEMORY_PARTITION_USER, PSP_VPL_ATTR_NONE, 1300, NULL);
	printf("%08X\n", vpl > 0 ? 1 : vpl);
	{
		result = sceKernelTryAllocateVpl(vpl, 1, &pointer_base);
		printf("%08X\n", result);
		sceKernelFreeVpl(vpl, pointer_base);
	
		result = sceKernelTryAllocateVpl(vpl, 300, &pointer1);
		printf("%08X: %08X\n", result, pointer_relative_to_base(pointer1));
		
		result = sceKernelTryAllocateVpl(vpl, 600, &pointer2);
		printf("%08X: %08X\n", result, pointer_relative_to_base(pointer2));
		
		result = sceKernelTryAllocateVpl(vpl, 900, &pointer3);
		printf("%08X: %08X\n", result, pointer_relative_to_base(pointer3));
		
		sceKernelFreeVpl(vpl, pointer1);

		result = sceKernelTryAllocateVpl(vpl, 900, &pointer3);
		printf("%08X: %08X\n", result, pointer_relative_to_base(pointer3));

		sceKernelFreeVpl(vpl, pointer2);

		result = sceKernelTryAllocateVpl(vpl, 900, &pointer3);
		printf("%08X: %08X\n", result, pointer_relative_to_base(pointer3));
	}
	sceKernelDeleteVpl(vpl);
}

int threadid;
int vpl;
void* ptr1;
void* ptr2;

int threadFunction(int argsize, void *argdata) {
	sceKernelDelayThread(10000);
	printf("sceKernelFreeVpl: 0x%08X\n", sceKernelFreeVpl(vpl, ptr1));

	return 0;
}

void testMultiVpl() {
	vpl = sceKernelCreateVpl("VPL", PSP_MEMORY_PARTITION_USER, PSP_VPL_ATTR_NONE, 1024, NULL);
	printf("sceKernelAllocateVpl: 0x%08X\n", sceKernelAllocateVpl(vpl, 512, &ptr1, NULL));
	{
		sceKernelStartThread(threadid = sceKernelCreateThread("thread", (void *)&threadFunction, 0x12, 0x10000, 0, NULL), 0, NULL);
	}
	printf("sceKernelAllocateVpl: 0x%08X\n", sceKernelAllocateVpl(vpl, 1024, &ptr2, NULL));
}

int main(int argc, char **argv) {
	testSimpleVpl();
	testMultiVpl();

	return 0;
}