#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

#define lengthof(v) (sizeof(v) / sizeof(v[0]))


//int bss[16 * 1024] __attribute__((section(".bss2"), used));
int bss[16 * 1024];

#define BSS_VALUE 0x13579BDF
#define FPL_VALUE 0x02468ACE

int main(int argc, char **argv) {
	int n;
	int fplid;
	int *fpl;
	int ko;
	
	fplid = sceKernelCreateFpl("BSS_NO_OVERLAP", 2, 0, sizeof(bss), 1, NULL);
	sceKernelAllocateFpl(fplid, (void**)&fpl, NULL);
	
	for (n = 0; n < lengthof(bss); n++) bss[n] = BSS_VALUE;
	for (n = 0; n < lengthof(bss); n++) fpl[n] = FPL_VALUE;

	ko = 0;
	for (n = 0; n < lengthof(bss); n++) {
		if (bss[n] != BSS_VALUE) ko++;
		if (fpl[n] != FPL_VALUE) ko++;
	}
	printf("KO: %d\n", ko);

	return 0;
}
