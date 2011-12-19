#include <common.h>

#include <pspkernel.h>
#include <pspsysmem.h>
#include <pspthreadman.h>

int testThreadStackFillsWith_0xFF_AndIsAlignedTo_0x10__thread(SceSize arglen, void *argp) {
	int n, FFLen = 0;
	SceUID thid = sceKernelGetThreadId();
	SceKernelThreadInfo info;
	sceKernelReferThreadStatus(thid, &info);
	//printf("StackStart: %08X\n", ((unsigned int)info.stack)/* - 0x08900000*/);
	printf("StackSize   : %08X\n", info.stackSize);
	for (n = 0; n < info.stackSize; n++) {
		if (((unsigned char *)info.stack)[-n] == 0xFF) FFLen++;
	}
	printf("FFLen       : %08X\n", FFLen);
	printf("Aligned_0x10: %s\n"  , (((unsigned int)info.stack) & 0xF) ? "yes" : "no");
	return 0;
}

void testThreadStackFillsWith_0xFF_AndIsAlignedTo_0x10() {
	printf("testThreadStackFillsWith_0xFF_AndIsAlignedTo_0x10:\n");
	SceUID thid = sceKernelCreateThread("my_thread", testThreadStackFillsWith_0xFF_AndIsAlignedTo_0x10__thread, 0x18, 0x201, 0, NULL);
	sceKernelStartThread(thid, 0, NULL);
	sceKernelWaitThreadEnd(thid, NULL);
}

int main(int argc, char **argv) {
	testThreadStackFillsWith_0xFF_AndIsAlignedTo_0x10();
	
	return 0;
}
