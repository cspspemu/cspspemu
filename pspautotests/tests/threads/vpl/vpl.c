#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

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
	vpl = sceKernelCreateVpl("VPL", 2, 0, 1300, NULL);
	printf("%08X\n", vpl);
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

int main(int argc, char **argv) {
	testSimpleVpl();

	return 0;
}