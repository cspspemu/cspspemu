#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

void testFirstUndefinedUids() {
	printf("%08X\n", sceKernelWaitEventFlag(1, 1, PSP_EVENT_WAITOR, NULL, NULL));
	printf("%08X\n", sceKernelWaitSema(1, 1, NULL));
}

int main(int argc, char **argv) {
	testFirstUndefinedUids();

	return 0;
}