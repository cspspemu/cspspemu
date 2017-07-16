#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

int main(int argc, char **argv) {
	SceUID sema;
	
	sema = sceKernelCreateSema("sema1", 0, 0, 2, NULL);
	printf("sema > 0 : %s\n", (sema > 0) ? "ok" : "error");

	return 0;
}