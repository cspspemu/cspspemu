#include <common.h>

#include <pspkernel.h>
#include <oslib/oslib.h>

char dataSource[] = "Hello World. This is a test to check if sceDmacMemcpy works.";
char dataDest[sizeof(dataSource)] = {0};

void testMemcpy(const char *title, int (*memcpyFunc)(void *d, const void *s, unsigned int sz)) {
	char temp[256];

	snprintf(temp, sizeof(temp), "%s normal operation:", title);
	memset(dataDest, 0, sizeof(dataDest));
	checkpointNext(temp);
	checkpoint("  Source: %s", dataSource);
	checkpoint("  Before copy: %s", dataDest);

	// Warning: sceDmacMemcpy bypasses the CPU cache.
	sceKernelDcacheWritebackInvalidateRange(dataDest, sizeof(dataDest));
	memcpyFunc(dataDest, dataSource, sizeof(dataSource));

	checkpoint("  After copy: %s", dataDest);
	
	snprintf(temp, sizeof(temp), "%s errors:", title);
	checkpoint(temp);
	checkpoint("  Normal: %08x", memcpyFunc(dataDest, dataSource, 4));
	checkpoint("  NULL, 0 length: %08x", memcpyFunc(0, 0, 0));
	checkpoint("  NULL, with length: %08x", memcpyFunc(0, 0, 4));
	checkpoint("  0 length: %08x", memcpyFunc(dataDest, dataSource, 0));
	// Crashes.
	//checkpoint("  Same ptr: %08x", memcpyFunc(dataDest, dataDest, 4));
}

int dmacCopyFunc(SceSize argc, void *argp) {
	checkpoint("  * thread: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00100000));
}

int main(int argc, char *argv[]) {
	testMemcpy("sceDmacMemcpy", &sceDmacMemcpy);
	testMemcpy("sceDmacTryMemcpy", &sceDmacTryMemcpy);

	checkpointNext("Size of copy:");
	// Approximate speed: 225 MB/s.
	checkpoint("  sceDmacMemcpy 1MB: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00100000));
	checkpoint("  sceDmacTryMemcpy 1MB: %08x", sceDmacTryMemcpy((void*)0x04000000, (void*)0x04100000, 0x00100000));
	checkpoint("  sceDmacMemcpy 1KB: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00000400));
	checkpoint("  sceDmacMemcpy 512B: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00000200));
	// This is suspiciously consistent, but maybe it's just chance.
	checkpoint("  sceDmacMemcpy 272B: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00000110));
	checkpoint("  sceDmacMemcpy 271B: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x0000010F));
	checkpoint("  sceDmacMemcpy 257B: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00000101));
	checkpoint("  sceDmacMemcpy 256B: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00000100));
	
	checkpointNext("Concurrent copies:");
	SceUID copyThread = sceKernelCreateThread("dmac", &dmacCopyFunc, 0x10, 0x1000, 0, NULL);
	sceKernelStartThread(copyThread, 0, NULL);
	checkpoint("  sceDmacTryMemcpy 1MB: %08x", sceDmacTryMemcpy((void*)0x04000000, (void*)0x04100000, 0x00100000));
	checkpoint("  sceDmacMemcpy 1MB: %08x", sceDmacMemcpy((void*)0x04000000, (void*)0x04100000, 0x00100000));

	checkpointNext("memalign:");
	void *ptr;
	ptr = memalign(128 , 2048); checkpoint("%d", ((int)ptr) % 128);
	ptr = memalign(1024, 2048); checkpoint("%d", ((int)ptr) % 1024);
	//ptr = memalign(100 , 2048); checkpoint("%d", ((int)ptr) % 100);

	//checkpoint("%i bytes available", oslGetRamStatus().maxAvailable);

	return 0;
}